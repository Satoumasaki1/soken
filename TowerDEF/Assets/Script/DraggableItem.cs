using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    // ドラッグを開始したときに呼ばれる
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 元の位置を保存
        originalPosition = rectTransform.position;
        // ドラッグ中に他のUIの下に行かないように透明度を変更
        canvasGroup.blocksRaycasts = false;
    }

    // ドラッグ中に呼ばれる
    public void OnDrag(PointerEventData eventData)
    {
        // マウスに合わせてアイテムを移動
        rectTransform.position = Input.mousePosition;
    }

    // ドラッグを終了したときに呼ばれる
    public void OnEndDrag(PointerEventData eventData)
    {
        // ドラッグを終了した際に何もドロップ先がなければ元の位置に戻す
        canvasGroup.blocksRaycasts = true;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            rectTransform.position = originalPosition;
        }
    }
}
