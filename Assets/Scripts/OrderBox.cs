using System.Collections.Generic;
using UnityEngine;

public class OrderBox : MonoBehaviour
{
    [Header("Order box")]
    public List<FoodItem> collectedItems = new List<FoodItem>();
    public int maxItems = 3;

    void Update()
    {
        if (collectedItems.Count >= 3)
        {
            GetComponent<DropZone>().enabled = false;
        }
    }

    /// <summary>
    /// Añade un FoodItem a la caja.
    /// </summary>
    /// <param name="food"></param>
    public void AddItem(FoodItem food)
    {
        if (food == null) return;

        // Si la caja está llena, no aceptar más
        if (collectedItems.Count >= maxItems)
        {
            Debug.Log($"🚫 Caja llena: no se puede agregar {food.foodName}");
            return;
        }

        // Si estaba en otra caja, lo quitamos
        OrderBox oldBox = food.currentBox;
        if (oldBox != null && oldBox != this)
        {
            oldBox.RemoveItem(food);
        }

        // Añadir
        collectedItems.Add(food);
        food.transform.SetParent(transform, false);
        food.currentBox = this;

        var anim = food.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Bag");

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
            food.GetComponent<Animator>().SetTrigger("Base");
            Debug.Log($"📤 Caja: removido {food.foodName} (Estado: {food.currentState})");

            if (GetComponent<DropZone>().enabled == false)
            {
                GetComponent<DropZone>().enabled = true;
            }
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
