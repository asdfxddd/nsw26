using System;
using UnityEngine;

public static class DamageSystem
{
    public readonly struct PlayerDamageEvent
    {
        public PlayerDamageEvent(GameObject target, float appliedDamage, bool wasKilled, bool isExplosionDamage)
        {
            Target = target;
            AppliedDamage = appliedDamage;
            WasKilled = wasKilled;
            IsExplosionDamage = isExplosionDamage;
        }

        public GameObject Target { get; }

        public float AppliedDamage { get; }

        public bool WasKilled { get; }

        public bool IsExplosionDamage { get; }
    }

    public static event Action<float> OnPlayerDamageConfirmed;

    public static event Action<PlayerDamageEvent> OnPlayerDamageApplied;

    public static float ApplyPlayerDamage(GameObject target, float attemptedDamage, bool isExplosionDamage = false)
    {
        if (target == null || attemptedDamage <= 0f)
        {
            return 0f;
        }

        float appliedDamage = 0f;
        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            appliedDamage = damageable.TakeDamage(attemptedDamage);
        }
        else
        {
            target.SendMessage("TakeDamage", attemptedDamage, SendMessageOptions.DontRequireReceiver);
        }

        if (appliedDamage <= 0f)
        {
            return 0f;
        }

        bool wasKilled = false;
        if (target.TryGetComponent<MonsterController>(out MonsterController monsterController))
        {
            wasKilled = monsterController.IsDead;
        }

        OnPlayerDamageConfirmed?.Invoke(appliedDamage);
        OnPlayerDamageApplied?.Invoke(new PlayerDamageEvent(target, appliedDamage, wasKilled, isExplosionDamage));

        return Mathf.Max(0f, appliedDamage);
    }
}

public interface IDamageable
{
    float TakeDamage(float amount);
}
