using UnityEngine;

public class MonsterChase : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1.5f;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private Transform visualRoot;

    private float baseScaleX = 1f;

    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        baseScaleX = Mathf.Abs(visualRoot.localScale.x);

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void Update()
    {
        if (TimeStopController.IsTimeStopped)
        {
            return;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }

            if (target == null)
            {
                return;
            }
        }

        Vector3 direction = (target.position - transform.position);
        direction.z = 0f;
        if (direction.sqrMagnitude <= 0f)
        {
            return;
        }

        Vector3 delta = direction.normalized * moveSpeed * Time.deltaTime;
        transform.position += delta;

        UpdateFacing(direction.x);
    }

    private void UpdateFacing(float directionX)
    {
        if (Mathf.Approximately(directionX, 0f))
        {
            return;
        }

        Vector3 scale = visualRoot.localScale;
        scale.x = directionX > 0f ? baseScaleX : -baseScaleX;
        visualRoot.localScale = scale;
    }
}
