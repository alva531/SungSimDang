using System.Collections;
using UnityEngine;

public class Fryer : MonoBehaviour
{
    [Header("References")]
    public RectTransform slot;

    [Header("Timers")]
    public float semiCookTime = 2f;
    public float cookTime = 3f;
    public float burnDelay = 2f;

    private FoodItem currentFood;
    private bool cooking = false;
    private Coroutine cookRoutine;

    public bool IsFree => !cooking;

    public void PlaceItem(FoodItem item)
    {
        if (cooking || item == null) return;

        currentFood = item;

        // avisar al draggable que está en esta freidora
        var draggable = currentFood.GetComponent<DraggableItem>();
        if (draggable != null)
            draggable.SetFryer(this);

        // centrar en slot sin deformar
        Vector3 originalScale = currentFood.transform.localScale;
        currentFood.transform.SetParent(slot, false);
        currentFood.transform.localPosition = Vector3.zero;
        currentFood.transform.localScale = originalScale;

        cookRoutine = StartCoroutine(CookRoutine(currentFood.currentState));
    }

    private IEnumerator CookRoutine(FoodState startState)
    {
        cooking = true;
        Debug.Log($"⏳ {currentFood.foodName} -> START COOKING desde {startState}");

        switch (startState)
        {
            case FoodState.Raw:
                // Raw -> SemiCooked
                yield return WaitAndCheck(semiCookTime);
                if (currentFood != null)
                    currentFood.SemiCook();
                goto case FoodState.SemiCooked;

            case FoodState.SemiCooked:
                // SemiCooked -> Cooked
                yield return WaitAndCheck(cookTime - semiCookTime);
                if (currentFood != null)
                    currentFood.Cook();
                goto case FoodState.Cooked;

            case FoodState.Cooked:
                // Cooked -> Burnt
                yield return WaitAndCheck(burnDelay);
                if (currentFood != null)
                    currentFood.Burn();
                break;

            case FoodState.Burnt:
                // Ya está quemado, no hay nada más que hacer
                break;
        }

        ReleaseFood();
    }

    private IEnumerator WaitAndCheck(float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    public void ReleaseFood()
    {
        if (currentFood == null) return;

        currentFood.transform.SetParent(slot.parent, true); 
        currentFood.SetDraggable(true);

        var draggable = currentFood.GetComponent<DraggableItem>();
        if (draggable != null)
            draggable.SetFryer(null);

        currentFood = null;

        if (cookRoutine != null)
        {
            StopCoroutine(cookRoutine);
            cookRoutine = null;
        }

        cooking = false;
    }

    public void InterruptCooking()
    {
        if (currentFood == null) return;

        Debug.Log($"⏸ {currentFood.foodName} -> INTERRUPTED en estado {currentFood.currentState}");

        if (cookRoutine != null)
        {
            StopCoroutine(cookRoutine);
            cookRoutine = null;
        }

        cooking = false;

        currentFood.SetDraggable(true);
        var draggable = currentFood.GetComponent<DraggableItem>();
        if (draggable != null)
            draggable.SetFryer(null);

        currentFood = null;
    }
}
