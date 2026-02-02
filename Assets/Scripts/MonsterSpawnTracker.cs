using UnityEngine;

public class MonsterSpawnTracker : MonoBehaviour
{
    private MonsterSpawner spawner;
    private MonsterSpawner.SpawnRuntime runtime;

    public void Initialize(MonsterSpawner owner, MonsterSpawner.SpawnRuntime spawnRuntime)
    {
        spawner = owner;
        runtime = spawnRuntime;
    }

    private void OnDestroy()
    {
        if (spawner != null && runtime != null)
        {
            spawner.NotifyDespawn(runtime);
        }
    }
}
