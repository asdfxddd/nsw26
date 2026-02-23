using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterSpawner : MonoBehaviour
{
    [Serializable]
    public class StageMonsterRule
    {
        public int StageId;
        public string MonsterId;
        public float SpawnStartSec;
        public float WaveIntervalSec;
        public int WaveSizeStart;
        public int WaveSizeGrowth;
        public int WaveSizeMax;
        public int TotalBudget;
        public int MaxAliveCap;
    }

    public class SpawnRuntime
    {
        public StageMonsterRule rule;
        public int waveIndex;
        public int totalSpawned;
        public int aliveCount;
        public float nextWaveTime;
    }

    [Serializable]
    public class MonsterPrefabEntry
    {
        public string MonsterId;
        public GameObject Prefab;
    }

    [SerializeField]
    private string stageMonsterResourcePath = "StageMonster";

    [SerializeField]
    private string stageResourcePath = "Stage";

    [SerializeField]
    private bool useStageIdOverride;

    [SerializeField]
    private int stageIdOverride = 1;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Camera spawnCamera;

    [SerializeField]
    private MapBoundaryController mapBoundary;

    [SerializeField]
    private float spawnRadiusBuffer = 0.75f;

    [SerializeField]
    private float spawnRadiusThickness = 1.5f;

    [SerializeField]
    private List<MonsterPrefabEntry> monsterPrefabs = new List<MonsterPrefabEntry>();

    private readonly List<SpawnRuntime> activeRules = new List<SpawnRuntime>();
    private readonly Dictionary<string, GameObject> monsterPrefabLookup = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

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

        BuildMonsterPrefabLookup();

        LoadRules();
    }

    private void Update()
    {
        if (player == null || activeRules.Count == 0)
        {
            return;
        }

        float time = Time.timeSinceLevelLoad;
        foreach (SpawnRuntime runtime in activeRules)
        {
            if (runtime.totalSpawned >= runtime.rule.TotalBudget)
            {
                continue;
            }

            if (time < runtime.nextWaveTime)
            {
                continue;
            }

            int waveSize = runtime.rule.WaveSizeStart + (runtime.waveIndex * runtime.rule.WaveSizeGrowth);
            if (runtime.rule.WaveSizeMax > 0)
            {
                waveSize = Mathf.Min(waveSize, runtime.rule.WaveSizeMax);
            }

            int remainingBudget = runtime.rule.TotalBudget - runtime.totalSpawned;
            int remainingAliveCap = runtime.rule.MaxAliveCap > 0
                ? runtime.rule.MaxAliveCap - runtime.aliveCount
                : remainingBudget;

            int spawnCount = Mathf.Min(waveSize, remainingBudget, remainingAliveCap);
            if (spawnCount > 0)
            {
                SpawnWave(runtime, spawnCount);
            }

            runtime.waveIndex++;
            float interval = Mathf.Max(0.05f, runtime.rule.WaveIntervalSec);
            runtime.nextWaveTime += interval;
        }
    }

    private void SpawnWave(SpawnRuntime runtime, int count)
    {
        if (!monsterPrefabLookup.TryGetValue(runtime.rule.MonsterId, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning($"Monster prefab '{runtime.rule.MonsterId}' not found in Monster Prefabs list.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetSpawnPosition();
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);

            if (!instance.TryGetComponent<MonsterController>(out _))
            {
                instance.AddComponent<MonsterController>();
            }

            MonsterSpawnTracker tracker = instance.GetComponent<MonsterSpawnTracker>();
            if (tracker == null)
            {
                tracker = instance.AddComponent<MonsterSpawnTracker>();
            }

            tracker.Initialize(this, runtime);
            runtime.totalSpawned++;
            runtime.aliveCount++;
        }
    }

    private void BuildMonsterPrefabLookup()
    {
        monsterPrefabLookup.Clear();
        foreach (MonsterPrefabEntry entry in monsterPrefabs)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.MonsterId) || entry.Prefab == null)
            {
                continue;
            }

            monsterPrefabLookup[entry.MonsterId.Trim()] = entry.Prefab;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 center = player.position;
        float minRadius = 6f;
        float maxRadius = minRadius + spawnRadiusThickness;

        if (spawnCamera != null && spawnCamera.orthographic)
        {
            float height = spawnCamera.orthographicSize;
            float width = height * spawnCamera.aspect;
            minRadius = Mathf.Sqrt(width * width + height * height) + spawnRadiusBuffer;
            maxRadius = minRadius + spawnRadiusThickness;
        }

        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        if (direction == Vector2.zero)
        {
            direction = Vector2.up;
        }

        float radius = UnityEngine.Random.Range(minRadius, maxRadius);
        Vector3 position = center + new Vector3(direction.x, direction.y, 0f) * radius;

        if (mapBoundary != null)
        {
            position = mapBoundary.ClampPosition(position);
        }

        return position;
    }

    private void LoadRules()
    {
        int stageId = ResolveStageId();
        TextAsset stageMonsterCsv = Resources.Load<TextAsset>(stageMonsterResourcePath);
        if (stageMonsterCsv == null)
        {
            Debug.LogWarning("StageMonster.csv not found in Resources.");
            return;
        }

        List<StageMonsterRule> rules = ParseStageMonsterRules(stageMonsterCsv.text, stageId);
        foreach (StageMonsterRule rule in rules)
        {
            SpawnRuntime runtime = new SpawnRuntime
            {
                rule = rule,
                waveIndex = 0,
                totalSpawned = 0,
                aliveCount = 0,
                nextWaveTime = rule.SpawnStartSec
            };
            activeRules.Add(runtime);
        }
    }

    private int ResolveStageId()
    {
        if (useStageIdOverride)
        {
            return stageIdOverride;
        }

        TextAsset stageCsv = Resources.Load<TextAsset>(stageResourcePath);
        if (stageCsv == null)
        {
            return stageIdOverride;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        string[] lines = stageCsv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 2)
            {
                continue;
            }

            if (!string.Equals(columns[1].Trim(), sceneName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(columns[0], out int parsed))
            {
                return parsed;
            }
        }

        return stageIdOverride;
    }

    private static List<StageMonsterRule> ParseStageMonsterRules(string csvText, int stageId)
    {
        List<StageMonsterRule> results = new List<StageMonsterRule>();
        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 9)
            {
                continue;
            }

            if (!int.TryParse(columns[0], out int rowStageId))
            {
                continue;
            }

            if (rowStageId != stageId)
            {
                continue;
            }

            StageMonsterRule rule = new StageMonsterRule
            {
                StageId = rowStageId,
                MonsterId = columns[1].Trim(),
                SpawnStartSec = ParseFloat(columns[2]),
                WaveIntervalSec = ParseFloat(columns[3]),
                WaveSizeStart = ParseInt(columns[4]),
                WaveSizeGrowth = ParseInt(columns[5]),
                WaveSizeMax = ParseInt(columns[6]),
                TotalBudget = ParseInt(columns[7]),
                MaxAliveCap = ParseInt(columns[8])
            };

            results.Add(rule);
        }

        return results;
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out int parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static float ParseFloat(string value)
    {
        if (float.TryParse(value, out float parsed))
        {
            return parsed;
        }

        return 0f;
    }

    public void NotifyDespawn(SpawnRuntime runtime)
    {
        if (runtime != null)
        {
            runtime.aliveCount = Mathf.Max(0, runtime.aliveCount - 1);
        }
    }
}
