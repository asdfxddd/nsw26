using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinController : MonoBehaviour
{
    [SerializeField, Tooltip("획득 시 지급할 코인 수량")]
    private int coinValue = 1;

    private bool isCollected;

    public int CoinValue => coinValue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || GameplayPauseState.IsGameplayPaused || !other.CompareTag("Player"))
        {
            return;
        }

        if (!other.TryGetComponent<PlayerStatus>(out PlayerStatus playerStatus))
        {
            return;
        }

        isCollected = true;
        playerStatus.AddCoins(coinValue);
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        coinValue = Mathf.Max(1, coinValue);
    }
}
