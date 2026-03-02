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

    private readonly Collider2D[] overlapResults = new Collider2D[256];
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

        if (playerStatus == null)
        {
            playerStatus = GetComponent<PlayerStatus>();
            if (playerStatus == null)
            {
                return;
            }
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            playerStatus.CurrentPickupRadius,
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

            collectible.BeginMagnetAttraction(transform, playerStatus.PickupMoveSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        PlayerStatus status = playerStatus != null ? playerStatus : GetComponent<PlayerStatus>();
        if (status == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, status.CurrentPickupRadius);
    }

    private void OnValidate()
    {
        maxCollectiblesPerFrame = Mathf.Clamp(maxCollectiblesPerFrame, 1, overlapResults.Length);
    }
}
