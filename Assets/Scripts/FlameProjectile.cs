using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FlameProjectile : MonoBehaviour
{
    [SerializeField]
    private string monsterLayerName = "monster";

    [SerializeField, Tooltip("같은 적에게 다시 피해를 주기까지의 내부 쿨다운(초)")]
    private float hitCooldown = 0.2f;

    private readonly Dictionary<int, float> nextHitTimeByTarget = new Dictionary<int, float>();

    private FlameController ownerController;

    public void Configure(FlameController controller, float durationSeconds)
    {
        ownerController = controller;
        Destroy(gameObject, Mathf.Max(0.01f, durationSeconds));
    }

    public void UpdateFacing(Vector2 facingDirection, float forwardOffset)
    {
        Vector2 facing = facingDirection.sqrMagnitude > 0f ? facingDirection.normalized : Vector2.right;
        transform.localPosition = facing * forwardOffset;

        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryApplyFlameHit(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryApplyFlameHit(collision.gameObject);
    }

    private void TryApplyFlameHit(GameObject target)
    {
        if (GameplayPauseState.IsGameplayPaused || target == null || ownerController == null)
        {
            return;
        }

        if (!IsLayerName(target.layer, monsterLayerName))
        {
            return;
        }

        int targetId = target.GetInstanceID();
        float now = Time.time;
        if (nextHitTimeByTarget.TryGetValue(targetId, out float nextHitTime) && now < nextHitTime)
        {
            return;
        }

        float damage = ownerController.CalculateDamage();
        if (damage <= 0f)
        {
            return;
        }

        float appliedDamage = DamageSystem.ApplyPlayerDamage(target, damage);
        if (appliedDamage <= 0f)
        {
            return;
        }

        nextHitTimeByTarget[targetId] = now + Mathf.Max(0f, hitCooldown);
        ApplyKnockback(target);
    }

    private void ApplyKnockback(GameObject target)
    {
        Vector2 knockbackDirection = ownerController.GetKnockbackDirection();
        if (knockbackDirection.sqrMagnitude <= 0f)
        {
            return;
        }

        Vector3 delta = (Vector3)(knockbackDirection.normalized * ownerController.KnockbackDistance);
        target.transform.position += delta;
    }

    private static bool IsLayerName(int layer, string expectedLayerName)
    {
        if (string.IsNullOrWhiteSpace(expectedLayerName))
        {
            return false;
        }

        string actualLayerName = LayerMask.LayerToName(layer);
        return string.Equals(actualLayerName, expectedLayerName, StringComparison.OrdinalIgnoreCase);
    }

    private void OnDestroy()
    {
        if (ownerController != null)
        {
            ownerController.NotifyProjectileDestroyed(this);
        }
    }

    private void OnValidate()
    {
        hitCooldown = Mathf.Max(0f, hitCooldown);
    }
}
