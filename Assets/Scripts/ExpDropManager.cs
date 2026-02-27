using UnityEngine;

public class ExpDropManager : MonoBehaviour
{
    private static ExpDropManager instance;

    [SerializeField]
    private float magnetRange = 2.5f;

    [SerializeField]
    private float absorbMoveSpeed = 10f;

    [SerializeField]
    private Transform playerTransform;

    public static ExpDropManager Instance
    {
        get
        {
            if (instance == null)
            {
                ExpDropManager existing = FindFirstObjectByType<ExpDropManager>();
                if (existing != null)
                {
                    instance = existing;
                }
                else
                {
                    GameObject managerObject = new GameObject(nameof(ExpDropManager));
                    instance = managerObject.AddComponent<ExpDropManager>();
                }
            }

            return instance;
        }
    }

    public float MagnetRange => magnetRange;

    public float AbsorbMoveSpeed => absorbMoveSpeed;

    public int TotalExp => PlayerExperience.Instance.CurrentExp;

    public int CurrentLevel => PlayerExperience.Instance.CurrentLevel;

    public int ExpIntoCurrentLevel => PlayerExperience.Instance.CurrentExp;

    public int NeedXpForNextLevel => PlayerExperience.Instance.NeedExp;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        _ = PlayerExperience.Instance;
    }

    public bool TryGetPlayerPosition(out Vector3 playerPosition)
    {
        EnsurePlayerTransform();
        if (playerTransform == null)
        {
            playerPosition = default;
            return false;
        }

        playerPosition = playerTransform.position;
        return true;
    }

    public void AddExp(int amount)
    {
        if (GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        PlayerExperience.Instance.AddExp(amount);
    }

    private void EnsurePlayerTransform()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnValidate()
    {
        magnetRange = Mathf.Max(0f, magnetRange);
        absorbMoveSpeed = Mathf.Max(0.01f, absorbMoveSpeed);
    }
}
