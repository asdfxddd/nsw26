using UnityEngine;

public abstract class MagnetCollectible : MonoBehaviour, IMagnetCollectible
{
    [SerializeField, Tooltip("자석 흡수 이동 속도")]
    private float magnetMoveSpeed = 10f;

    [SerializeField, Tooltip("플레이어 도달 판정 거리")]
    private float collectDistance = 0.05f;

    private Transform attractionTarget;
    private bool isMagnetized;
    private bool isCollected;

    public bool IsCollected => isCollected;

    public void BeginMagnetAttraction(Transform target, float moveSpeed)
    {
        if (target == null || isCollected)
        {
            return;
        }

        attractionTarget = target;
        isMagnetized = true;

        if (moveSpeed > 0f)
        {
            magnetMoveSpeed = moveSpeed;
        }
    }

    protected virtual void Update()
    {
        if (GameplayPauseState.IsGameplayPaused || !isMagnetized || isCollected || attractionTarget == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            attractionTarget.position,
            magnetMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, attractionTarget.position) <= collectDistance)
        {
            isCollected = true;
            OnCollected();
            Destroy(gameObject);
        }
    }

    protected abstract void OnCollected();

    protected virtual void OnValidate()
    {
        magnetMoveSpeed = Mathf.Max(0.01f, magnetMoveSpeed);
        collectDistance = Mathf.Max(0.01f, collectDistance);
    }
}
