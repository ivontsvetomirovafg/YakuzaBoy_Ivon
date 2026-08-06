using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] 
    private float moveSpeed;

    [Header("Salto")]
    [SerializeField] 
    private float jumpForce;
    [SerializeField] 
    private float groundDistance = 0.2f;

    [SerializeField]
    private bool doubleJump = true;    
    [SerializeField]
    private float doubleJumpCooldown = 1f;  
    private float doubleJumpTimer;

    [Header("Pared")]
    [SerializeField]
    private float wallJumpForceX = 8f;
    [SerializeField]
    private float wallJumpForceY = 12f;

    private bool isTouchingWall;
    private bool isWallStuck;
    private Vector2 wallNormal;

    [Header("Animacion")]
    [SerializeField] 
    private Animator animator;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput == 0)
        {
            animator.SetBool("Run", false);
        }
        else
        {
            animator.SetBool("Run", true);
        }

        if (isWallStuck == false)
        {
            if (moveInput < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else if (moveInput > 0)
            {
                transform.eulerAngles = Vector3.zero;
            }
        }
        else
        {
            transform.eulerAngles = wallNormal.x > 0 ? Vector3.zero : new Vector3(0, 180, 0); // REVISAR
        }

        if (Input.GetButtonDown("Jump") == true)
        {
            if (isGrounded == true)
            {
                Jump();
            }
            else if (doubleJump == true)
            {
                Jump();
                doubleJump = false;  
                doubleJumpTimer = 0f;
            }
        }

        if (doubleJump == false)
        {
            doubleJumpTimer += Time.deltaTime;
            if (doubleJumpTimer >= doubleJumpCooldown)
            {
                doubleJump = true;
            }
        }
        CheckGrounded();
     
        isWallStuck  = isTouchingWall && isGrounded == false && rb.linearVelocity.y < 0f;
        animator.SetBool("Wall", isWallStuck);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (isWallStuck == true)
        {
            rb.linearVelocity = Vector2.zero; 
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
            wallNormal = collision.GetContact(0).normal; //preguntar
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("JumpStart");
    }

    void WallJump()
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(wallNormal.x * wallJumpForceX, wallJumpForceY), ForceMode2D.Impulse);

        doubleJump = true; 
        animator.SetTrigger("JumpStart");
    }

    void CheckGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, groundDistance);
        isGrounded = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].transform.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }

        if (isGrounded == true)
        {
            animator.SetBool("Jump", false);
        }
        else
        {
            animator.SetBool("Jump", true);
        }
    }
}
