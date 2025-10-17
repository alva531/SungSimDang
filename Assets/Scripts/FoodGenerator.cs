using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FoodGenerator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Generator settings")]
    public GameObject foodPrefab;
    public RectTransform spawnParent;
    public Vector2 foodSize = new Vector2(100, 100);

    private GameObject currentSpawned;
    private bool isDragging;
    private PointerEventData cachedEventData;
    private DraggableItem currentDraggable;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (foodPrefab == null || spawnParent == null) return;

        currentSpawned = Instantiate(foodPrefab, spawnParent);
        RectTransform rt = currentSpawned.GetComponent<RectTransform>();
        rt.sizeDelta = foodSize;

        RectTransform myRect = transform as RectTransform;
        rt.position = myRect != null ? myRect.TransformPoint(myRect.rect.center) : transform.position;
        rt.localScale = Vector3.one;

        var fi = currentSpawned.GetComponent<FoodItem>();
        if (fi != null && string.IsNullOrEmpty(fi.foodName))
            fi.foodName = currentSpawned.name.Replace("(Clone)", "").Trim();

        currentDraggable = currentSpawned.GetComponent<DraggableItem>();

        // 🔹 Asignar pointerDrag para DropZones
        cachedEventData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position,
            pointerPress = currentSpawned,
            pointerDrag = currentSpawned
        };

        isDragging = true;
        currentDraggable.OnBeginDrag(cachedEventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging && currentDraggable != null)
        {
            currentDraggable.OnEndDrag(eventData);

            // 🔹 disparar OnDrop manualmente
            if (cachedEventData.pointerEnter != null)
            {
                ExecuteEvents.Execute(
                    cachedEventData.pointerEnter,
                    cachedEventData,
                    ExecuteEvents.dropHandler
                );
            }
        }

        currentSpawned = null;
        currentDraggable = null;
        isDragging = false;
    }

    private void Update()
    {
        if (!isDragging || currentDraggable == null) return;

        Vector2 currentPointerPos;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            currentPointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            currentPointerPos = Mouse.current.position.ReadValue();
        else
            return;

        cachedEventData.position = currentPointerPos;

        currentDraggable.OnDrag(cachedEventData);

        // 🔹 Actualizar pointerEnter para que DropZones detecten desde el primer frame
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(cachedEventData, results);
        cachedEventData.pointerEnter = results.Count > 0 ? results[0].gameObject : null;
    }
}
