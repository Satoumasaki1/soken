using UnityEngine;
using UnityEngine.EventSystems;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManagerの参照
    [SerializeField] private GameManager.ResourceFishType fishType; // この魚の種類
    [SerializeField] private Canvas canvas; // UI用のCanvas
    [SerializeField] private GameObject fishPrefab; // 場に設置する魚のPrefab

    private GameObject dragPreview; // ドラッグ中のプレビュー用オブジェクト
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // ドラッグ可能かどうかを判定するフラグ

    private void Awake()
    {
        // 初期化処理などがあればここに記述
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

        // ドラッグ中に表示するプレビューオブジェクトを作成
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false; // Raycastを無効化
        dragPreviewRectTransform = dragPreview.AddComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;

        // プレビューに魚の画像を設定
        var image = dragPreview.AddComponent<UnityEngine.UI.Image>();
        image.sprite = GetComponent<UnityEngine.UI.Image>().sprite;
        image.color = new Color(1f, 1f, 1f, 0.7f); // 半透明に設定
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
        if (!canDrag || dragPreview == null)
            return;

        // ドロップ位置を取得
        Vector3 mousePosition = Input.mousePosition;

        // ワールド座標に変換
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane + 10f)); // 適切なZ位置を設定

        // 在庫がある場合に魚を生成
        if (gameManager.finventory[fishType] > 0)
        {
            SpawnFishAt(worldPosition);
            gameManager.finventory[fishType]--; // 在庫を減らす
        }
        else
        {
            Debug.Log("在庫不足で設置できません");
        }

        // プレビューオブジェクトを削除
        Destroy(dragPreview);
    }

    private void SpawnFishAt(Vector3 position)
    {
        // Z位置を調整（平面上に設置する場合）
        position.z = 0;

        // 魚を場に生成
        Instantiate(fishPrefab, position, Quaternion.identity);
        Debug.Log($"{fishType} を設置しました！");
    }
}
