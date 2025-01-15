using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager; // GameManager�̎Q��
    [SerializeField] private GameManager.ResourceFishType fishType; // ���̋��̎��
    [SerializeField] private Canvas canvas; // UI�p��Canvas
    [SerializeField] private GameObject fishPrefab; // ��ɐݒu���鋛��Prefab
    [SerializeField] private ParticleSystem objectStarPrefab; // �p�[�e�B�N���V�X�e���̃v���n�u
    [SerializeField] private float yOffset = 2f; // ����������ɔz�u���邽�߂̃I�t�Z�b�g�i�f�t�H���g�l2�j

    private GameObject dragPreview; // �h���b�O���̃v���r���[�p�I�u�W�F�N�g
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true; // �h���b�O�\���ǂ����𔻒肷��t���O
    private bool isDragging = false; // �h���b�O�����ǂ������Ǘ�����t���O

    public void Start()
    {
        // GameManager�������擾
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Canvas�������擾
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("�݌ɂ�����܂���B�h���b�O�ł��܂���B");
            canDrag = false;
            return;
        }

        canDrag = true;
        isDragging = true;

        // �h���b�O���ɕ\������v���r���[�I�u�W�F�N�g���쐬
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
                    // ����ݒu����ʒu���v�Z
                    Vector3 spawnPosition = hit.point + Vector3.up * yOffset;

                    // ���𐶐����ăp�[�e�B�N�����Đ�
                    SpawnFishAt(spawnPosition);

                    // �݌ɂ����炵�AUI���X�V
                    gameManager.finventory[fishType]--;
                    gameManager.UpdateResourceUI();
                }
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
            ParticleSystem particle = Instantiate(objectStarPrefab, position, Quaternion.identity);

            // �p�[�e�B�N���̃V�~�����[�V������Ԃ��uWorld�v�ɐݒ�
            var mainModule = particle.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

            // �p�[�e�B�N���Đ�
            particle.Play();

            // �p�[�e�B�N���̍Đ��I����Ɏ����폜
            Destroy(particle.gameObject, particle.main.duration);
        }
        else
        {
            Debug.LogWarning("objectStarPrefab �p�[�e�B�N���v���n�u���ݒ肳��Ă��܂���I");
        }
    }
}
