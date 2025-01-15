using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManagerの参照
    [SerializeField] private GameManager.ResourceFishType fishType; // この魚の種類
    [SerializeField] private Canvas canvas; // UI用のCanvas
    [SerializeField] private GameObject fishPrefab; // 場に設置する魚のPrefab
    [SerializeField] private float yOffset = 2f; // 魚を少し上に配置するためのオフセット（デフォルト値2）

    private GameObject dragPreview; // ドラッグ中のプレビュー用オブジェクト
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // ドラッグ可能かどうかを判定するフラグ

    private bool isDragging = false; // ドラッグ中かどうかを管理するフラグ

    [SerializeField] public ParticleSystem objectStar;//設置星パーティクル



    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            Debug.Log(canvas != null ? "Canvasを自動取得しました。" : "Canvasが見つかりませんでした。");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 在庫を確認
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("在庫がありません。ドラッグできません。");
            canDrag = false; // ドラッグを無効化
            return;
        }
        canDrag = true; // ドラッグを有効化
        isDragging = true; // ドラッグ中フラグを設定

        // ドラッグ中に表示するプレビューオブジェクトを作成
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);
        Debug.Log($"Parent: {dragPreview.transform.parent.name}");


        // プレビューにImageコンポーネントを追加
        var image = dragPreview.AddComponent<UnityEngine.UI.Image>();
        // コンポーネント追加後にログを出力
        Debug.Log("Imageコンポーネントを追加しました。");
        var originalImage = GetComponent<UnityEngine.UI.Image>();

        // スプライトをコピー
        image.sprite = originalImage.sprite;

        // 縦横比を維持
        image.preserveAspect = true;

        // サイズを元のImageコンポーネントに合わせる
        RectTransform originalRect = GetComponent<RectTransform>();
        dragPreviewRectTransform = dragPreview.GetComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = originalRect.sizeDelta;

        // 半透明に設定
        image.color = new Color(1f, 1f, 1f, 0.7f); // 半透明に設定
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false; // Raycastを無効化
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ドラッグが無効な場合は何もしない
        if (!canDrag || dragPreview == null)
            return;

        // ドラッグ中にプレビューオブジェクトをマウスに追従
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
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

        // マウス位置を取得
        Vector3 mousePosition = Input.mousePosition;

        // ワールド座標に変換（Z軸をカメラからの距離で調整）
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane + 10f));

        // 3Dレイキャストを使用して、マップ上にドロップできる場所を確認
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            // レイキャストがヒットした場所がマップ上かどうかを確認
            if (hit.collider != null && hit.collider.CompareTag("Map"))
            {
                // マップ上にいる場合のみ生成
                if (gameManager.finventory[fishType] > 0)
                {
                    // 魚を設置する場所にZ軸を合わせ、少し上にオフセットを加える
                    Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + yOffset, hit.point.z);

                    SpawnFishAt(spawnPosition);  // 魚を設置
                    gameManager.finventory[fishType]--; // 在庫を減らす
                    gameManager.UpdateResourceUI();
                    // 在庫減少後、UIを更新するためにFishInventoryUIに通知
                    FishInventoryUIManager fishInventoryUI = GameObject.Find("FishInventoryUIManager").GetComponent<FishInventoryUIManager>();
                    if (fishInventoryUI != null)
                    {
                        fishInventoryUI.OnFishStockDecreased(fishType, 1); // 在庫が減った時の
                    }
                }
                else
                {
                    Debug.Log("在庫不足で設置できません");
                }
            }
            else
            {
                Debug.Log("マップの上でない場所に設置しようとしています");
            }
        }
    }

    private void SpawnFishAt(Vector3 position)
    {
        // 魚を場に生成
        Instantiate(fishPrefab, position, Quaternion.identity);
        objectStar.Play(); // パーティクルを再生
        Debug.Log($"{fishType} を設置しました！");
        // object star パーティクルを再生する
        if (objectStar != null)
        {
            
            Debug.Log("パーティクル再生");
        }
        else
        {
            Debug.LogWarning("objectStar パーティクルがアタッチされていません！");
        }
    }
}
