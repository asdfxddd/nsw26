using UnityEngine;

public class ExpOrbController : MonoBehaviour
{
    [SerializeField]
    private int expValue = 1;

    private bool isAbsorbing;

    public int ExpValue => expValue;

    private void Update()
    {
        if (GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        ExpDropManager dropManager = ExpDropManager.Instance;
        if (!dropManager.TryGetPlayerPosition(out Vector3 playerPosition))
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, playerPosition);
        if (!isAbsorbing)
        {
            if (distance > dropManager.MagnetRange)
            {
                return;
            }

            isAbsorbing = true;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            playerPosition,
            dropManager.AbsorbMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, playerPosition) <= 0.05f)
        {
            dropManager.AddExp(expValue);
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        expValue = Mathf.Max(1, expValue);
    }
}
