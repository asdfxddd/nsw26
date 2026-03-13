using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinController : MagnetCollectible
{
    [SerializeField, Tooltip("획득 시 지급할 코인 수량")]
    private int coinValue = 1;

    public override bool CanUseBoostedPickupRadius => false;
    public int CoinValue => coinValue;

    protected override void OnCollected()
    {
        CoinManager coinManager = CoinManager.Instance;
        if (coinManager == null)
        {
            return;
        }

        coinManager.AddCoins(coinValue);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        coinValue = Mathf.Max(1, coinValue);
    }
}
