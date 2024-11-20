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

    // �h���b�O���J�n�����Ƃ��ɌĂ΂��
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ���̈ʒu��ۑ�
        originalPosition = rectTransform.position;
        // �h���b�O���ɑ���UI�̉��ɍs���Ȃ��悤�ɓ����x��ύX
        canvasGroup.blocksRaycasts = false;
    }

    // �h���b�O���ɌĂ΂��
    public void OnDrag(PointerEventData eventData)
    {
        // �}�E�X�ɍ��킹�ăA�C�e�����ړ�
        rectTransform.position = Input.mousePosition;
    }

    // �h���b�O���I�������Ƃ��ɌĂ΂��
    public void OnEndDrag(PointerEventData eventData)
    {
        // �h���b�O���I�������ۂɉ����h���b�v�悪�Ȃ���Ό��̈ʒu�ɖ߂�
        canvasGroup.blocksRaycasts = true;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            rectTransform.position = originalPosition;
        }
    }
}
