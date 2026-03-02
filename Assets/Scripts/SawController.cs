using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
public class SawController : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 5f;

    [SerializeField]
    private float cooldown = 1f;

    [SerializeField]
    private float speed = 6f;

    [SerializeField, Tooltip("100 = 100%, 150 = 150%")]
    private float damageMultiplier = 100f;

    [SerializeField]
    private GameObject projectilePrefab;

    private PlayerStatus ownerStatus;
    private bool isActive;
    private float nextSpawnTime;
    private float speedMultiplier = 1f;
    private Vector3 lastPosition;
    private Vector2 playerVelocity;

    public float LifeTime
    {
        get => lifeTime;
        set => lifeTime = Mathf.Max(0.05f, value);
    }

    public float Cooldown
    {
        get => cooldown;
        set => cooldown = Mathf.Max(0.01f, value);
    }

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public GameObject ProjectilePrefab
    {
        get => projectilePrefab;
        set => projectilePrefab = value;
    }

    private void Awake()
    {
        ownerStatus = GetComponent<PlayerStatus>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        UpdatePlayerVelocity();

        if (GameplayPauseState.IsGameplayPaused || !isActive)
        {
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("SawController: ProjectilePrefab is not assigned.");
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnSaw();
        nextSpawnTime = Time.time + Mathf.Max(0.01f, cooldown);
    }

    public void Activate(float speedScalePercent)
    {
        isActive = true;
        speedMultiplier *= Mathf.Max(0f, speedScalePercent) * 0.01f;
        nextSpawnTime = Mathf.Min(nextSpawnTime, Time.time);
    }

    public float CalculateDamage()
    {
        float attack = ownerStatus != null ? ownerStatus.CurrentAttack : 0f;
        return Mathf.Max(0f, attack * Mathf.Max(0f, damageMultiplier) * 0.01f);
    }

    private void SpawnSaw()
    {
        Vector2 direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.right;
        }

        float finalSpeed = Mathf.Max(0f, speed) * Mathf.Max(0f, speedMultiplier);

        GameObject projectileObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        if (!projectileObject.TryGetComponent<SawProjectile>(out SawProjectile projectile))
        {
            Debug.LogWarning("SawController: ProjectilePrefab does not have SawProjectile component.");
            Destroy(projectileObject);
            return;
        }

        projectile.Initialize(this, direction.normalized, finalSpeed, Mathf.Max(0.05f, lifeTime));
    }

    private void UpdatePlayerVelocity()
    {
        Vector3 currentPosition = transform.position;
        float dt = Time.deltaTime;

        if (dt > 0f)
        {
            Vector3 delta = currentPosition - lastPosition;
            playerVelocity = new Vector2(delta.x / dt, delta.y / dt);
        }

        lastPosition = currentPosition;
    }

    public Vector2 GetPlayerVelocity()
    {
        return playerVelocity;
    }

    private void OnValidate()
    {
        lifeTime = Mathf.Max(0.05f, lifeTime);
        cooldown = Mathf.Max(0.01f, cooldown);
        speed = Mathf.Max(0f, speed);
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
    }
}
