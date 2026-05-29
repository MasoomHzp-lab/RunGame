using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [SerializeField] private CollectibleItem collectiblePrefab;
    [SerializeField] private int poolSize = 20;

    [Header("Spawn Relative To Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceAhead = 3f;
    [SerializeField] private float spawnHeight = 1f;

    private readonly List<CollectibleItem> pool = new List<CollectibleItem>();

    private void Start()
    {
        if (collectiblePrefab == null) return;

        for (int i = 0; i < poolSize; i++)
        {
            var item = Instantiate(collectiblePrefab, transform);
            item.gameObject.SetActive(false);
            pool.Add(item);
        }
    }

    public void SpawnOne()
    {
        if (player == null) return;

        CollectibleItem item = GetAvailable();
        if (item == null) return;

        // ساخت موقعیت جلوی پلیر
        Vector3 spawnPosition = player.position + player.forward * spawnDistanceAhead;
        spawnPosition.y += spawnHeight;

        item.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        item.Activate();
    }

    private CollectibleItem GetAvailable()
    {
        foreach (var item in pool)
        {
            if (!item.gameObject.activeSelf)
                return item;
        }

        return null;
    }

    public void SetMoveSpeed(float speed)
    {
        // فعلاً لازم نیست
    }
}