using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileController : MonoBehaviour, IPlayerAttackDamageSource
{
    [SerializeField]
    [FormerlySerializedAs("damage")]
    [FormerlySerializedAs("projectileDamage")]
    private float damageMultiplier = 1f;

    [SerializeField]
    private float speed = 6f;

    [SerializeField]
    private float lifetime = 2f;

    [SerializeField]
    private float scale = 1f;

    [SerializeField]
    private string monsterLayerName = "monster";

    [SerializeField]
    private string projectileLayerName = "projectile";

    public float DamageMultiplier => damageMultiplier;

    private readonly HashSet<int> blockedTargetIds = new HashSet<int>();

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.right;
    private Transform baseRotationReference;
    private PlayerStatus ownerStatus;
    private bool hasImpacted;
    private int remainingSplitCount;

    public void Initialize(Vector2 direction, Transform rotationReference)
    {
        if (direction.sqrMagnitude > 0f)
        {
            moveDirection = direction.normalized;
        }

        baseRotationReference = rotationReference;
        ApplyOrientation();
    }

    public void SetOwnerStatus(PlayerStatus playerStatus)
    {
        ownerStatus = playerStatus;
    }

    public void SetRemainingSplitCount(int count)
    {
        remainingSplitCount = Mathf.Max(0, count);
    }

    public void SetBlockedTargets(IEnumerable<int> targetInstanceIds)
    {
        blockedTargetIds.Clear();
        if (targetInstanceIds == null)
        {
            return;
        }

        foreach (int targetId in targetInstanceIds)
        {
            blockedTargetIds.Add(targetId);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        transform.localScale = Vector3.one * scale;
    }

    private void OnEnable()
    {
        hasImpacted = false;
        ApplyOrientation();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject target)
    {
        if (GameplayPauseState.IsGameplayPaused || hasImpacted)
        {
            return;
        }

        if (target == gameObject || target.CompareTag("Player"))
        {
            return;
        }

        if (IsLayerName(target.layer, projectileLayerName))
        {
            return;
        }

        if (!IsLayerName(target.layer, monsterLayerName))
        {
            return;
        }

        int targetId = target.GetInstanceID();
        if (blockedTargetIds.Contains(targetId))
        {
            return;
        }

        hasImpacted = true;
        float calculatedDamage = CalculateDamage();
        DamageSystem.ApplyPlayerDamage(target, calculatedDamage);

        if (remainingSplitCount > 0)
        {
            SpawnSplitProjectiles(target);
        }

        Destroy(gameObject);
    }

    private void SpawnSplitProjectiles(GameObject hitTarget)
    {
        Transform[] targets = FindClosestSplitTargets(hitTarget);
        if (targets.Length <= 0)
        {
            return;
        }

        blockedTargetIds.Add(hitTarget.GetInstanceID());

        int nextSplitCount = remainingSplitCount - 1;
        Vector3 spawnPosition = transform.position;

        for (int i = 0; i < targets.Length; i++)
        {
            Transform targetTransform = targets[i];
            if (targetTransform == null)
            {
                continue;
            }

            Vector2 direction = (targetTransform.position - spawnPosition);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = moveDirection;
            }

            ProjectileController splitProjectile = Instantiate(this, spawnPosition, Quaternion.identity);
            splitProjectile.Initialize(direction, baseRotationReference);
            splitProjectile.SetOwnerStatus(ownerStatus);
            splitProjectile.SetRemainingSplitCount(nextSplitCount);
            splitProjectile.SetBlockedTargets(blockedTargetIds);
        }
    }

    private Transform[] FindClosestSplitTargets(GameObject hitTarget)
    {
        MonsterController[] monsters = FindObjectsByType<MonsterController>(FindObjectsSortMode.None);
        if (monsters == null || monsters.Length <= 0)
        {
            return Array.Empty<Transform>();
        }

        Transform first = null;
        Transform second = null;
        float firstDistance = float.MaxValue;
        float secondDistance = float.MaxValue;

        Vector3 origin = transform.position;

        for (int i = 0; i < monsters.Length; i++)
        {
            MonsterController monster = monsters[i];
            if (monster == null)
            {
                continue;
            }

            Transform candidate = monster.transform;
            if (candidate == null)
            {
                continue;
            }

            if (hitTarget != null && candidate.gameObject == hitTarget)
            {
                continue;
            }

            int candidateId = candidate.gameObject.GetInstanceID();
            if (blockedTargetIds.Contains(candidateId))
            {
                continue;
            }

            float sqrDistance = (candidate.position - origin).sqrMagnitude;
            if (sqrDistance < firstDistance)
            {
                second = first;
                secondDistance = firstDistance;
                first = candidate;
                firstDistance = sqrDistance;
            }
            else if (sqrDistance < secondDistance)
            {
                second = candidate;
                secondDistance = sqrDistance;
            }
        }

        if (first != null && second != null)
        {
            return new[] { first, second };
        }

        if (first != null)
        {
            return new[] { first };
        }

        return Array.Empty<Transform>();
    }

    private float CalculateDamage()
    {
        float currentAttack = ownerStatus != null ? ownerStatus.CurrentAttack : 0f;
        return Mathf.Max(0f, currentAttack * Mathf.Max(0f, damageMultiplier));
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

    private void ApplyOrientation()
    {
        Vector2 direction = moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : Vector2.right;
        bool flipX = direction.x < 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipX;
        }

        Quaternion baseRotation = baseRotationReference != null ? baseRotationReference.rotation : Quaternion.identity;
        Vector2 baseRight = baseRotationReference != null ? baseRotationReference.right : Vector2.right;

        float baseRightAngle = Mathf.Atan2(baseRight.y, baseRight.x) * Mathf.Rad2Deg;
        float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Sprite's default forward is +X. If flipped, visual forward becomes -X,
        // so we offset by 180° to keep left-diagonal orientations correct.
        float visualFacingAngle = flipX ? directionAngle - 180f : directionAngle;

        float relativeAngle = Mathf.DeltaAngle(baseRightAngle, visualFacingAngle);
        transform.rotation = Quaternion.AngleAxis(relativeAngle, Vector3.forward) * baseRotation;
    }

    private void OnValidate()
    {
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        speed = Mathf.Max(0f, speed);
        lifetime = Mathf.Max(0f, lifetime);
        scale = Mathf.Max(0.01f, scale);
    }
}

public interface IPlayerAttackDamageSource
{
    float DamageMultiplier { get; }
}
