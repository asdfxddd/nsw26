using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMagnetCollector : MonoBehaviour
{
    [SerializeField, Tooltip("자석 흡수 대상 태그")]
    private string collectibleTag = "MagnetCollectible";

    [SerializeField, Tooltip("자석 흡수 검색용 레이어 마스크")]
    private LayerMask collectibleMask = ~0;

    [SerializeField, Tooltip("매 프레임 최대 처리 가능한 아이템 수")]
    private int maxCollectiblesPerFrame = 128;

    private float temporaryRadiusMultiplier = 1f;
    private float temporarySpeedMultiplier = 1f;

    private readonly Collider2D[] overlapResults = new Collider2D[256];
    private PlayerStatus playerStatus;
    private CollectibleController collectibleController;

    private void Awake()
    {
        playerStatus = GetComponent<PlayerStatus>();
        collectibleController = GetComponent<CollectibleController>();
    }

    private void Update()
    {
        if (GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        if (playerStatus == null)
        {
            playerStatus = GetComponent<PlayerStatus>();
            if (playerStatus == null)
            {
                return;
            }
        }

        if (collectibleController == null)
        {
            collectibleController = GetComponent<CollectibleController>();
            if (collectibleController == null)
            {
                return;
            }
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            GetEffectivePickupRadius(),
            overlapResults,
            collectibleMask);

        int processCount = Mathf.Min(hitCount, Mathf.Min(maxCollectiblesPerFrame, overlapResults.Length));
        for (int i = 0; i < processCount; i++)
        {
            Collider2D collectibleCollider = overlapResults[i];
            if (collectibleCollider == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(collectibleTag) && !string.Equals(collectibleCollider.tag, collectibleTag))
            {
                continue;
            }

            IMagnetCollectible collectible = collectibleCollider.GetComponent<IMagnetCollectible>();
            if (collectible == null || collectible.IsCollected)
            {
                continue;
            }

            if (!collectibleController.TryGetSettings(collectibleCollider.tag, out float configuredMoveSpeed, out float configuredDistance))
            {
                continue;
            }

            float moveSpeed = configuredMoveSpeed * temporarySpeedMultiplier;
            collectible.BeginMagnetAttraction(transform, moveSpeed, configuredDistance);
        }
    }

    public void SetTemporaryMagnetBoost(float radiusMultiplier, float speedMultiplier)
    {
        temporaryRadiusMultiplier = Mathf.Max(0.01f, radiusMultiplier);
        temporarySpeedMultiplier = Mathf.Max(0.01f, speedMultiplier);
    }

    public void ClearTemporaryMagnetBoost()
    {
        temporaryRadiusMultiplier = 1f;
        temporarySpeedMultiplier = 1f;
    }

    private float GetEffectivePickupRadius()
    {
        if (playerStatus == null)
        {
            return 0.01f;
        }

        return playerStatus.CurrentPickupRadius * temporaryRadiusMultiplier;
    }

    private void OnDrawGizmosSelected()
    {
        PlayerStatus status = playerStatus != null ? playerStatus : GetComponent<PlayerStatus>();
        if (status == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, status.CurrentPickupRadius * temporaryRadiusMultiplier);
    }

    private void OnValidate()
    {
        maxCollectiblesPerFrame = Mathf.Clamp(maxCollectiblesPerFrame, 1, overlapResults.Length);
        temporaryRadiusMultiplier = Mathf.Max(0.01f, temporaryRadiusMultiplier);
        temporarySpeedMultiplier = Mathf.Max(0.01f, temporarySpeedMultiplier);
    }
}
