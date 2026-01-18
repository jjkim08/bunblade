using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 5f;

    [SerializeField] string runBoolParam = "IsRunning";
    [SerializeField] string speedFloatParam = "Speed";
    [SerializeField] string groundedBoolParam = "IsGrounded";
    [SerializeField] float runThreshold = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (animator != null)
        {
            float horizSpeed = Mathf.Abs(rb.linearVelocity.x);
            animator.SetFloat(speedFloatParam, horizSpeed);
            animator.SetBool(runBoolParam, isGrounded && horizSpeed > runThreshold);
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;

        if (animator != null)
        {
            animator.SetBool(groundedBoolParam, isGrounded);
        }
    }
}
