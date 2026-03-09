using UnityEngine;

public class TreasureBoxSpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject treasureBoxPrefab;

    [SerializeField]
    private float spawnCooldown = 8f;

    [SerializeField]
    private float initialDelay = 3f;

    [SerializeField]
    private int maxActiveBoxes = 3;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Camera spawnCamera;

    [SerializeField]
    private MapBoundaryController mapBoundary;

    [SerializeField]
    private float spawnRadiusBuffer = 1.5f;

    [SerializeField]
    private float spawnRadiusThickness = 2f;

    [SerializeField]
    private int spawnPositionTryCount = 12;

    private float nextSpawnTime;
    private int activeBoxCount;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (spawnCamera == null)
        {
            spawnCamera = Camera.main;
        }

        if (mapBoundary == null)
        {
            mapBoundary = FindObjectOfType<MapBoundaryController>();
        }

        spawnCooldown = Mathf.Max(0.05f, spawnCooldown);
        initialDelay = Mathf.Max(0f, initialDelay);
        maxActiveBoxes = Mathf.Max(1, maxActiveBoxes);
        spawnPositionTryCount = Mathf.Max(1, spawnPositionTryCount);
        spawnRadiusBuffer = Mathf.Max(0.25f, spawnRadiusBuffer);
        spawnRadiusThickness = Mathf.Max(0.25f, spawnRadiusThickness);
        nextSpawnTime = Time.time + initialDelay;
    }

    private void Update()
    {
        if (GameplayPauseState.IsGameplayPaused || treasureBoxPrefab == null || player == null)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        nextSpawnTime = Time.time + spawnCooldown;

        if (activeBoxCount >= maxActiveBoxes)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPositionOutsideCamera();
        GameObject boxObject = Instantiate(treasureBoxPrefab, spawnPosition, Quaternion.identity);

        if (!boxObject.TryGetComponent<TreasureBoxController>(out _))
        {
            boxObject.AddComponent<TreasureBoxController>();
        }

        TreasureBoxSpawnTracker tracker = boxObject.GetComponent<TreasureBoxSpawnTracker>();
        if (tracker == null)
        {
            tracker = boxObject.AddComponent<TreasureBoxSpawnTracker>();
        }

        tracker.Initialize(this);
        activeBoxCount++;
    }

    public void NotifyTreasureBoxDestroyed()
    {
        activeBoxCount = Mathf.Max(0, activeBoxCount - 1);
    }

    private Vector3 GetSpawnPositionOutsideCamera()
    {
        Vector3 center = player.position;
        float minRadius = 6f;
        float maxRadius = minRadius + spawnRadiusThickness;

        if (spawnCamera != null && spawnCamera.orthographic)
        {
            float halfHeight = spawnCamera.orthographicSize;
            float halfWidth = halfHeight * spawnCamera.aspect;
            minRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight)) + spawnRadiusBuffer;
            maxRadius = minRadius + spawnRadiusThickness;
        }

        Vector3 fallbackPosition = center + Vector3.up * minRadius;

        for (int i = 0; i < spawnPositionTryCount; i++)
        {
            float angleRad = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            float radius = UnityEngine.Random.Range(minRadius, maxRadius);
            Vector3 candidate = center + new Vector3(direction.x, direction.y, 0f) * radius;

            if (mapBoundary != null)
            {
                candidate = mapBoundary.ClampPosition(candidate);
            }

            fallbackPosition = candidate;
            if (IsOutsideCamera(candidate))
            {
                return candidate;
            }
        }

        return fallbackPosition;
    }

    private bool IsOutsideCamera(Vector3 worldPosition)
    {
        if (spawnCamera == null)
        {
            return true;
        }

        Vector3 viewportPoint = spawnCamera.WorldToViewportPoint(worldPosition);
        return viewportPoint.z < 0f
            || viewportPoint.x < 0f
            || viewportPoint.x > 1f
            || viewportPoint.y < 0f
            || viewportPoint.y > 1f;
    }

    private void OnValidate()
    {
        spawnCooldown = Mathf.Max(0.05f, spawnCooldown);
        initialDelay = Mathf.Max(0f, initialDelay);
        maxActiveBoxes = Mathf.Max(1, maxActiveBoxes);
        spawnPositionTryCount = Mathf.Max(1, spawnPositionTryCount);
        spawnRadiusBuffer = Mathf.Max(0.25f, spawnRadiusBuffer);
        spawnRadiusThickness = Mathf.Max(0.25f, spawnRadiusThickness);
    }

    private sealed class TreasureBoxSpawnTracker : MonoBehaviour
    {
        private TreasureBoxSpawnManager owner;

        public void Initialize(TreasureBoxSpawnManager spawnManager)
        {
            owner = spawnManager;
        }

        private void OnDestroy()
        {
            if (owner != null)
            {
                owner.NotifyTreasureBoxDestroyed();
            }
        }
    }
}
