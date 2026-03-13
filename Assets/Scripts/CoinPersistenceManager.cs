using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CoinPersistenceManager : MonoBehaviour
{
    public static CoinPersistenceManager Instance { get; private set; }

    private const int MaxTotalCoins = 1000000000;

    private ICoinStorageBackend storageBackend;
    private int totalCoins;
    private string lastUpdatedAt;

    public int TotalCoins => totalCoins;
    public string LastUpdatedAt => lastUpdatedAt;

    public event Action<int> OnTotalCoinsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject(nameof(CoinPersistenceManager));
        bootstrapObject.AddComponent<CoinPersistenceManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        storageBackend = CreateStorageBackend();
        LoadOrRecoverDefaults();
    }

    public void CommitRunCoins(int runCoins)
    {
        if (runCoins <= 0)
        {
            return;
        }

        totalCoins = ClampCoins(totalCoins + runCoins);
        lastUpdatedAt = DateTime.UtcNow.ToString("O");

        if (!SaveCurrentState())
        {
            Debug.LogWarning($"[CoinPersistence] Failed to save total coin data. totalCoins={totalCoins}");
        }

        OnTotalCoinsChanged?.Invoke(totalCoins);
    }

#if UNITY_EDITOR
    [ContextMenu("Cheat/Reset Total Coins To Zero")]
    public void CheatResetTotalCoins()
    {
        totalCoins = 0;
        lastUpdatedAt = DateTime.UtcNow.ToString("O");
        if (!SaveCurrentState())
        {
            Debug.LogWarning("[CoinPersistence] Cheat reset failed to save.");
        }

        OnTotalCoinsChanged?.Invoke(totalCoins);
        Debug.Log("[CoinPersistence] Editor cheat applied: total coins reset to 0.");
    }

    [ContextMenu("Cheat/Add 100 Total Coins")]
    public void CheatAdd100Coins()
    {
        totalCoins = ClampCoins(totalCoins + 100);
        lastUpdatedAt = DateTime.UtcNow.ToString("O");
        if (!SaveCurrentState())
        {
            Debug.LogWarning("[CoinPersistence] Cheat add failed to save.");
        }

        OnTotalCoinsChanged?.Invoke(totalCoins);
        Debug.Log("[CoinPersistence] Editor cheat applied: +100 total coins.");
    }
#endif

    private void LoadOrRecoverDefaults()
    {
        if (storageBackend != null && storageBackend.TryLoad(out CoinSaveData data) && data != null)
        {
            totalCoins = ClampCoins(data.totalCoins);
            lastUpdatedAt = string.IsNullOrWhiteSpace(data.lastUpdatedAt)
                ? DateTime.UtcNow.ToString("O")
                : data.lastUpdatedAt;

            Debug.Log($"[CoinPersistence] Loaded total coin data. totalCoins={totalCoins}, lastUpdatedAt={lastUpdatedAt}");
            OnTotalCoinsChanged?.Invoke(totalCoins);
            return;
        }

        totalCoins = 0;
        lastUpdatedAt = DateTime.UtcNow.ToString("O");

        Debug.LogWarning("[CoinPersistence] Failed to load coin data. Recovered to defaults (totalCoins=0).");
        if (!SaveCurrentState())
        {
            Debug.LogWarning("[CoinPersistence] Failed to persist default recovery state.");
        }

        OnTotalCoinsChanged?.Invoke(totalCoins);
    }

    private bool SaveCurrentState()
    {
        if (storageBackend == null)
        {
            return false;
        }

        CoinSaveData data = new CoinSaveData
        {
            totalCoins = ClampCoins(totalCoins),
            lastUpdatedAt = lastUpdatedAt
        };

        return storageBackend.TrySave(data);
    }

    private static ICoinStorageBackend CreateStorageBackend()
    {
        // 기본 구현은 PlayerPrefs(Key-Value). 필요 시 JsonFileCoinStorageBackend로 교체 가능.
        return new PlayerPrefsCoinStorageBackend();
    }

    private static int ClampCoins(int value)
    {
        return Mathf.Clamp(value, 0, MaxTotalCoins);
    }
}
