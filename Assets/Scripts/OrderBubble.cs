using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OrderBubble : MonoBehaviour
{
    public RectTransform container;    // Panel donde se instancian iconos
    public GameObject iconPrefab;      // Prefab UI Image (pequeño)

    private List<GameObject> spawned = new List<GameObject>();

    public void ShowOrderWithStates(List<(GameObject prefab, FoodState state)> foodRequests)
    {
        Clear();
        if (foodRequests == null) return;

        foreach (var req in foodRequests)
        {
            GameObject icon = Instantiate(iconPrefab, container);
            Image img = icon.GetComponent<Image>();

            if (img != null && req.prefab != null)
            {
                Sprite s = null;

                // Obtener sprite desde el FoodItem del prefab
                var fi = req.prefab.GetComponent<FoodItem>();
                if (fi != null)
                    s = fi.GetSpriteForState(req.state);

                // Fallback al sprite de la imagen del prefab
                if (s == null)
                {
                    var prefabImg = req.prefab.GetComponent<Image>();
                    if (prefabImg != null) s = prefabImg.sprite;
                }

                if (s != null) img.sprite = s;
            }

            // 🔧 Desactivar cualquier Draggable o Animator del ícono (no del prefab)
            var drag = icon.GetComponent<DraggableItem>();
            if (drag != null) drag.enabled = false;

            var anim = icon.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;

            spawned.Add(icon);
        }
    }
    
    public void Clear()
    {
        for (int i = spawned.Count - 1; i >= 0; i--) Destroy(spawned[i]);
        spawned.Clear();
    }
}
