using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TreasureBoxController : MonoBehaviour, IDamageable
{
    [Serializable]
    private struct DropEntry
    {
        public GameObject dropPrefab;

        [Range(0f, 1f)]
        public float chance;
    }

    [SerializeField]
    private float maxHP = 10f;

    [SerializeField]
    private DropEntry[] dropEntries;

    private float currentHP;

    public bool IsDestroyed => currentHP <= 0f;

    private void Awake()
    {
        maxHP = Mathf.Max(1f, maxHP);
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
            OnDestroyed();
        }

        return appliedDamage;
    }

    private void OnDestroyed()
    {
        TryDropItem();
        Destroy(gameObject);
    }

    private void TryDropItem()
    {
        if (dropEntries == null || dropEntries.Length == 0)
        {
            return;
        }

        float randomPoint = UnityEngine.Random.value;
        float accumulatedChance = 0f;
        Vector3 dropPosition = transform.position;

        for (int i = 0; i < dropEntries.Length; i++)
        {
            DropEntry entry = dropEntries[i];
            if (entry.dropPrefab == null)
            {
                continue;
            }

            accumulatedChance += Mathf.Clamp01(entry.chance);
            if (randomPoint <= accumulatedChance)
            {
                Instantiate(entry.dropPrefab, dropPosition, Quaternion.identity);
                return;
            }
        }
    }

    private void OnValidate()
    {
        maxHP = Mathf.Max(1f, maxHP);

        if (dropEntries == null)
        {
            return;
        }

        for (int i = 0; i < dropEntries.Length; i++)
        {
            DropEntry entry = dropEntries[i];
            entry.chance = Mathf.Clamp01(entry.chance);
            dropEntries[i] = entry;
        }
    }
}
