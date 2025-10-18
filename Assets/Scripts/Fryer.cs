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

        item.GetComponent<Animator>().SetTrigger("Horno");

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
                yield return WaitAndCheck(cookTime);
                if (currentFood != null)
                    currentFood.Cook();
                goto case FoodState.Cooked;

            //case FoodState.SemiCooked:
                //yield return WaitAndCheck(cookTime - semiCookTime);
                //if (currentFood != null)
                   //currentFood.Cook();
                //goto case FoodState.Cooked;

            case FoodState.Cooked:
                yield return WaitAndCheck(burnDelay);
                if (currentFood != null)
                    currentFood.Burn();
                break;

            case FoodState.Burnt:
                break;
        }
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
        currentFood.GetComponent<Animator>().SetTrigger("Base");

        currentFood.SetDraggable(true);
        var draggable = currentFood.GetComponent<DraggableItem>();
        if (draggable != null)
            draggable.SetFryer(null);

        currentFood = null;
    }
}
