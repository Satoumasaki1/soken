using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // �h���b�v���ꂽ�A�C�e�����擾
        GameObject droppedItem = eventData.pointerDrag;

        if (droppedItem != null)
        {
            // �����ŃA�C�e�����������h���b�v���ꂽ���̏������L�q����
            Debug.Log("Item dropped: " + droppedItem.name);
            // �h���b�v��̈ʒu�Ɉړ�
            droppedItem.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }
}
