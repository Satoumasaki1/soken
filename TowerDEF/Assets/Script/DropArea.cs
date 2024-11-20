using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたアイテムを取得
        GameObject droppedItem = eventData.pointerDrag;

        if (droppedItem != null)
        {
            // ここでアイテムが正しくドロップされた時の処理を記述する
            Debug.Log("Item dropped: " + droppedItem.name);
            // ドロップ先の位置に移動
            droppedItem.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }
}
