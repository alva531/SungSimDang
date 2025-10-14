using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform originalParent;
    private Vector2 originalAnchoredPos;
    private CanvasGroup canvasGroup;
    private RectTransform rect;

    private Fryer currentFryer;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    public void SetFryer(Fryer fryer)
    {
        currentFryer = fryer;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 🔹 si estaba en una freidora -> cancelar cocinado
        if (currentFryer != null)
        {
            currentFryer.InterruptCooking();
            currentFryer = null;
        }

        // 🔹 si estaba en una caja -> removerlo
        FoodItem fi = GetComponent<FoodItem>();
        if (fi != null && fi.currentBox != null)
        {
            fi.currentBox.RemoveItem(fi);
        }

        originalParent = transform.parent;
        originalAnchoredPos = rect.anchoredPosition;

        transform.SetParent(transform.root); // mover al tope del canvas
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 🔹 solo "Caja" vuelve al lugar original si nadie la acepta
        if (CompareTag("Caja") && transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            rect.anchoredPosition = originalAnchoredPos;
        }
    }
}
