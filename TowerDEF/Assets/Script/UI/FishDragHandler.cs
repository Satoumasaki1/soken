using UnityEngine;
using UnityEngine.EventSystems;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManager�̎Q��
    [SerializeField] private GameManager.ResourceFishType fishType; // ���̋��̎��
    [SerializeField] private Canvas canvas; // UI�p��Canvas
    [SerializeField] private GameObject fishPrefab; // ��ɐݒu���鋛��Prefab

    private GameObject dragPreview; // �h���b�O���̃v���r���[�p�I�u�W�F�N�g
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // �h���b�O�\���ǂ����𔻒肷��t���O

    private void Awake()
    {
        // �����������Ȃǂ�����΂����ɋL�q
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // �݌ɂ��m�F
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("�݌ɂ�����܂���B�h���b�O�ł��܂���B");
            canDrag = false; // �h���b�O�𖳌���
            return;
        }

        canDrag = true; // �h���b�O��L����

        // �h���b�O���ɕ\������v���r���[�I�u�W�F�N�g���쐬
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false; // Raycast�𖳌���
        dragPreviewRectTransform = dragPreview.AddComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;

        // �v���r���[�ɋ��̉摜��ݒ�
        var image = dragPreview.AddComponent<UnityEngine.UI.Image>();
        image.sprite = GetComponent<UnityEngine.UI.Image>().sprite;
        image.color = new Color(1f, 1f, 1f, 0.7f); // �������ɐݒ�
    }

    public void OnDrag(PointerEventData eventData)
    {
        // �h���b�O�������ȏꍇ�͉������Ȃ�
        if (!canDrag || dragPreview == null)
            return;

        // �h���b�O���Ƀv���r���[�I�u�W�F�N�g���}�E�X�ɒǏ]
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
        // �h���b�O�������ȏꍇ�͉������Ȃ�
        if (!canDrag || dragPreview == null)
            return;

        // �h���b�v�ʒu���擾
        Vector3 mousePosition = Input.mousePosition;

        // ���[���h���W�ɕϊ�
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane + 10f)); // �K�؂�Z�ʒu��ݒ�

        // �݌ɂ�����ꍇ�ɋ��𐶐�
        if (gameManager.finventory[fishType] > 0)
        {
            SpawnFishAt(worldPosition);
            gameManager.finventory[fishType]--; // �݌ɂ����炷
        }
        else
        {
            Debug.Log("�݌ɕs���Őݒu�ł��܂���");
        }

        // �v���r���[�I�u�W�F�N�g���폜
        Destroy(dragPreview);
    }

    private void SpawnFishAt(Vector3 position)
    {
        // Z�ʒu�𒲐��i���ʏ�ɐݒu����ꍇ�j
        position.z = 0;

        // ������ɐ���
        Instantiate(fishPrefab, position, Quaternion.identity);
        Debug.Log($"{fishType} ��ݒu���܂����I");
    }
}
