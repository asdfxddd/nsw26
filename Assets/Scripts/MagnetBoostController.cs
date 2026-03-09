using UnityEngine;

[DisallowMultipleComponent]
public class MagnetBoostController : MonoBehaviour
{
    [SerializeField, Tooltip("자석 부스트 지속 시간(초)")]
    private float boostDuration = 5f;

    [SerializeField, Tooltip("자석 흡수 속도 배수")]
    private float boostSpeedMultiplier = 3f;

    [SerializeField, Tooltip("자석 흡수 반경 배수")]
    private float boostRadiusMultiplier = 100f;

    [SerializeField, Tooltip("자석 흡수 대상 태그")]
    private string collectibleTag = "MagnetCollectible";

    [SerializeField, Tooltip("전체 씬 재탐색 주기(초)")]
    private float rescanInterval = 0.15f;

    private PlayerMagnetCollector magnetCollector;
    private PlayerStatus playerStatus;
    private float remainingTime;
    private float rescanTimer;
    private bool isActive;

    private void Awake()
    {
        magnetCollector = GetComponent<PlayerMagnetCollector>();
        playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (!isActive || GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        rescanTimer -= Time.deltaTime;

        if (rescanTimer <= 0f)
        {
            ApplyBoostToAllCollectibles();
            rescanTimer = Mathf.Max(0.01f, rescanInterval);
        }

        if (remainingTime <= 0f)
        {
            EndBoost();
        }
    }

    public void MagnetBoost()
    {
        if (magnetCollector == null)
        {
            magnetCollector = GetComponent<PlayerMagnetCollector>();
        }

        if (playerStatus == null)
        {
            playerStatus = GetComponent<PlayerStatus>();
        }

        if (magnetCollector == null || playerStatus == null)
        {
            return;
        }

        remainingTime = Mathf.Max(0.01f, boostDuration);
        rescanTimer = 0f;
        isActive = true;

        magnetCollector.SetTemporaryMagnetBoost(boostRadiusMultiplier, boostSpeedMultiplier);
        ApplyBoostToAllCollectibles();
    }

    private void ApplyBoostToAllCollectibles()
    {
        if (string.IsNullOrEmpty(collectibleTag))
        {
            return;
        }

        GameObject[] allCollectibles = GameObject.FindGameObjectsWithTag(collectibleTag);
        float boostedSpeed = playerStatus.PickupMoveSpeed * Mathf.Max(0.01f, boostSpeedMultiplier);

        for (int i = 0; i < allCollectibles.Length; i++)
        {
            GameObject collectibleObject = allCollectibles[i];
            if (collectibleObject == null)
            {
                continue;
            }

            IMagnetCollectible collectible = collectibleObject.GetComponent<IMagnetCollectible>();
            if (collectible == null || collectible.IsCollected)
            {
                continue;
            }

            collectible.BeginMagnetAttraction(transform, boostedSpeed);
        }
    }

    private void EndBoost()
    {
        isActive = false;
        remainingTime = 0f;
        rescanTimer = 0f;

        if (magnetCollector != null)
        {
            magnetCollector.ClearTemporaryMagnetBoost();
        }
    }

    private void OnDisable()
    {
        EndBoost();
    }

    private void OnValidate()
    {
        boostDuration = Mathf.Max(0.01f, boostDuration);
        boostSpeedMultiplier = Mathf.Max(0.01f, boostSpeedMultiplier);
        boostRadiusMultiplier = Mathf.Max(0.01f, boostRadiusMultiplier);
        rescanInterval = Mathf.Max(0.01f, rescanInterval);
    }
}
