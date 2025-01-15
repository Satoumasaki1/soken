using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManager�̎Q��
    [SerializeField] private GameManager.ResourceFishType fishType; // ���̋��̎��
    [SerializeField] private Canvas canvas; // UI�p��Canvas
    [SerializeField] private GameObject fishPrefab; // ��ɐݒu���鋛��Prefab
    [SerializeField] private ParticleSystem objectStarPrefab; // �X�^�[�G�t�F�N�g�̃v���n�u
    [SerializeField] private float yOffset = 2f; // ����ݒu����Ƃ���Y���W�I�t�Z�b�g

    private GameObject dragPreview; // �h���b�O���̃v���r���[�p�I�u�W�F�N�g
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // �h���b�O�\���ǂ���
    private bool isDragging = false; // �h���b�O�����ǂ������Ǘ�����t���O

    public void Start()
    {
        // GameManager�̎����擾
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Canvas�̎����擾
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            Debug.Log(canvas != null ? "Canvas�������擾���܂����B" : "Canvas��������܂���ł����B");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // �݌Ɋm�F
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("�݌ɂ�����܂���B�h���b�O�ł��܂���B");
            canDrag = false;
            return;
        }

        canDrag = true;
        isDragging = true;

        // �h���b�O���̃v���r���[�I�u�W�F�N�g���쐬
        dragPreview = new GameObject("DragPreview");
        dragPreview.transform.SetParent(canvas.transform, false);

        // �v���r���[��Image�R���|�[�l���g��ǉ�
        var image = dragPreview.AddComponent<Image>();
        var originalImage = GetComponent<Image>();
        image.sprite = originalImage.sprite;
        image.preserveAspect = true;

        // �T�C�Y������Image�R���|�[�l���g�ɍ��킹��
        RectTransform originalRect = GetComponent<RectTransform>();
        dragPreviewRectTransform = dragPreview.GetComponent<RectTransform>();
        dragPreviewRectTransform.sizeDelta = originalRect.sizeDelta;

        // �������ɐݒ�
        image.color = new Color(1f, 1f, 1f, 0.7f); // ������
        dragPreview.AddComponent<CanvasGroup>().blocksRaycasts = false; // Raycast�𖳌���
    }

    public void OnDrag(PointerEventData eventData)
    {
        // �h���b�O�������ȏꍇ�͉������Ȃ�
        if (!canDrag || dragPreview == null)
            return;

        // �h���b�O���̃v���r���[�I�u�W�F�N�g���}�E�X�ɒǏ]������
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
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

        // �}�E�X�ʒu���擾���ă��[���h���W�ɕϊ�
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // �}�b�v��Ƀq�b�g���邩�`�F�b�N
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.CompareTag("Map"))
            {
                if (gameManager.finventory[fishType] > 0)
                {
                    // �q�b�g�����ʒu�ɋ���ݒu���AY�I�t�Z�b�g��������
                    Vector3 spawnPosition = hit.point + Vector3.up * yOffset;
                    SpawnFishAt(spawnPosition);

                    // �݌ɂ����炵��UI���X�V
                    gameManager.finventory[fishType]--;
                    gameManager.UpdateResourceUI();
                }
                else
                {
                    Debug.Log("�݌ɕs���Őݒu�ł��܂���B");
                }
            }
            else
            {
                Debug.Log("�}�b�v�ȊO�̏ꏊ�ɐݒu���悤�Ƃ��Ă��܂��B");
            }
        }
    }

    private void SpawnFishAt(Vector3 position)
    {
        // ������ɐ���
        Instantiate(fishPrefab, position, Quaternion.identity);

        // �p�[�e�B�N�������̈ʒu�Ő������čĐ�
        if (objectStarPrefab != null)
        {
            // �p�[�e�B�N�����C���X�^���X��
            ParticleSystem particle = Instantiate(objectStarPrefab, position, Quaternion.identity);

            // �p�[�e�B�N���̃V�~�����[�V������Ԃ��uWorld�v�ɐݒ�
            var mainModule = particle.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

            // �p�[�e�B�N���Đ�
            particle.Play();

            // �Đ��I����Ƀp�[�e�B�N�����폜
            Destroy(particle.gameObject, particle.main.duration);
        }
        else
        {
            Debug.LogWarning("objectStarPrefab �p�[�e�B�N���v���n�u���ݒ肳��Ă��܂���I");
        }
    }
}
