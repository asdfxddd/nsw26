using System;
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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.right;
    private Transform baseRotationReference;
    private PlayerStatus ownerStatus;
    private bool hasImpacted;

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

        hasImpacted = true;
        float calculatedDamage = CalculateDamage();

        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(calculatedDamage);
        }
        else
        {
            target.SendMessage("TakeDamage", calculatedDamage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
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

public interface IDamageable
{
    void TakeDamage(float amount);
}
