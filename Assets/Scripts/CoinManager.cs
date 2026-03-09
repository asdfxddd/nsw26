using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField, Tooltip("현재 보유 코인 수")]
    private int currentCoins;

    public event Action<int> OnCoinsChanged;

    public int CurrentCoins => Mathf.Max(0, currentCoins);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentCoins = Mathf.Max(0, currentCoins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentCoins = Mathf.Max(0, currentCoins + amount);
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public void ResetCoins()
    {
        currentCoins = 0;
        OnCoinsChanged?.Invoke(currentCoins);
    }
}
