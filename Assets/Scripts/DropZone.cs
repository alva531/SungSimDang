using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public enum ZoneType { Box, Customer, Fryer, Trash }
    public ZoneType zoneType;

    // referencias que asignarás en el Inspector o por spawner
    public OrderBox linkedBox;          // si ZoneType == Box
    public CustomerOrder linkedCustomer;// si ZoneType == Customer
    public Fryer linkedFryer;          // si ZoneType == Fryer

    public BoxSpawner boxSpawner;

    void Start()
    {
        boxSpawner = GameObject.FindAnyObjectByType<BoxSpawner>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedGO = eventData.pointerDrag;
        Debug.Log("DROP");
        if (draggedGO == null) return;

        FoodItem food = draggedGO.GetComponent<FoodItem>();
        OrderBox box = draggedGO.GetComponent<OrderBox>();

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
                        Destroy(box.gameObject);
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
}
