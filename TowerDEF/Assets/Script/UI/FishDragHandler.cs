using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManagerの参照
    [SerializeField] private GameManager.ResourceFishType fishType; // この魚の種類
    [SerializeField] private Canvas canvas; // UI用のCanvas
    [SerializeField] private GameObject fishPrefab; // 場に設置する魚のPrefab
    [SerializeField] private ParticleSystem objectStarPrefab; // パーティクルシステムのプレハブ
    [SerializeField] private float yOffset = 2f; // 魚を少し上に配置するためのオフセット（デフォルト値2）

    private GameObject dragPreview; // ドラッグ中のプレビュー用オブジェクト
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // ドラッグ可能かどうかを判定するフラグ
    private bool isDragging = false; // ドラッグ中かどうかを管理するフラグ

    public void Start()
    {
        // GameManagerを自動取得
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Canvasを自動取得
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("在庫がありません。ドラッグできません。");
            canDrag = false;
            return;
        }

        canDrag = true;
        isDragging = true;

        // ドラッグ中に表示するプレビューオブジェクトを作成
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);

        var image = dragPreview.AddComponent<Image>();
        var originalImage = GetComponent<Image>();
        image.sprite = originalImage.sprite;
        image.preserveAspect = true;

        RectTransform originalRect = GetComponent<RectTransform>();
        dragPreviewRectTransform = dragPreview.GetComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = originalRect.sizeDelta;

        image.color = new Color(1f, 1f, 1f, 0.7f);
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || dragPreview == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );
        dragPreviewRectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag || dragPreview == null || !isDragging) return;

        Destroy(dragPreview);
        dragPreview = null;
        isDragging = false;

        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.CompareTag("Map"))
            {
                if (gameManager.finventory[fishType] > 0)
                {
                    // 魚を設置する位置を計算
                    Vector3 spawnPosition = hit.point + Vector3.up * yOffset;

                    // 魚を生成してパーティクルを再生
                    SpawnFishAt(spawnPosition);

                    // 在庫を減らし、UIを更新
                    gameManager.finventory[fishType]--;
                    gameManager.UpdateResourceUI();
                }
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
            ParticleSystem particle = Instantiate(objectStarPrefab, position, Quaternion.identity);

            // パーティクルのシミュレーション空間を「World」に設定
            var mainModule = particle.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

            // パーティクル再生
            particle.Play();

            // パーティクルの再生終了後に自動削除
            Destroy(particle.gameObject, particle.main.duration);
        }
        else
        {
            Debug.LogWarning("objectStarPrefab パーティクルプレハブが設定されていません！");
        }
    }
}
