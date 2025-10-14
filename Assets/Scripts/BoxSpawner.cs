using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public GameObject boxPrefab;
    public Transform spawnPoint;

    private GameObject currentBox;

    void Start()
    {
        SpawnNewBox();
    }

    public void SpawnNewBox()
    {
        if (currentBox != null) return;
        currentBox = Instantiate(boxPrefab, spawnPoint.position, Quaternion.identity, transform);
    }

    public void OnBoxDelivered()
    {
        currentBox = null;
        SpawnNewBox();
    }
}
