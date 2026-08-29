using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] 
    private float moveSpeed;
    public bool canMove = true;

    [Header("Vida")]
    public float maxLife;
    public float currentLife;
    public bool isDead = false; 
    [SerializeField] 
    private int killCount;

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
    private float wallJumpForceX;
    [SerializeField]
    private float wallJumpForceY;
    [SerializeField]
    private float wallJumpDuration = 0.08f;

    private bool isTouchingWall;
    private bool isWallStuck;
    private Vector2 wallNormal;
    private float wallJumpTimer;
    
    [Header("Agachar")]
    [SerializeField]
    private float radioDetectTecho;  
    [SerializeField]
    private Vector2 desplazamientoDetectTecho;      

    private bool techoBloqueado;              
    private bool isCrouching;
    
    [Header("Ataque")]
    public float damage;

    //enemiigo que relentiza: 
    private int slowHits;

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;   
    public float fireRate = 0.3f;  
    private float fireTime;

    [Header("Animacion")]
    [SerializeField] 
    private Animator animator;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    [Header("UI")]
    [SerializeField]
    private Image lifeBar;
    [SerializeField]
    private Text killsText; 

    [Header("Audio")]
    [SerializeField]
    private AudioClip deathSFX;
    [SerializeField]
    private AudioClip runSFX;
    [SerializeField]
    private AudioClip shootSFX;
    [SerializeField]
    private AudioClip jumpSFX;
    [SerializeField]
    private AudioClip hitSFX;

    private LevelManager levelManager; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }    
    
    private void Start()
    {
        UpdateLife();

        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        killCount = PlayerPrefs.GetInt("KillCount", 0); //Guarda datos entre partidas.
        UpdateKillCount();

        if (PlayerPrefs.HasKey("SpawnX")) //EXPLICAR
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }

    void Update()
    {
        //TEMPORAL//

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteKey("SpawnX");
            PlayerPrefs.DeleteKey("SpawnY");
            PlayerPrefs.DeleteKey("KillCount");
            PlayerPrefs.Save();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        if (canMove == false)
        {
            return; 
        }

        if (currentLife <=0)
        {
            return;
        }

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
            if (wallNormal.x > 0)
            {
                transform.eulerAngles = Vector3.zero;
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
        }

        if (Input.GetButtonDown("Jump") == true)
        {
            if (isWallStuck == true)
            {
                WallJump();
            }

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
        Attack();
        CheckGrounded();
        CheckCrouch(); 

        if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
            isWallStuck = false;
        }
        else
        {
            isWallStuck = isTouchingWall && isGrounded == false;
        }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obst"))
        {
            if (isDead == true)
            {
                return;
            }

            currentLife = 0;
            UpdateLife();
            Die();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
            wallNormal = collision.GetContact(0).normal;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
        }
    }

    // ATAQUE //

    private void Attack()
    {
        if (currentLife <=0)
        {
            return;
        }

        if (isGrounded == false)
        {
            return; 
        }
        
        if (Input.GetMouseButtonDown(1) && Time.time >= fireTime + fireRate) 
        {
            Shoot();
            animator.SetTrigger("Attack");
            fireTime = Time.time;
        }
    }

    private void Shoot()
    {
        AudioManager.Instance.PlaySFX(shootSFX);
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bullet = bulletObj.GetComponent<BulletController>();
        bullet.damage = damage;     
        Vector2 direction;

        if (transform.eulerAngles.y == 0)
        {
            direction = Vector2.right;
        }
        else
        {
            direction = Vector2.left;
        }        
        bullet.SetDirection(direction);
    }

    public void SlowHit(float slowMultiplier, float slowDuration)
    {
        if (isDead == true)
        {
            return;
        }

        slowHits++;

        if (slowHits < 3)
        {
            StartCoroutine(SlowEffect(slowMultiplier, slowDuration));
            animator.SetTrigger("Hit");
        }
        else
        {
            currentLife = 0;
            Die();
        }
    }

    private IEnumerator SlowEffect(float slowMultiplier, float slowDuration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = moveSpeed * slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        moveSpeed = originalSpeed;
    }

    public void TakePlayerDamage(float _damage)
    {
        if (isDead == true)
        {
            return; 
        }

        currentLife -= _damage;
        UpdateLife();

        if (currentLife <= 0)
        {
            Die();
        }
        else
        {
            AudioManager.Instance.PlaySFX(hitSFX);
            animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        AudioManager.Instance.PlaySFX(deathSFX);
        animator.SetTrigger("Death");
        enabled = false; 

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn() // EXPLICAR
    {
        yield return new WaitForSeconds(2f);

        killCount++;
        PlayerPrefs.SetInt("KillCount", killCount);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // SALTO //

    void Jump()
    {
        AudioManager.Instance.PlaySFX(jumpSFX);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("JumpStart");
    }

    void WallJump()
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(wallNormal.x * wallJumpForceX, wallJumpForceY), ForceMode2D.Impulse);

        AudioManager.Instance.PlaySFX(jumpSFX);
        doubleJump = true;
        isWallStuck = false;
        wallJumpTimer = wallJumpDuration;
        animator.SetTrigger("JumpStart");
    }

    // AGACHAR //

    void CheckCrouch()
    {
        Vector2 checkPosition = (Vector2)transform.position + desplazamientoDetectTecho;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPosition, radioDetectTecho);
        techoBloqueado = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Wall"))
            {
                techoBloqueado = true;
            }
        }

        bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (wantsToCrouch == true || techoBloqueado == true)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        animator.SetBool("Agachar", isCrouching);

        if (isCrouching == true && moveInput == 0)
        {
            animator.speed = 0f;
        }
        else
        {
            animator.speed = 1f;
        }
    }
    //

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

    // EXPLICAR --> PlayerPrefs //
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Spawn")
        {
            levelManager.spawnPoint = collision.transform;
        
            PlayerPrefs.SetFloat("SpawnX", collision.transform.position.x);
            PlayerPrefs.SetFloat("SpawnY", collision.transform.position.y);
            PlayerPrefs.Save();
        }
        else if (collision.gameObject.tag == "Door")
        {
            levelManager.FinishLevel();
        }
    }

    public void UpdateLife()
    {
        lifeBar.fillAmount = currentLife / maxLife;
    }

    public void UpdateKillCount()
    {
        killsText.text = "x" + killCount.ToString();
    }
}
