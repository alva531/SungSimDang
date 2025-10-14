using System.Collections.Generic;
using UnityEngine;

public class OrderBox : MonoBehaviour
{
    [Header("Order box")]
    public List<FoodItem> collectedItems = new List<FoodItem>();
    public int maxItems = 3;

    /// <summary>
    /// Añade un FoodItem a la caja.
    /// </summary>
    /// <param name="food"></param>
    public void AddItem(FoodItem food)
    {
        if (food == null) return;

        // Si estaba en otra caja, lo quitamos
        OrderBox oldBox = food.currentBox;
        if (oldBox != null && oldBox != this)
        {
            oldBox.RemoveItem(food);
        }

        // Si estamos llenos, remover el primero
        if (collectedItems.Count >= maxItems)
        {
            FoodItem toRemove = collectedItems[0];
            RemoveItem(toRemove);
        }

        // Añadir
        collectedItems.Add(food);
        food.transform.SetParent(transform, false);
        food.currentBox = this;

        Debug.Log($"📥 Caja: agregado {food.foodName} (Estado: {food.currentState})");
    }

    /// <summary>
    /// Quita un FoodItem de la caja.
    /// </summary>
    /// <param name="food"></param>
    public void RemoveItem(FoodItem food)
    {
        if (food == null) return;
        if (collectedItems.Contains(food))
        {
            collectedItems.Remove(food);
            food.currentBox = null;
            Debug.Log($"📤 Caja: removido {food.foodName} (Estado: {food.currentState})");
        }
    }

    /// <summary>
    /// Devuelve una copia de los items actualmente en la caja.
    /// </summary>
    /// <returns></returns>
    public List<FoodItem> GetItems()
    {
        return new List<FoodItem>(collectedItems);
    }
}
