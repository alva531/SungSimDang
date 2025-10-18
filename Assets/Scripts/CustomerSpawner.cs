using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawner")]
    public GameObject customerPrefab;
    public List<RectTransform> spawnPoints;
    public float initialDelay = 1f;
    public float spawnInterval = 4f;

    [Header("Orders")]
    public GameObject[] possibleFoodPrefabs;
    [Range(0f,1f)] public float probOne = 0.6f;
    [Range(0f,1f)] public float probTwo = 0.3f;
    [Range(0f,1f)] public float probThree = 0.1f;

    private CustomerOrder[] activeSlots;

    private void Awake()
    {
        activeSlots = new CustomerOrder[spawnPoints.Count];
        NormalizeProbs();
    }

    private void Start()
    {
        InvokeRepeating(nameof(TrySpawn), initialDelay, spawnInterval);
    }

    private void NormalizeProbs()
    {
        float s = probOne + probTwo + probThree;
        if (s <= 0f) { probOne = 0.6f; probTwo = 0.3f; probThree = 0.1f; return; }
        probOne /= s; probTwo /= s; probThree /= s;
    }

    private void TrySpawn()
    {
        int freeIndex = GetFreeSlot();
        if (freeIndex == -1) return;

        GameObject go = Instantiate(customerPrefab, spawnPoints[freeIndex], false);
        go.transform.localPosition = Vector3.zero;

        var order = go.GetComponent<CustomerOrder>();
        if (order == null) { Destroy(go); return; }

        var pedido = GenerateOrderPrefabs();
        order.SetOrder(pedido);

        activeSlots[freeIndex] = order;
    }

    private int GetFreeSlot()
    {
        for (int i = 0; i < activeSlots.Length; i++)
            if (activeSlots[i] == null) return i;
        return -1;
    }

    // --- Aquí randomizamos el estado pedido para cada item ---
    private List<(GameObject, FoodState)> GenerateOrderPrefabs()
    {
        var list = new List<(GameObject, FoodState)>();
        if (possibleFoodPrefabs == null || possibleFoodPrefabs.Length == 0) return list;

        int size = GetOrderSize();
        for (int i = 0; i < size; i++)
        {
            var pref = possibleFoodPrefabs[Random.Range(0, possibleFoodPrefabs.Length)];
            FoodState desiredState = GetRandomRequestedState();
            list.Add((pref, desiredState));
        }
        return list;
    }

    // Modifica probabilidades aquí según quieras que pidan más Cooked, etc.
    private FoodState GetRandomRequestedState()
    {
        float r = Random.value;
        // ejemplo: SemiCooked 40%, Cooked 60%
        //if (r < 0.25f) return FoodState.SemiCooked;
        return FoodState.Cooked;
    }

    private int GetOrderSize()
    {
        float r = Random.value;
        if (r < probOne) return 1;
        if (r < probOne + probTwo) return 2;
        return 3;
    }

    private void Update()
    {
        for (int i = 0; i < activeSlots.Length; i++)
        {
            if (activeSlots[i] != null && activeSlots[i].gameObject == null)
                activeSlots[i] = null;
        }
    }
}
