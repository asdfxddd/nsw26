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
        transform.localScale = Vector3.one * scale;
    }

    private void OnEnable()
    {
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
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0 || target.layer != enemyLayer)
        {
            return;
        }

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
        Vector2 rotationDirection = direction;

        if (flipX)
        {
            rotationDirection = new Vector2(-direction.x, direction.y);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipX;
        }

        Quaternion baseRotation = baseRotationReference != null ? baseRotationReference.rotation : Quaternion.identity;
        Quaternion lookRotation = Quaternion.FromToRotation(Vector3.right, new Vector3(rotationDirection.x, rotationDirection.y, 0f));
        transform.rotation = baseRotation * lookRotation;
    }
}

public interface IDamageable
{
    void TakeDamage(float amount);
}
