using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManagerの参照
    [SerializeField] private GameManager.ResourceFishType fishType; // この魚の種類
    [SerializeField] private Canvas canvas; // UI用のCanvas
    [SerializeField] private GameObject fishPrefab; // 場に設置する魚のPrefab
    [SerializeField] private ParticleSystem objectStarPrefab; // スターエフェクトのプレハブ
    [SerializeField] private float yOffset = 2f; // 魚を設置するときのY座標オフセット

    private GameObject dragPreview; // ドラッグ中のプレビュー用オブジェクト
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // ドラッグ可能かどうか
    private bool isDragging = false; // ドラッグ中かどうかを管理するフラグ

    public void Start()
    {
        // GameManagerの自動取得
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Canvasの自動取得
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            Debug.Log(canvas != null ? "Canvasを自動取得しました。" : "Canvasが見つかりませんでした。");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 在庫確認
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("在庫がありません。ドラッグできません。");
            canDrag = false;
            return;
        }

        canDrag = true;
        isDragging = true;

        // ドラッグ中のプレビューオブジェクトを作成
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);

        // プレビューにImageコンポーネントを追加
        var image = dragPreview.AddComponent<Image>();
        var originalImage = GetComponent<Image>();
        image.sprite = originalImage.sprite;
        image.preserveAspect = true;

        // サイズを元のImageコンポーネントに合わせる
        RectTransform originalRect = GetComponent<RectTransform>();
        dragPreviewRectTransform = dragPreview.GetComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = originalRect.sizeDelta;

        // 半透明に設定
        image.color = new Color(1f, 1f, 1f, 0.7f); // 半透明
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false; // Raycastを無効化
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ドラッグが無効な場合は何もしない
        if (!canDrag || dragPreview == null)
            return;

        // ドラッグ中のプレビューオブジェクトをマウスに追従させる
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );
        dragPreviewRectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ドラッグが無効な場合は何もしない
        if (!canDrag || dragPreview == null || !isDragging)
            return;

        if (dragPreview != null)
        {
            Destroy(dragPreview); // プレビューオブジェクトを削除
            dragPreview = null;   // 参照をクリア
        }

        isDragging = false; // ドラッグ終了フラグをリセット

        // マウス位置を取得してワールド座標に変換
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // マップ上にヒットするかチェック
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.CompareTag("Map"))
            {
                if (gameManager.finventory[fishType] > 0)
                {
                    // ヒットした位置に魚を設置し、Yオフセットを加える
                    Vector3 spawnPosition = hit.point + Vector3.up * yOffset;
                    SpawnFishAt(spawnPosition);

                    // 在庫を減らしてUIを更新
                    gameManager.finventory[fishType]--;
                    gameManager.UpdateResourceUI();
                }
                else
                {
                    Debug.Log("在庫不足で設置できません。");
                }
            }
            else
            {
                Debug.Log("マップ以外の場所に設置しようとしています。");
            }
        }
    }

    private void SpawnFishAt(Vector3 position)
    {
        // 魚を場に生成
        Instantiate(fishPrefab, position, Quaternion.identity);

        // パーティクルを魚の位置で生成して再生
        if (objectStarPrefab != null)
        {
            // パーティクルをインスタンス化
            ParticleSystem particle = Instantiate(objectStarPrefab, position, Quaternion.identity);

            // パーティクルのシミュレーション空間を「World」に設定
            var mainModule = particle.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

            // パーティクル再生
            particle.Play();

            // 再生終了後にパーティクルを削除
            Destroy(particle.gameObject, particle.main.duration);
        }
        else
        {
            Debug.LogWarning("objectStarPrefab パーティクルプレハブが設定されていません！");
        }
    }
}
