using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FoodGenerator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Generator settings")]
    public GameObject foodPrefab;      // Prefab del objeto arrastrable (FoodItem + DraggableItem)
    public RectTransform spawnParent;  // Canvas o panel donde se instancian los ítems
    public Vector2 foodSize = new Vector2(100, 100);

    private GameObject currentSpawned;
    private bool isDragging;
    private PointerEventData cachedEventData;
    private DraggableItem currentDraggable;

    // guardamos el GameObject que actualmente tiene "pointerEnter" (para poder despachar exit/enter)
    private GameObject lastPointerEnter;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (foodPrefab == null || spawnParent == null) return;

        // Instanciar el nuevo ítem
        currentSpawned = Instantiate(foodPrefab, spawnParent);
        RectTransform rt = currentSpawned.GetComponent<RectTransform>();
        rt.sizeDelta = foodSize;
        rt.localScale = Vector3.one;

        // Posicionar en el centro del generador
        RectTransform myRect = transform as RectTransform;
        rt.position = myRect != null ? myRect.TransformPoint(myRect.rect.center) : transform.position;

        // Asignar nombre si no tiene
        var fi = currentSpawned.GetComponent<FoodItem>();
        if (fi != null && string.IsNullOrEmpty(fi.foodName))
            fi.foodName = currentSpawned.name.Replace("(Clone)", "").Trim();

        // Obtener el componente draggable
        currentDraggable = currentSpawned.GetComponent<DraggableItem>();

        // Crear PointerEventData y registrar correctamente el objeto
        cachedEventData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position,
            pressPosition = eventData.position,
            pointerPress = currentSpawned,
            pointerDrag = currentSpawned,
            // usa el mismo pointerId que vino en el eventData para compatibilidad (touch/mouse)
            pointerId = eventData.pointerId,
            // rawPointerPress y lastPress ayudan a que handlers que miran estas propiedades no fallen
            rawPointerPress = currentSpawned,
        };

        // marcar inicio de drag
        isDragging = true;
        lastPointerEnter = null;

        // llamar OnBeginDrag del draggable (tu script)
        currentDraggable.OnBeginDrag(cachedEventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging && currentDraggable != null)
        {
            // llamar OnEndDrag del draggable (tu script)
            currentDraggable.OnEndDrag(eventData);

            // Disparar OnDrop manualmente sobre el objeto bajo el puntero (si existe)
            if (cachedEventData != null && cachedEventData.pointerEnter != null)
            {
                ExecuteEvents.Execute(
                    cachedEventData.pointerEnter,
                    cachedEventData,
                    ExecuteEvents.dropHandler
                );
            }

            // Si quedó un hover, llamar exit
            if (lastPointerEnter != null)
            {
                ExecuteEvents.Execute(
                    lastPointerEnter,
                    cachedEventData,
                    ExecuteEvents.pointerExitHandler
                );
                lastPointerEnter = null;
            }
        }

        // Reset referencias
        currentSpawned = null;
        currentDraggable = null;
        isDragging = false;
        cachedEventData = null;
    }

    private void Update()
    {
        if (!isDragging || currentDraggable == null || cachedEventData == null || EventSystem.current == null) return;

        Vector2 currentPointerPos;

        // Compatibilidad con móvil y PC
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            currentPointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            currentPointerPos = Mouse.current.position.ReadValue();
        else
            return;

        // actualizar posición en eventData
        cachedEventData.position = currentPointerPos;

        // Actualizar la posición del ítem (tu OnDrag)
        currentDraggable.OnDrag(cachedEventData);

        // Raycast UI para saber qué objeto está debajo del puntero
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(cachedEventData, results);
        GameObject newEnter = results.Count > 0 ? results[0].gameObject : null;

        // Si cambió el objeto bajo el puntero, despachar exit/enter apropiados
        if (newEnter != lastPointerEnter)
        {
            // salir del anterior
            if (lastPointerEnter != null)
            {
                ExecuteEvents.Execute(
                    lastPointerEnter,
                    cachedEventData,
                    ExecuteEvents.pointerExitHandler
                );
            }

            // entrar al nuevo
            if (newEnter != null)
            {
                // importantísimo: actualizar pointerEnter y pointerDrag antes de ejecutar pointerEnter
                cachedEventData.pointerEnter = newEnter;
                cachedEventData.pointerDrag = currentSpawned;
                ExecuteEvents.Execute(
                    newEnter,
                    cachedEventData,
                    ExecuteEvents.pointerEnterHandler
                );
            }
            else
            {
                cachedEventData.pointerEnter = null;
            }

            lastPointerEnter = newEnter;
        }
        else
        {
            // si no cambió, aseguramos que pointerEnter esté correcto (por seguridad)
            cachedEventData.pointerEnter = newEnter;
            cachedEventData.pointerDrag = currentSpawned;
        }
    }
}