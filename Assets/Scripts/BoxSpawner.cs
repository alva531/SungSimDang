using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public GameObject boxPrefab;
    public Transform spawnPoint;
    public GameObject tutorial2;

    private GameObject currentBox;

    void Start()
    {
        SpawnNewBox();
    }

    public void SpawnNewBox()
    {
        if (currentBox != null) return;
        currentBox = Instantiate(boxPrefab, spawnPoint.position, Quaternion.identity, transform);
        currentBox.GetComponentInChildren<OrderBox>().tutorial = tutorial2;
    }

    public void OnBoxDelivered()
    {
        currentBox = null;
        SpawnNewBox();
    }
}
