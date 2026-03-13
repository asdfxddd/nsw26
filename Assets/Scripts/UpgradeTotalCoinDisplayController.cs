using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class UpgradeTotalCoinDisplayController : MonoBehaviour
{
    [SerializeField]
    private string mainSceneName = "MainScene";

    [SerializeField]
    private string popupName = "Popup_Upgrade";

    [SerializeField]
    private string totalCoinTextObjectName = "TXT_TotalCoin";

    [SerializeField]
    private TMP_Text totalCoinText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (FindObjectOfType<UpgradeTotalCoinDisplayController>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject(nameof(UpgradeTotalCoinDisplayController));
        bootstrapObject.AddComponent<UpgradeTotalCoinDisplayController>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != mainSceneName)
        {
            return;
        }

        ResolveTextReference();
        RefreshText();

        if (CoinPersistenceManager.Instance != null)
        {
            CoinPersistenceManager.Instance.OnTotalCoinsChanged += HandleTotalCoinsChanged;
        }
    }

    private void OnDestroy()
    {
        if (CoinPersistenceManager.Instance != null)
        {
            CoinPersistenceManager.Instance.OnTotalCoinsChanged -= HandleTotalCoinsChanged;
        }
    }

    private void HandleTotalCoinsChanged(int totalCoins)
    {
        RefreshText(totalCoins);
    }

    private void ResolveTextReference()
    {
        if (totalCoinText != null)
        {
            return;
        }

        GameObject textObject = GameObject.Find(totalCoinTextObjectName);
        if (textObject != null)
        {
            totalCoinText = textObject.GetComponent<TMP_Text>();
        }

        if (totalCoinText != null)
        {
            return;
        }

        GameObject popupObject = GameObject.Find(popupName);
        if (popupObject == null)
        {
            Debug.LogWarning("[CoinPersistence] Popup_Upgrade를 찾지 못해 총 코인 텍스트를 표시할 수 없습니다.");
            return;
        }

        GameObject created = new GameObject(totalCoinTextObjectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        created.transform.SetParent(popupObject.transform, false);

        RectTransform rect = created.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -24f);
        rect.sizeDelta = new Vector2(420f, 50f);

        totalCoinText = created.GetComponent<TextMeshProUGUI>();
        totalCoinText.fontSize = 32f;
        totalCoinText.alignment = TextAlignmentOptions.Center;
        totalCoinText.color = Color.white;
    }

    private void RefreshText()
    {
        int coins = CoinPersistenceManager.Instance != null ? CoinPersistenceManager.Instance.TotalCoins : 0;
        RefreshText(coins);
    }

    private void RefreshText(int coins)
    {
        if (totalCoinText == null)
        {
            return;
        }

        totalCoinText.text = $"총 코인: {Mathf.Max(0, coins)}";
    }
}
