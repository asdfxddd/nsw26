using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HUDCoinDisplayController : MonoBehaviour
{
    [SerializeField, Tooltip("HUD에 표시할 코인 TextMeshPro 텍스트")]
    private TMP_Text hudCoinText;

    [SerializeField, Tooltip("코인 보유량 데이터를 관리하는 CoinManager")]
    private CoinManager coinManager;

    private void Awake()
    {
        if (hudCoinText == null)
        {
            hudCoinText = GetComponent<TMP_Text>();
        }

        if (coinManager == null)
        {
            coinManager = CoinManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (coinManager == null)
        {
            coinManager = CoinManager.Instance;
        }

        if (coinManager != null)
        {
            coinManager.OnCoinsChanged += HandleCoinsChanged;
            HandleCoinsChanged(coinManager.CurrentCoins);
        }
        else
        {
            UpdateCoinText(0);
        }
    }

    private void OnDisable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged -= HandleCoinsChanged;
        }
    }

    private void HandleCoinsChanged(int currentCoins)
    {
        UpdateCoinText(currentCoins);
    }

    private void UpdateCoinText(int coinAmount)
    {
        if (hudCoinText == null)
        {
            return;
        }

        hudCoinText.text = $"Coin: {Mathf.Max(0, coinAmount)}";
    }
}
