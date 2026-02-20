using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterController : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float maxHP = 3f;

    [SerializeField]
    private float attackDamage = 10f;

    private float currentHP;

    private void Awake()
    {
        maxHP = Mathf.Max(1f, maxHP);
        attackDamage = Mathf.Max(0f, attackDamage);
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(0f, currentHP - amount);
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryAttackPlayer(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryAttackPlayer(collision.gameObject);
    }

    private void TryAttackPlayer(GameObject target)
    {
        if (!target.CompareTag("Player"))
        {
            return;
        }

        if (target.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            // PlayerHealth 내부 무적 처리로 연속 피해를 방지.
            playerHealth.TryTakeDamage(attackDamage);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        maxHP = Mathf.Max(1f, maxHP);
        attackDamage = Mathf.Max(0f, attackDamage);
    }
}
