using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] 
    private float moveSpeed;
    private float baseMoveSpeed; 
    private Coroutine slowCoroutine;

    public bool canMove = true;

    [Header("Vida")]
    public float maxLife;
    public float currentLife;
    public bool isDead = false; 
    [SerializeField] 
    private int killCount;
    [SerializeField] 
    private int coinsCount;
    private string monedasRecogidas = "";

    [Header("Salto")]
    [SerializeField] 
    private float jumpForce;
    [SerializeField] 
    private float groundDistance = 0.2f;

    [SerializeField]
    private bool doubleJump = true;    

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
    private bool canWallJump = true;
    
    [Header("Agachar")]
    [SerializeField]
    private float radioDetectTecho;  
    [SerializeField]
    private Vector2 desplazamientoDetectTecho;   

    [SerializeField]
    private float crouchColliderHeight; // tama�o Y del collider agachado.
    [SerializeField]
    private float crouchColliderOffsetY; // offset Y del collider agachado.

    private bool techoBloqueado;
    private bool isCrouching;

    private CapsuleCollider2D playerCollider;
    private Vector2 colliderNormal;
    private Vector2 colliderOffset;
    
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
    [SerializeField]
    private Text coinsText; 

    [Header("Audio")]
    [SerializeField]
    private AudioClip deathSFX;
    [SerializeField]
    private AudioClip shootSFX;
    [SerializeField]
    private AudioClip jumpSFX;
    [SerializeField]
    private AudioClip hitSFX;
    [SerializeField]
    private AudioClip coinSFX;
    [SerializeField]
    private AudioClip spawnPointSFX;

    private LevelManager levelManager; 
    [SerializeField]
    private CamController camController;
    private Transform spawnGuardado;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        colliderNormal = playerCollider.size;
        colliderOffset = playerCollider.offset;
        baseMoveSpeed = moveSpeed;
    }    
    
    private void Start()
    {
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        killCount = PlayerPrefs.GetInt("KillCount", 0); //PlayersPrefs = Guarda datos entre partidas.        
        UpdateKillCount();

        coinsCount = PlayerPrefs.GetInt("CoinsCount", 0); // Para leer las monedas guardadas hasta el ultimo spawn.
        coinsText.text = "x" + coinsCount.ToString();

        if (PlayerPrefs.HasKey("SpawnX")) //EXPLICAR
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");

            transform.position = new Vector3(x, y, transform.position.z);
        }
            
        if (PlayerPrefs.HasKey("CamMinX"))
        {
            float camMinX = PlayerPrefs.GetFloat("CamMinX");
            float camMaxX = PlayerPrefs.GetFloat("CamMaxX");
            float camMinY = PlayerPrefs.GetFloat("CamMinY");
            float camMaxY = PlayerPrefs.GetFloat("CamMaxY");

            camController.SetLimits(camMinX, camMaxX, camMinY, camMaxY);
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
            PlayerPrefs.DeleteKey("CoinsCount");
            PlayerPrefs.DeleteKey("CollectedCoins");
            PlayerPrefs.DeleteKey("TiempoTranscurrido");

            PlayerPrefs.DeleteKey("CamMinX");
            PlayerPrefs.DeleteKey("CamMaxX");
            PlayerPrefs.DeleteKey("CamMinY");
            PlayerPrefs.DeleteKey("CamMaxY");

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
            if (isWallStuck == true && canWallJump == true)
            {
                WallJump();
            }    

            else if (isGrounded == true)
            {
                Jump();
            }

            else if (doubleJump == true)
            {
                Jump();
                doubleJump = false;
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
        if (wallJumpTimer <= 0f)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }   

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
            canWallJump = true;
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
        
        if (isCrouching == true)
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

    public void SlowHit(float slowMultiplier, float slowDuration) //EXPLICAR
    {
        if (isDead == true)
        {
            return;
        }

        slowHits++;

        if (slowHits < 3)
        {
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
            }
            slowCoroutine = StartCoroutine(SlowEffect(slowMultiplier, slowDuration));
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
        moveSpeed = baseMoveSpeed * slowMultiplier;
        yield return new WaitForSeconds(slowDuration);
        moveSpeed = baseMoveSpeed; 
        slowCoroutine = null;
    }

    public void TakePlayerDamage(float _damage)
    {
        if (isDead == true)
        {
            return; 
        }

        currentLife -= _damage;

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
        animator.speed = 1f; // para la anim de agachar
        AudioManager.Instance.PlaySFX(deathSFX);
        animator.SetTrigger("Death");
        enabled = false; 

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn() 
    {
        yield return new WaitForSeconds(2f);

        killCount++;
        PlayerPrefs.SetInt("KillCount", killCount);
        levelManager.GuardarTiempoTrans();
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
        doubleJump = false;
        isWallStuck = false;
        canWallJump = false;
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

        bool agachar = Input.GetKey(KeyCode.LeftControl) || techoBloqueado;

        // Solo toca el collider cuando cambia el estado, no cada frame.
        if (agachar != isCrouching)
        {
            isCrouching = agachar;

            if (isCrouching == true)
            {
                playerCollider.size = new Vector2(colliderNormal.x, crouchColliderHeight);
                playerCollider.offset = new Vector2(colliderOffset.x, crouchColliderOffsetY);
            }
            else
            {
                playerCollider.size = colliderNormal;
                playerCollider.offset = colliderOffset;
            }

            animator.SetBool("Agachar", isCrouching);
        }

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
            doubleJump = true;
            canWallJump = true;
        }
        else
        {
            animator.SetBool("Jump", true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            AudioManager.Instance.PlaySFX(coinSFX);
            coinsCount++;
            coinsText.text = "x" + coinsCount.ToString();

            monedasRecogidas += collision.gameObject.name + ",";         
            
            Destroy(collision.gameObject); 
        }

        if (collision.gameObject.CompareTag("Spawn"))
        {
            if (collision.transform != spawnGuardado)
            {
                spawnGuardado = collision.transform;
                levelManager.spawnPoint = collision.transform;
                AudioManager.Instance.PlaySFX(spawnPointSFX);

                PlayerPrefs.SetFloat("SpawnX", collision.transform.position.x);
                PlayerPrefs.SetFloat("SpawnY", collision.transform.position.y);
                PlayerPrefs.SetInt("CoinsCount", coinsCount);

                // guardar la pos de la cam.
                PlayerPrefs.SetFloat("CamMinX", camController.minX);
                PlayerPrefs.SetFloat("CamMaxX", camController.maxX);
                PlayerPrefs.SetFloat("CamMinY", camController.minY);
                PlayerPrefs.SetFloat("CamMaxY", camController.maxY);

                //monedas 
                string monedasGuardadas = PlayerPrefs.GetString("CollectedCoins", "");
                monedasGuardadas += monedasRecogidas;
                PlayerPrefs.SetString("CollectedCoins", monedasGuardadas);
                monedasRecogidas = ""; // Ya quedaron guardadas.

                PlayerPrefs.Save();
            }
        }

        else if (collision.gameObject.CompareTag("Door"))
        {
            levelManager.FinishLevel();
        }
    }

    public void UpdateKillCount()
    {
        killsText.text = "x" + killCount.ToString();
    }
}
