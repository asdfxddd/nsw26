using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
public class FlameController : MonoBehaviour
{
    [SerializeField]
    private FlameProjectile flamePrefab;

    [SerializeField]
    private float damageMultiplier = 1f;

    [SerializeField]
    private float knockbackDistance = 1f;

    [SerializeField]
    private float coolTime = 1f;

    [SerializeField, Tooltip("플레이어 중심으로부터 화염방사체를 배치할 전방 거리")]
    private float forwardOffset = 0.75f;

    private PlayerStatus ownerStatus;
    private FlameProjectile activeProjectile;
    private bool isActive;
    private float durationSeconds = 1f;
    private float nextSpawnTime;
    private Vector2 lastFacingDirection = Vector2.right;

    public FlameProjectile FlamePrefab
    {
        get => flamePrefab;
        set => flamePrefab = value;
    }

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public float KnockbackDistance
    {
        get => knockbackDistance;
        set => knockbackDistance = Mathf.Max(0f, value);
    }

    public float CoolTime
    {
        get => coolTime;
        set => coolTime = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        ownerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        UpdateFacingDirection();

        if (!isActive)
        {
            return;
        }

        if (activeProjectile == null && Time.time >= nextSpawnTime)
        {
            SpawnFlameProjectile();
        }
    }

    public void ActivateFlameThrower(int durationMilliseconds)
    {
        isActive = true;
        durationSeconds = Mathf.Max(0.01f, durationMilliseconds * 0.001f);

        if (activeProjectile == null)
        {
            nextSpawnTime = Mathf.Min(nextSpawnTime, Time.time);
        }
    }

    private void SpawnFlameProjectile()
    {
        if (flamePrefab == null)
        {
            Debug.LogWarning("FlameController: FlamePrefab is not assigned.");
            return;
        }

        Vector2 facing = lastFacingDirection.sqrMagnitude > 0f ? lastFacingDirection.normalized : Vector2.right;
        Vector3 spawnPosition = transform.position + (Vector3)(facing * forwardOffset);

        activeProjectile = Instantiate(flamePrefab, spawnPosition, Quaternion.identity, transform);
        activeProjectile.Configure(this, durationSeconds);
        activeProjectile.UpdateFacing(facing, forwardOffset);

        nextSpawnTime = Time.time + Mathf.Max(0f, coolTime) + durationSeconds;
    }

    private void UpdateFacingDirection()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 0f)
        {
            lastFacingDirection = input.normalized;
        }

        if (activeProjectile != null)
        {
            activeProjectile.UpdateFacing(lastFacingDirection, forwardOffset);
        }
    }

    public float CalculateDamage()
    {
        float attack = ownerStatus != null ? ownerStatus.CurrentAttack : 0f;
        return Mathf.Max(0f, attack * Mathf.Max(0f, damageMultiplier));
    }

    public Vector2 GetKnockbackDirection()
    {
        Vector2 facing = lastFacingDirection.sqrMagnitude > 0f ? lastFacingDirection.normalized : Vector2.right;
        return -facing;
    }

    public void NotifyProjectileDestroyed(FlameProjectile projectile)
    {
        if (activeProjectile == projectile)
        {
            activeProjectile = null;
        }
    }

    private void OnValidate()
    {
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        knockbackDistance = Mathf.Max(0f, knockbackDistance);
        coolTime = Mathf.Max(0f, coolTime);
        forwardOffset = Mathf.Max(0f, forwardOffset);
    }
}
