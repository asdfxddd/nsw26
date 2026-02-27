using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
public class AutoShooter : MonoBehaviour
{
    [SerializeField]
    private float fireInterval = 0.2f;

    [SerializeField]
    private float minTurnCooldown = 0.1f;

    [SerializeField]
    private ProjectileController projectilePrefab;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private Transform baseRotationReference;

    private Vector2 lastAimDirection = Vector2.right;
    private float nextFireTime;
    private float nextTurnReadyTime;
    private PlayerStatus playerStatus;

    private void Awake()
    {
        playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        UpdateAimDirection();
        TryFire();
    }

    private void UpdateAimDirection()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude <= 0f)
        {
            return;
        }

        Vector2 normalized = input.normalized;
        if (Vector2.Dot(normalized, lastAimDirection) < 0.999f)
        {
            lastAimDirection = normalized;
            nextTurnReadyTime = Time.time + minTurnCooldown;
        }
    }

    private void TryFire()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        if (Time.time < nextFireTime || Time.time < nextTurnReadyTime)
        {
            return;
        }

        Transform spawnTransform = spawnPoint != null ? spawnPoint : transform;
        ProjectileController projectile = Instantiate(projectilePrefab, spawnTransform.position, Quaternion.identity);
        projectile.Initialize(lastAimDirection, baseRotationReference);
        projectile.SetOwnerStatus(playerStatus);

        nextFireTime = Time.time + fireInterval;
    }

    private void OnValidate()
    {
        fireInterval = Mathf.Max(0f, fireInterval);
        minTurnCooldown = Mathf.Max(0f, minTurnCooldown);
    }
}
