using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManager�̎Q��
    [SerializeField] private GameManager.ResourceFishType fishType; // ���̋��̎��
    [SerializeField] private Canvas canvas; // UI�p��Canvas
    [SerializeField] private GameObject fishPrefab; // ��ɐݒu���鋛��Prefab

    private GameObject dragPreview; // �h���b�O���̃v���r���[�p�I�u�W�F�N�g
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // �h���b�O�\���ǂ����𔻒肷��t���O

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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

        // �v���r���[��Image�R���|�[�l���g��ǉ�
        var image = dragPreview.AddComponent<UnityEngine.UI.Image>();
        var originalImage = GetComponent<UnityEngine.UI.Image>();

        // �X�v���C�g���R�s�[
        image.sprite = originalImage.sprite;

        // �c������ێ�
        image.preserveAspect = true;

        // �T�C�Y������Image�R���|�[�l���g�ɍ��킹��
        RectTransform originalRect = GetComponent<RectTransform>();
        dragPreviewRectTransform = dragPreview.GetComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = originalRect.sizeDelta;

        // �������ɐݒ�
        image.color = new Color(1f, 1f, 1f, 0.7f); // �������ɐݒ�
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false; // Raycast�𖳌���
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

        if (dragPreview != null)
        {
            Destroy(dragPreview); // �v���r���[�I�u�W�F�N�g���폜
            dragPreview = null;   // �Q�Ƃ��N���A
        }

        // �h���b�v�ʒu���擾
        Vector3 mousePosition = Input.mousePosition;

        // ���[���h���W�ɕϊ�
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane + 10f)); // �K�؂�Z�ʒu��ݒ�

        // �݌ɂ�����ꍇ�ɋ��𐶐�
        if (gameManager.finventory[fishType] > 0)
        {
            SpawnFishAt(worldPosition);
            gameManager.finventory[fishType]--; // �݌ɂ����炷
            gameManager.UpdateResourceUI();
        }
        else
        {
            Debug.Log("�݌ɕs���Őݒu�ł��܂���");
        }

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
