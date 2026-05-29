using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private BallTarget targetPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int poolSize = 8;

    private readonly List<BallTarget> pool = new List<BallTarget>();
    private float timer;

    private void Start()
    {
        CreatePool();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnTarget();
        }
    }

    private void CreatePool()
    {
        if (targetPrefab == null) return;

        for (int i = 0; i < poolSize; i++)
        {
            BallTarget instance = Instantiate(targetPrefab, transform);
            instance.gameObject.SetActive(false);
            pool.Add(instance);
        }
    }

    private void SpawnTarget()
    {
        BallTarget target = GetAvailableTarget();
        if (target == null || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        target.transform.position = point.position;
        target.transform.rotation = point.rotation;
        target.Activate(moveSpeed);
    }

    private BallTarget GetAvailableTarget()
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
        moveSpeed = speed;

        foreach (var target in pool)
        {
            if (target != null && target.gameObject.activeSelf)
                target.SetSpeed(speed);
        }
    }
}