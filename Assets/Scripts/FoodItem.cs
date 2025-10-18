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

[System.Serializable]
public struct StateAnimOverride
{
    public FoodState state;
    public AnimatorOverrideController anim;
}

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Animator))]
public class FoodItem : MonoBehaviour
{
    [Header("Info")]
    public string foodName;

    [Header("Sprites")]
    public Sprite rawSprite;
    public Sprite semiCookedSprite;
    public Sprite cookedSprite;
    public Sprite burntSprite;

    [Header("Sprites por estado")]
    public List<StateSprite> stateSprites = new List<StateSprite>();

    [Header("Animators por estado")]
    public List<StateAnimOverride> stateAnim = new List<StateAnimOverride>();

    [HideInInspector] public OrderBox currentBox;
    [HideInInspector] public FoodState currentState = FoodState.Raw;

    private Image image;
    private Animator animator;

    private Dictionary<FoodState, Sprite> spriteLookup;
    private Dictionary<FoodState, AnimatorOverrideController> animLookup;

    private void Awake()
    {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        BuildLookups();
        UpdateVisual();
    }

    private void BuildLookups()
    {
        // Sprites
        if (spriteLookup == null) spriteLookup = new Dictionary<FoodState, Sprite>();
        spriteLookup.Clear();

        if (stateSprites != null)
        {
            foreach (var entry in stateSprites)
            {
                if (entry.sprite != null && !spriteLookup.ContainsKey(entry.state))
                    spriteLookup.Add(entry.state, entry.sprite);
            }
        }

        // Fallback a los campos legacy
        if (!spriteLookup.ContainsKey(FoodState.Raw) && rawSprite != null)
            spriteLookup[FoodState.Raw] = rawSprite;
        if (!spriteLookup.ContainsKey(FoodState.SemiCooked) && semiCookedSprite != null)
            spriteLookup[FoodState.SemiCooked] = semiCookedSprite;
        if (!spriteLookup.ContainsKey(FoodState.Cooked) && cookedSprite != null)
            spriteLookup[FoodState.Cooked] = cookedSprite;
        if (!spriteLookup.ContainsKey(FoodState.Burnt) && burntSprite != null)
            spriteLookup[FoodState.Burnt] = burntSprite;

        // Animadores
        if (animLookup == null) animLookup = new Dictionary<FoodState, AnimatorOverrideController>();
        animLookup.Clear();

        if (stateAnim != null)
        {
            foreach (var entry in stateAnim)
            {
                if (entry.anim != null && !animLookup.ContainsKey(entry.state))
                    animLookup.Add(entry.state, entry.anim);
            }
        }
    }

    public Sprite GetSpriteForState(FoodState state)
    {
        if (spriteLookup == null || spriteLookup.Count == 0)
            BuildLookups();

        spriteLookup.TryGetValue(state, out Sprite s);
        return s;
    }

    public AnimatorOverrideController GetAnimatorForState(FoodState state)
    {
        if (animLookup == null || animLookup.Count == 0)
            BuildLookups();

        animLookup.TryGetValue(state, out AnimatorOverrideController a);
        return a;
    }

    private void UpdateVisual()
    {
        if (image == null) return;
        var a = GetAnimatorForState(currentState);
        if (a != null && animator != null)
        {
            animator.runtimeAnimatorController = a;
        }
    }

    public void SemiCook()
    {
        if (currentState == FoodState.Raw)
        {
            currentState = FoodState.Cooked;
            Debug.Log($"🍳 {foodName} -> SemiCooked");
            UpdateVisual();
        }
    }

    public void Cook()
    {
        if (currentState == FoodState.Raw)
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
