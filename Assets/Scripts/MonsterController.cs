using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterController : MonoBehaviour, IDamageable
{
    [Serializable]
    private struct ExpDropEntry
    {
        public GameObject orbPrefab;

        [Range(0f, 1f)]
        public float dropChance;
    }

    [SerializeField]
    private float maxHP = 3f;

    [SerializeField]
    private float attackDamage = 10f;

    [SerializeField]
    private ExpDropEntry[] expDropEntries;

    private float currentHP;

    public bool IsDead => currentHP <= 0f;

    private void Awake()
    {
        maxHP = Mathf.Max(1f, maxHP);
        attackDamage = Mathf.Max(0f, attackDamage);
        currentHP = maxHP;
    }

    public float TakeDamage(float amount)
    {
        if (GameplayPauseState.IsGameplayPaused || amount <= 0f || currentHP <= 0f)
        {
            return 0f;
        }

        float appliedDamage = Mathf.Min(currentHP, amount);
        currentHP = Mathf.Max(0f, currentHP - appliedDamage);
        if (currentHP <= 0f)
        {
            Die();
        }

        return appliedDamage;
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
        if (GameplayPauseState.IsGameplayPaused || !target.CompareTag("Player"))
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
        TryDropExpOrbs();
        Destroy(gameObject);
    }

    private void TryDropExpOrbs()
    {
        if (expDropEntries == null || expDropEntries.Length == 0)
        {
            return;
        }

        Vector3 dropPosition = transform.position;

        for (int i = 0; i < expDropEntries.Length; i++)
        {
            ExpDropEntry entry = expDropEntries[i];
            if (entry.orbPrefab == null)
            {
                continue;
            }

            float chance = Mathf.Clamp01(entry.dropChance);
            if (UnityEngine.Random.value <= chance)
            {
                Instantiate(entry.orbPrefab, dropPosition, Quaternion.identity);
            }
        }
    }

    private void OnValidate()
    {
        maxHP = Mathf.Max(1f, maxHP);
        attackDamage = Mathf.Max(0f, attackDamage);

        if (expDropEntries == null)
        {
            return;
        }

        for (int i = 0; i < expDropEntries.Length; i++)
        {
            ExpDropEntry entry = expDropEntries[i];
            entry.dropChance = Mathf.Clamp01(entry.dropChance);
            expDropEntries[i] = entry;
        }
    }
}
