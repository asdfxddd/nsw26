using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class MissileProjectileController : MonoBehaviour
{
    [SerializeField]
    private float turnSpeed = 720f;

    [SerializeField]
    private float maxLifetime = 4f;

    [SerializeField, Tooltip("When target is lost, missile keeps flying straight for remaining distance/speed. This sets minimum time.")]
    private float minPostTargetLostLifetime = 0.2f;

    [SerializeField, Tooltip("Upper bound for straight flight after losing target.")]
    private float maxPostTargetLostLifetime = 1.5f;

    [SerializeField]
    private string monsterLayerName = "monster";

    [SerializeField]
    private string projectileLayerName = "projectile";

    private Rigidbody2D rb;
    private PlayerStatus ownerStatus;
    private Transform currentTarget;
    private Vector2 moveDirection = Vector2.right;
    private float speed;
    private float damageMultiplier;
    private float lastKnownTargetDistance;
    private float targetLostExpireTime = -1f;
    private bool hasImpacted;

    public void Initialize(PlayerStatus owner, Transform target, float missileSpeed, float damageMultiplierValue)
    {
        ownerStatus = owner;
        currentTarget = target;
        speed = Mathf.Max(0f, missileSpeed);
        damageMultiplier = Mathf.Max(0f, damageMultiplierValue);

        Vector2 initialDirection = target != null
            ? (Vector2)(target.position - transform.position)
            : Vector2.right;

        if (initialDirection.sqrMagnitude > 0.0001f)
        {
            moveDirection = initialDirection.normalized;
        }

        if (target != null)
        {
            lastKnownTargetDistance = Vector2.Distance(transform.position, target.position);
        }

        AlignToDirection();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        hasImpacted = false;
        targetLostExpireTime = -1f;
        Destroy(gameObject, Mathf.Max(0.1f, maxLifetime));
    }

    private void FixedUpdate()
    {
        if (GameplayPauseState.IsGameplayPaused || hasImpacted)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateHoming();
        rb.linearVelocity = moveDirection * speed;
    }

    private void UpdateHoming()
    {
        if (currentTarget != null)
        {
            Vector2 toTarget = (Vector2)(currentTarget.position - transform.position);
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                lastKnownTargetDistance = toTarget.magnitude;
                Vector2 desiredDirection = toTarget.normalized;
                float maxRadians = Mathf.Deg2Rad * turnSpeed * Time.fixedDeltaTime;
                moveDirection = Vector2.Lerp(moveDirection, desiredDirection, maxRadians).normalized;
                AlignToDirection();
                return;
            }
        }

        if (targetLostExpireTime < 0f)
        {
            float fallbackDuration = speed > 0f ? lastKnownTargetDistance / speed : minPostTargetLostLifetime;
            fallbackDuration = Mathf.Clamp(fallbackDuration, minPostTargetLostLifetime, maxPostTargetLostLifetime);
            targetLostExpireTime = Time.time + fallbackDuration;
        }

        if (Time.time >= targetLostExpireTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHit(collision.gameObject);
    }

    private void TryHit(GameObject target)
    {
        if (target == null || hasImpacted || GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        if (target == gameObject || target.CompareTag("Player"))
        {
            return;
        }

        if (IsLayerName(target.layer, projectileLayerName) || !IsLayerName(target.layer, monsterLayerName))
        {
            return;
        }

        hasImpacted = true;
        float damage = ownerStatus != null ? ownerStatus.CurrentAttack * damageMultiplier : 0f;
        DamageSystem.ApplyPlayerDamage(target, damage);
        Destroy(gameObject);
    }

    private void AlignToDirection()
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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

    private void OnValidate()
    {
        turnSpeed = Mathf.Max(0f, turnSpeed);
        maxLifetime = Mathf.Max(0.1f, maxLifetime);
        minPostTargetLostLifetime = Mathf.Max(0.05f, minPostTargetLostLifetime);
        maxPostTargetLostLifetime = Mathf.Max(minPostTargetLostLifetime, maxPostTargetLostLifetime);
    }
}
