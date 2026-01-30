using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f;

    [SerializeField]
    private Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveY = Animator.StringToHash("moveY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool isMoving = moveInput.sqrMagnitude > 0f;

        if (isMoving)
        {
            Vector2 facing = GetCardinalDirection(moveInput);
            lastMoveDirection = facing;
            animator.SetFloat(MoveX, facing.x);
            animator.SetFloat(MoveY, facing.y);
        }
        else
        {
            animator.SetFloat(MoveX, lastMoveDirection.x);
            animator.SetFloat(MoveY, lastMoveDirection.y);
        }

        animator.SetBool(IsMoving, isMoving);
    }

    private void FixedUpdate()
    {
        Vector2 velocity = moveInput;
        if (velocity.sqrMagnitude > 1f)
        {
            velocity = velocity.normalized;
        }

        rb.linearVelocity = velocity * moveSpeed;
    }

    private static Vector2 GetCardinalDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            return new Vector2(Mathf.Sign(input.x), 0f);
        }

        return new Vector2(0f, Mathf.Sign(input.y));
    }
}
