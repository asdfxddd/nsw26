using UnityEngine;

public class MonsterChase : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1.5f;

    [SerializeField]
    private Transform target;

    private void Awake()
    {
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
    }
}
