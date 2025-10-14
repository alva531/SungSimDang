using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FoodState
{
    Raw,
    SemiCooked,
    Cooked,
    Burnt
}

[System.Serializable]
public struct StateSprite
{
    public FoodState state;
    public Sprite sprite;
}

public class FoodItem : MonoBehaviour
{
    [Header("Info")]
    public string foodName;

    [Header("Legacy sprites (opcional)")]
    public Sprite rawSprite;
    public Sprite semiCookedSprite;
    public Sprite cookedSprite;
    public Sprite burntSprite;

    [Header("Sprites por estado (preferible)")]
    public List<StateSprite> stateSprites = new List<StateSprite>();

    [HideInInspector] public OrderBox currentBox;
    [HideInInspector] public FoodState currentState = FoodState.Raw;

    private Image image;
    private Dictionary<FoodState, Sprite> spriteLookup;

    private void Awake()
    {
        image = GetComponent<Image>();
        BuildLookup();
        UpdateVisual();
    }

    // Construye / rellena el diccionario desde stateSprites o desde los campos legacy
    private void BuildLookup()
    {
        if (spriteLookup == null) spriteLookup = new Dictionary<FoodState, Sprite>();
        spriteLookup.Clear();

        // Primero desde la lista serializable (si está)
        if (stateSprites != null)
        {
            foreach (var entry in stateSprites)
            {
                if (entry.sprite != null && !spriteLookup.ContainsKey(entry.state))
                    spriteLookup.Add(entry.state, entry.sprite);
            }
        }

        // Fallback a los campos legacy si faltan entradas
        if (!spriteLookup.ContainsKey(FoodState.Raw) && rawSprite != null)
            spriteLookup[FoodState.Raw] = rawSprite;
        if (!spriteLookup.ContainsKey(FoodState.SemiCooked) && semiCookedSprite != null)
            spriteLookup[FoodState.SemiCooked] = semiCookedSprite;
        if (!spriteLookup.ContainsKey(FoodState.Cooked) && cookedSprite != null)
            spriteLookup[FoodState.Cooked] = cookedSprite;
        if (!spriteLookup.ContainsKey(FoodState.Burnt) && burntSprite != null)
            spriteLookup[FoodState.Burnt] = burntSprite;
    }

    // Método público seguro: construye el lookup si hace falta (útil cuando llamas desde un prefab)
    public Sprite GetSpriteForState(FoodState state)
    {
        if (spriteLookup == null || spriteLookup.Count == 0)
            BuildLookup();

        spriteLookup.TryGetValue(state, out Sprite s);
        return s;
    }

    private void UpdateVisual()
    {
        if (image == null) return;
        var s = GetSpriteForState(currentState);
        if (s != null) image.sprite = s;
    }

    public void SemiCook()
    {
        if (currentState == FoodState.Raw)
        {
            currentState = FoodState.SemiCooked;
            Debug.Log($"🍳 {foodName} -> SemiCooked");
            UpdateVisual();
        }
    }

    public void Cook()
    {
        if (currentState == FoodState.SemiCooked)
        {
            currentState = FoodState.Cooked;
            Debug.Log($"🔥 {foodName} -> Cooked");
            UpdateVisual();
        }
    }

    public void Burn()
    {
        if (currentState == FoodState.Cooked)
        {
            currentState = FoodState.Burnt;
            Debug.Log($"💀 {foodName} -> Burnt");
            UpdateVisual();
        }
    }

    public void SetDraggable(bool draggable)
    {
        var drag = GetComponent<DraggableItem>();
        if (drag != null)
            drag.enabled = draggable;
    }
}
