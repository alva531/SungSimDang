using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodGenerator : MonoBehaviour, IPointerDownHandler
{
    [Header("Generator settings")]
    public GameObject foodPrefab;      // Prefab UI (Image) que contiene FoodItem + DraggableItem
    public RectTransform spawnParent;  // Panel/Canvas donde instanciar

    [Header("Food Size")]
    public Vector2 foodSize = new Vector2(100, 100); // ancho x alto en píxeles

    public void OnPointerDown(PointerEventData eventData)
    {
        if (foodPrefab == null || spawnParent == null) return;

        // Instanciar
        GameObject go = Instantiate(foodPrefab, spawnParent);
        RectTransform rt = go.GetComponent<RectTransform>();

        // Ajustar tamaño
        rt.sizeDelta = foodSize;

        // Posicionar sobre el generador (asumiendo que este objeto es UI y en el mismo Canvas)
        RectTransform myRect = transform as RectTransform;
        if (myRect != null)
        {
            // convertir posición local del generador en espacio del spawnParent
            Vector3 worldPos = myRect.TransformPoint(myRect.rect.center);
            rt.position = worldPos;
        }
        else
        {
            rt.position = transform.position;
        }

        // resetear escala
        rt.localScale = Vector3.one;

        // opcional: inicializar nombre
        var fi = go.GetComponent<FoodItem>();
        if (fi != null && string.IsNullOrEmpty(fi.foodName))
        {
            fi.foodName = go.name.Replace("(Clone)", "").Trim();
        }
    }
}
