using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private float smoothTime = 0.15f;

    [SerializeField]
    private float zOffset = -10f;

    [SerializeField]
    private MapBoundaryController mapBoundary;

    private Transform playerTransform;
    private Vector3 velocity;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (mapBoundary == null)
        {
            mapBoundary = FindObjectOfType<MapBoundaryController>();
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            playerTransform = player.transform;
        }

        Vector3 targetPosition = playerTransform.position;
        targetPosition.z = zOffset;

        if (mapBoundary != null)
        {
            targetPosition = mapBoundary.ClampPosition(targetPosition);
            targetPosition.z = zOffset;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
