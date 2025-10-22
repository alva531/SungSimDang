using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    [Header("UI")]
    public OrderBubble bubble;
    public DropZone dropZone;

    private List<(string foodName, FoodState requiredState)> requestedItems = new List<(string, FoodState)>();

    public event Action<CustomerOrder> onOrderCompleted;

    public void SetOrder(List<(GameObject prefab, FoodState state)> foodRequests)
    {
        requestedItems.Clear();
        if (foodRequests == null) return;

        foreach (var req in foodRequests)
        {
            if (req.prefab == null) continue;

            string name;
            var foodItemComp = req.prefab.GetComponent<FoodItem>();
            if (foodItemComp != null && !string.IsNullOrEmpty(foodItemComp.foodName))
                name = foodItemComp.foodName;
            else
                name = req.prefab.name.Replace("(Clone)", "").Trim();

            requestedItems.Add((name, req.state));
        }

        bubble?.ShowOrderWithStates(foodRequests);
    }

    public void ConfigureAppearance(int spawnIndex, Sprite normalSprite, Sprite hoverSprite)
    {
        if (dropZone == null)
            dropZone = GetComponentInChildren<DropZone>();

        if (dropZone != null)
        {
            dropZone.normalSprite = normalSprite;
            dropZone.hoverSprite = hoverSprite;
            if (dropZone.targetImage != null)
                dropZone.targetImage.sprite = normalSprite;
        }
    }

    public bool ValidateOrder(List<FoodItem> deliveredItems)
    {
        if (deliveredItems == null) return false;
        if (deliveredItems.Count != requestedItems.Count) return false;

        var pending = new List<(string foodName, FoodState requiredState)>(requestedItems);

        foreach (var delivered in deliveredItems)
        {
            if (delivered == null) return false;

            string dname = !string.IsNullOrEmpty(delivered.foodName)
                ? delivered.foodName
                : delivered.gameObject.name.Replace("(Clone)", "").Trim();

            FoodState dstate = delivered.currentState;

            int idx = pending.FindIndex(p => p.foodName == dname && p.requiredState == dstate);
            if (idx >= 0) pending.RemoveAt(idx);
            else return false;
        }

        return pending.Count == 0;
    }

    public void OnOrderCompleted()
    {
        bubble?.Clear();
        Debug.Log($"🧑‍🍳 Cliente satisfecho: {gameObject.name}");

        // 🔸 Notificar al spawner
        onOrderCompleted?.Invoke(this);

        Destroy(gameObject, 0.1f);
    }
}
