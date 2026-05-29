using UnityEngine;

public class GroundLooper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] groundSegments;
    [SerializeField] private Transform player;
    [SerializeField] private TargetSpawner targetSpawner;
    [SerializeField] private CollectibleSpawner collectibleSpawner;

    [Header("Settings")]
    [SerializeField] private float recycleBehindDistance = 150f;
    [SerializeField] private float worldObjectSpeed = 3f;

    private bool isRunning;

    private void Reset()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    private void Update()
    {
        if (!isRunning) return;
        RecycleSegments();
    }

    public void SetRunning(bool value)
    {
        isRunning = value;

        if (targetSpawner != null)
            targetSpawner.enabled = value;

        if (collectibleSpawner != null)
            collectibleSpawner.enabled = value;
    }

    public void SetPlayerSpeed(float speed)
    {
        worldObjectSpeed = Mathf.Max(0f, speed);

        if (targetSpawner != null)
            targetSpawner.SetMoveSpeed(worldObjectSpeed);

        if (collectibleSpawner != null)
            collectibleSpawner.SetMoveSpeed(worldObjectSpeed);
    }

    private void RecycleSegments()
    {
        if (player == null || groundSegments == null || groundSegments.Length == 0)
            return;

        float recycleLine = player.position.z - recycleBehindDistance;

        for (int i = 0; i < groundSegments.Length; i++)
        {
            Transform segment = groundSegments[i];
            if (segment == null) continue;

            Bounds segmentBounds = GetBounds(segment);

            if (segmentBounds.max.z < recycleLine)
            {
                Transform farthest = GetFarthestSegment(segment);
                if (farthest == null) continue;

                Bounds farthestBounds = GetBounds(farthest);
                Bounds thisBounds = GetBounds(segment);

                float deltaZ = farthestBounds.max.z - thisBounds.min.z;
                segment.position += new Vector3(0f, 0f, deltaZ);
            }
        }
    }

    private Transform GetFarthestSegment(Transform ignore)
    {
        Transform farthest = null;
        float maxZ = float.MinValue;

        for (int i = 0; i < groundSegments.Length; i++)
        {
            Transform segment = groundSegments[i];
            if (segment == null || segment == ignore) continue;

            Bounds bounds = GetBounds(segment);

            if (bounds.max.z > maxZ)
            {
                maxZ = bounds.max.z;
                farthest = segment;
            }
        }

        return farthest;
    }

    private Bounds GetBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }
}