using System;
using UnityEngine;

public static class DamageSystem
{
    public static event Action<float> OnPlayerDamageConfirmed;

    public static float ApplyPlayerDamage(GameObject target, float attemptedDamage)
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

        if (appliedDamage > 0f)
        {
            OnPlayerDamageConfirmed?.Invoke(appliedDamage);
        }

        return Mathf.Max(0f, appliedDamage);
    }
}

public interface IDamageable
{
    float TakeDamage(float amount);
}
