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

    [SerializeField]
    private int totalExp;

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

    public int TotalExp => totalExp;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
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
        if (amount <= 0)
        {
            return;
        }

        totalExp += amount;
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
        totalExp = Mathf.Max(0, totalExp);
    }
}
