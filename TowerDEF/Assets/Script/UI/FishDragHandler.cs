using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManager�̎Q��
    [SerializeField] private GameManager.ResourceFishType fishType; // ���̋��̎��
    [SerializeField] private Canvas canvas; // UI�p��Canvas
    [SerializeField] private GameObject fishPrefab; // ��ɐݒu���鋛��Prefab
    [SerializeField] private float yOffset = 2f; // ����������ɔz�u���邽�߂̃I�t�Z�b�g�i�f�t�H���g�l2�j

    private GameObject dragPreview; // �h���b�O���̃v���r���[�p�I�u�W�F�N�g
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // �h���b�O�\���ǂ����𔻒肷��t���O

    private bool isDragging = false; // �h���b�O�����ǂ������Ǘ�����t���O

    [SerializeField] public ParticleSystem objectStar;//�ݒu���p�[�e�B�N��



    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            Debug.Log(canvas != null ? "Canvas�������擾���܂����B" : "Canvas��������܂���ł����B");
        }
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
        isDragging = true; // �h���b�O���t���O��ݒ�

        // �h���b�O���ɕ\������v���r���[�I�u�W�F�N�g���쐬
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);
        Debug.Log($"Parent: {dragPreview.transform.parent.name}");


        // �v���r���[��Image�R���|�[�l���g��ǉ�
        var image = dragPreview.AddComponent<UnityEngine.UI.Image>();
        // �R���|�[�l���g�ǉ���Ƀ��O���o��
        Debug.Log("Image�R���|�[�l���g��ǉ����܂����B");
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
        if (!canDrag || dragPreview == null || !isDragging)
            return;

        if (dragPreview != null)
        {
            Destroy(dragPreview); // �v���r���[�I�u�W�F�N�g���폜
            dragPreview = null;   // �Q�Ƃ��N���A
        }

        isDragging = false; // �h���b�O�I���t���O�����Z�b�g

        // �}�E�X�ʒu���擾
        Vector3 mousePosition = Input.mousePosition;

        // ���[���h���W�ɕϊ��iZ�����J��������̋����Œ����j
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane + 10f));

        // 3D���C�L���X�g���g�p���āA�}�b�v��Ƀh���b�v�ł���ꏊ���m�F
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            // ���C�L���X�g���q�b�g�����ꏊ���}�b�v�ォ�ǂ������m�F
            if (hit.collider != null && hit.collider.CompareTag("Map"))
            {
                // �}�b�v��ɂ���ꍇ�̂ݐ���
                if (gameManager.finventory[fishType] > 0)
                {
                    // ����ݒu����ꏊ��Z�������킹�A������ɃI�t�Z�b�g��������
                    Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + yOffset, hit.point.z);

                    SpawnFishAt(spawnPosition);  // ����ݒu
                    gameManager.finventory[fishType]--; // �݌ɂ����炷
                    gameManager.UpdateResourceUI();
                    // �݌Ɍ�����AUI���X�V���邽�߂�FishInventoryUI�ɒʒm
                    FishInventoryUIManager fishInventoryUI = GameObject.Find("FishInventoryUIManager").GetComponent<FishInventoryUIManager>();
                    if (fishInventoryUI != null)
                    {
                        fishInventoryUI.OnFishStockDecreased(fishType, 1); // �݌ɂ�����������
                    }
                }
                else
                {
                    Debug.Log("�݌ɕs���Őݒu�ł��܂���");
                }
            }
            else
            {
                Debug.Log("�}�b�v�̏�łȂ��ꏊ�ɐݒu���悤�Ƃ��Ă��܂�");
            }
        }
    }

    private void SpawnFishAt(Vector3 position)
    {
        // ������ɐ���
        Instantiate(fishPrefab, position, Quaternion.identity);
        objectStar.Play(); // �p�[�e�B�N�����Đ�
        Debug.Log($"{fishType} ��ݒu���܂����I");
        // object star �p�[�e�B�N�����Đ�����
        if (objectStar != null)
        {
            
            Debug.Log("�p�[�e�B�N���Đ�");
        }
        else
        {
            Debug.LogWarning("objectStar �p�[�e�B�N�����A�^�b�`����Ă��܂���I");
        }
    }
}
