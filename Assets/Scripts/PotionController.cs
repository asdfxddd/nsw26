using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PotionController : MagnetCollectible
{
    [SerializeField, Tooltip("포션 획득 시 회복되는 체력 값")]
    private int healValue = 20;

    public override bool CanUseBoostedPickupRadius => false;
    public int HealValue => healValue;

    protected override void OnCollected()
    {
        if (AttractionTarget == null)
        {
            return;
        }

        PlayerHealth playerHealth = AttractionTarget.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = AttractionTarget.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            return;
        }

        if (healValue > 0)
        {
            playerHealth.Heal(healValue);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        healValue = Mathf.Max(0, healValue);
    }
}
