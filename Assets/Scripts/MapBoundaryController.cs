using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MapBoundaryController : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D boundaryCollider;

    public Bounds WorldBounds
    {
        get
        {
            if (boundaryCollider == null)
            {
                boundaryCollider = GetComponent<BoxCollider2D>();
            }

            return boundaryCollider.bounds;
        }
    }

    private void Awake()
    {
        if (boundaryCollider == null)
        {
            boundaryCollider = GetComponent<BoxCollider2D>();
        }
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        Bounds bounds = WorldBounds;
        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
        return position;
    }
}
