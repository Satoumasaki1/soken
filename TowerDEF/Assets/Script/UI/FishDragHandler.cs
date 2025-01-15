using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameManager.ResourceFishType fishType;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private ParticleSystem objectStarPrefab;
    [SerializeField] private float yOffset = 2f;

    private GameObject dragPreview;
    private RectTransform dragPreviewRectTransform;

    private bool canDrag = true;
    private bool isDragging = false;

    public void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gameManager.finventory[fishType] <= 0)
        {
            Debug.Log("在庫がありません。ドラッグできません。");
            canDrag = false;
            return;
        }

        canDrag = true;
        isDragging = true;

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
                    Vector3 spawnPosition = hit.point + Vector3.up * yOffset;
                    SpawnFishAt(spawnPosition);
                    gameManager.finventory[fishType]--;
                    gameManager.UpdateResourceUI();
                }
            }
        }
    }

    private void SpawnFishAt(Vector3 position)
    {
        Instantiate(fishPrefab, position, Quaternion.identity);

        if (objectStarPrefab != null)
        {
            ParticleSystem particle = Instantiate(objectStarPrefab, position, Quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, particle.main.duration);
        }
    }
}
