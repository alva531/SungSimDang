using System.Collections.Generic;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    [Header("UI")]
    public OrderBubble bubble;

    // Guardamos nombre + estado requerido
    private List<(string foodName, FoodState requiredState)> requestedItems = new List<(string, FoodState)>();

    /// <summary>
    /// Configura el pedido del cliente.
    /// </summary>
    /// <param name="foodRequests">Lista de tuplas: prefab + estado deseado</param>
    public void SetOrder(List<(GameObject prefab, FoodState state)> foodRequests)
    {
        requestedItems.Clear();
        if (foodRequests == null) return;

        foreach (var req in foodRequests)
        {
            if (req.prefab == null) continue;

            // Tomamos el nombre definido en el prefab o en el FoodItem si existe
            string name;
            var foodItemComp = req.prefab.GetComponent<FoodItem>();
            if (foodItemComp != null && !string.IsNullOrEmpty(foodItemComp.foodName))
                name = foodItemComp.foodName;
            else
                name = req.prefab.name.Replace("(Clone)", "").Trim();

            requestedItems.Add((name, req.state));
        }

        // Mostrar en la burbuja UI
        bubble?.ShowOrderWithStates(foodRequests);
    }

    /// <summary>
    /// Valida si los items entregados cumplen el pedido exacto.
    /// </summary>
    /// <param name="deliveredItems">Lista de FoodItem entregados</param>
    /// <returns>true si todo coincide</returns>
    public bool ValidateOrder(List<FoodItem> deliveredItems)
    {
        if (deliveredItems == null) return false;
        if (deliveredItems.Count != requestedItems.Count) return false;

        // Hacemos copia para ir removiendo items correctos
        var pending = new List<(string foodName, FoodState requiredState)>(requestedItems);

        foreach (var delivered in deliveredItems)
        {
            if (delivered == null) return false;

            string dname = !string.IsNullOrEmpty(delivered.foodName)
                ? delivered.foodName
                : delivered.gameObject.name.Replace("(Clone)", "").Trim();

            FoodState dstate = delivered.currentState;

            // Buscamos match exacto
            int idx = pending.FindIndex(p => p.foodName == dname && p.requiredState == dstate);
            if (idx >= 0)
            {
                pending.RemoveAt(idx);
            }
            else
            {
                return false; // algo no coincide
            }
        }

        return pending.Count == 0;
    }

    /// <summary>
    /// Llama cuando el pedido fue completado correctamente.
    /// </summary>
    public void OnOrderCompleted()
    {
        bubble?.Clear();
        Debug.Log($"🧑‍🍳 Cliente satisfecho: {gameObject.name}");
        Destroy(gameObject, 0.1f);
    }
}
