using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ZoneType { Box, Customer, Fryer, Trash }
    public ZoneType zoneType;

    [Header("Referencias")]
    public OrderBox linkedBox;
    public CustomerOrder linkedCustomer;
    public Fryer linkedFryer;
    public BoxSpawner boxSpawner;

    [Header("Imagen de estado")]
    [Tooltip("Imagen a modificar. Si está vacío, se usa la del propio objeto.")]
    public Image targetImage;

    [Header("Sprites de estado")]
    public Sprite normalSprite;   // Frame 1 (por defecto)
    public Sprite hoverSprite;    // Frame 2 (cuando algo válido pasa por encima)

    private Image internalImage;  // respaldo si no se asigna manualmente

    void Start()
    {
        internalImage = GetComponent<Image>();
        if (targetImage == null) targetImage = internalImage;

        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;

        boxSpawner = GameObject.FindAnyObjectByType<BoxSpawner>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        GameObject draggedGO = eventData.pointerDrag;

        // ✅ Solo mostrar hover si el objeto arrastrado es válido para esta zona
        bool isValid = IsValidForZone(draggedGO);
        if (isValid && hoverSprite != null && targetImage != null)
        {
            targetImage.sprite = hoverSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null && normalSprite != null)
        {
            targetImage.sprite = normalSprite;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedGO = eventData.pointerDrag;
        Debug.Log("DROP");
        if (draggedGO == null) return;

        // Volvemos al sprite normal al soltar
        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;

        FoodItem food = draggedGO.GetComponent<FoodItem>();
        OrderBox box = draggedGO.GetComponentInChildren<OrderBox>();

        switch (zoneType)
        {
            case ZoneType.Box:
                if (linkedBox != null && food != null)
                {
                    linkedBox.AddItem(food);
                    food.transform.SetParent(linkedBox.transform, false);
                    Debug.Log($"📦 Agregado {food.foodName} a la caja.");
                }
                break;

            case ZoneType.Fryer:
                Debug.Log("FREIR");
                if (linkedFryer != null && food != null)
                {
                    if (linkedFryer.IsFree)
                    {
                        linkedFryer.PlaceItem(food);
                        Debug.Log($"🍞 {food.foodName} colocado en freidora.");
                    }
                    else
                    {
                        Debug.Log("⚠️ Freidora ocupada.");
                    }
                }
                break;

            case ZoneType.Customer:
                if (linkedCustomer != null && box != null)
                {
                    bool ok = linkedCustomer.ValidateOrder(box.GetItems());
                    if (ok)
                    {
                        Debug.Log("✅ Pedido correcto - cliente satisfecho");
                        Destroy(box.GetComponentInParent<DraggableBox>().gameObject);
                        boxSpawner.OnBoxDelivered();
                        linkedCustomer.OnOrderCompleted();
                    }
                    else
                    {
                        Debug.Log("❌ Pedido incorrecto");
                    }
                }
                break;

            case ZoneType.Trash:
                if (draggedGO.CompareTag("Caja"))
                {
                    Destroy(draggedGO);
                    boxSpawner.OnBoxDelivered();
                }
                else
                {
                    Destroy(draggedGO);
                }
                break;
        }
    }

    private bool IsValidForZone(GameObject draggedGO)
    {
        switch (zoneType)
        {
            case ZoneType.Box:
                return draggedGO.GetComponent<FoodItem>() != null;

            case ZoneType.Customer:
                return draggedGO.GetComponentInChildren<OrderBox>() != null;

            case ZoneType.Fryer:
                // 👇 Solo si hay un food y la freidora está libre
                return draggedGO.GetComponent<FoodItem>() != null &&
                       linkedFryer != null &&
                       linkedFryer.IsFree;

            case ZoneType.Trash:
                return draggedGO.CompareTag("Caja") || draggedGO.GetComponent<FoodItem>() != null;

            default:
                return false;
        }
    }
}
