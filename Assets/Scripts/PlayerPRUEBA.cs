using UnityEngine;

public class PlayerPRUEBA : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] 
    private float moveSpeed;

    [Header("Vida")]
    public float maxLife;
    public float currentLife;

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
    private float wallJumpForceX; // fuerza horizontal del salto al saltar desde la pared

    [SerializeField]
    private float wallJumpForceY; // fuerza vertical del salto al saltar desde la pared

    [SerializeField]
    private float wallJumpDuration = 0.25f; // cuanto tiempo, tras saltar de la pared, ignoramos que sigamos tocandola (para poder despegarnos)

    private bool isTouchingWall; // true mientras el collider del jugador esta en contacto fisico con algo con tag "Wall"
    private bool isWallStuck; // true solo cuando ademas de tocar la pared, estamos en el aire (no en el suelo) -> aqui es cuando "nos pegamos" a la pared
    private Vector2 wallNormal; // direccion perpendicular a la pared (nos dice hacia que lado "empuja" la pared al jugador)
    private float wallJumpTimer; // cuenta atras activa justo despues de un wall jump

    [Header("Ataque")]
    public float baseDamage;
    public float damage;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }    
    
    private void Start()
    {
        damage = baseDamage;

        UpdateLife();
        UpdateDamage();
    }

    void Update()
    {
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
        
        // Si NO estamos pegados a la pared, el giro depende del input del jugador (como siempre)
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
            // Si SI estamos pegados a la pared, ignoramos el input y giramos segun la pared:
            // queremos que el personaje siempre mire "hacia fuera" de la pared, no hacia dentro.
            // wallNormal.x > 0 significa que la pared esta a nuestra izquierda empujando hacia la derecha,
            // por eso miramos a la derecha (0 grados); si no, miramos a la izquierda (180 grados).
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
            // Comprobamos primero si estamos pegados a la pared: el wall jump tiene prioridad
            // sobre el salto normal y el doble salto.
            if (isWallStuck == true)
            {
                WallJump();
            }
            // "else if" (no un "if" aparte) para que solo se ejecute UNA de las 3 opciones de salto por pulsacion,
            // evitando que se acumulen fuerzas si varias condiciones fueran true a la vez.
            else if (isGrounded == true)
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

        // Este bloque decide, cada frame, si isWallStuck debe estar activo o no.
        if (wallJumpTimer > 0f)
        {
            // Justo despues de un wall jump, restamos tiempo al timer y forzamos isWallStuck a false.
            // Esto evita que, al seguir tocando la pared un instante tras saltar, nos volvamos a pegar
            // inmediatamente sin poder despegarnos.
            wallJumpTimer -= Time.deltaTime;
            isWallStuck = false;
        }
        else
        {
            // Pasado ese tiempo de "gracia", volvemos a la logica normal:
            // estamos pegados si tocamos pared Y no estamos en el suelo.
            isWallStuck = isTouchingWall && isGrounded == false;
        }

        // Le decimos al Animator si debe reproducir la animacion de "pegado a la pared" o no.
        animator.SetBool("Wall", isWallStuck);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Si estamos pegados a la pared, anulamos toda la velocidad (X e Y) para que el jugador
        // se quede quieto "clavado" ahi, en vez de caer por gravedad o moverse con el input.
        if (isWallStuck == true)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Se llama automaticamente cada frame que el collider del jugador sigue tocando otro collider.
    // La usamos (en vez de OnCollisionEnter2D) porque queremos saber en TODO momento si seguimos
    // tocando la pared, no solo en el instante en que empezamos a tocarla.
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
            // GetContact(0).normal nos da la direccion perpendicular a la superficie de la pared,
            // apuntando hacia el jugador. La guardamos para saber hacia donde "empujar" en el wall jump.
            wallNormal = collision.GetContact(0).normal;
        }
    }

    // Se llama automaticamente en el frame en que dejamos de tocar ese collider.
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

    public void TakePlayerDamage(float _damage)
    {
        currentLife -= _damage;
        UpdateLife();
    }

    // SALTO //

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("JumpStart");
    }

    void WallJump()
    {
        // Ponemos la velocidad a cero antes de aplicar la fuerza, igual que en Jump(),
        // para que el impulso sea siempre consistente y no se sume a la velocidad que ya llevaba.
        rb.linearVelocity = Vector2.zero;

        // wallNormal.x nos dice hacia que lado "empuja" la pared (positivo o negativo),
        // asi que multiplicarlo por wallJumpForceX hace que siempre saltemos ALEJANDONOS
        // de la pared correcta, sin importar si es la pared izquierda o la derecha.
        rb.AddForce(new Vector2(wallNormal.x * wallJumpForceX, wallJumpForceY), ForceMode2D.Impulse);

        // Al saltar de la pared, recuperamos el doble salto, para poder encadenar
        // pared -> pared -> doble salto si el jugador quiere seguir subiendo.
        doubleJump = true;

        // Nos "despegamos" inmediatamente de la pared...
        isWallStuck = false;

        // ...y activamos el timer de gracia (wallJumpDuration) para que, aunque sigamos
        // tocando fisicamente la pared un instante, no nos volvamos a pegar a ella.
        wallJumpTimer = wallJumpDuration;

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

    public void UpdateLife()
    {
        //lifeBar.fillAmount = currentLife / maxLife;
        //lifeText.text = "LIFE: " + currentLife + " / " + maxLife;      
    }

    public void UpdateDamage()
    {
        //damageText.text = "DMG: " + damage;
    }
}