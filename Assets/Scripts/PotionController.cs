using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PotionController : MonoBehaviour
{
    [SerializeField, Tooltip("포션 획득 시 회복되는 체력 값")]
    private int healValue = 20;

    private bool isConsumed;

    public int HealValue => healValue;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isConsumed || GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        if (!other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            return;
        }

        if (healValue > 0)
        {
            playerHealth.Heal(healValue);
        }

        isConsumed = true;
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        healValue = Mathf.Max(0, healValue);
    }
}
