using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileController : MonoBehaviour
{
    [SerializeField]
    private float damage = 1f;

    [SerializeField]
    private float speed = 6f;

    [SerializeField]
    private float lifetime = 2f;

    [SerializeField]
    private float scale = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.right;
    private Transform baseRotationReference;
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
        if (hasImpacted)
        {
            return;
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0 || target.layer != enemyLayer)
        {
            return;
        }

        hasImpacted = true;

        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
        else
        {
            target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
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
        speed = Mathf.Max(0f, speed);
        lifetime = Mathf.Max(0f, lifetime);
        scale = Mathf.Max(0.01f, scale);
    }
}

public interface IDamageable
{
    void TakeDamage(float amount);
}
