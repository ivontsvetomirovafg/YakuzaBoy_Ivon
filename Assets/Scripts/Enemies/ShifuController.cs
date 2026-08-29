using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShifuController : MonoBehaviour
{
    [Header("Deteccion")]
    public float detectionRange;
    [SerializeField]
    private float closeRange;

    [SerializeField]
    private float chaseSpeed;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeed;       
    [SerializeField]
    private float dashDuration;      
    [SerializeField]
    private float dashCooldown;     
    private float dashTimer;
    private bool isDashing;
    private Vector2 dashDirection;

    [Header("Vida")]
    public int maxLife = 3;
    public int currentLife;
    [SerializeField]
    private GameObject[] hearts;

    [Header("Ataque")]
    public float damage;

    [Header("Animacion")]
    public Animator animator;

    [Header("Audio")]
    [SerializeField]
    private AudioClip deathSFX;
    [SerializeField]
    private AudioClip attackSFX;
    [SerializeField]
    private AudioClip hitSFX;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerController playerController;
    private bool playerDetected;
    private bool alterao;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();

        currentLife = maxLife;
    }

    void Update()
    {
        if (currentLife <= 0)
        {
            return;
        }

        if (playerController.isDead == true)
        {
            return;
        }

        CheckPlayer();

        if (playerDetected == false)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Run", false);
            return;
        }

        if (isDashing == true)
        {
            return;
        }

        float distanceSq = (player.position - transform.position).sqrMagnitude;
        float closeRangeSq = closeRange * closeRange;

        if (distanceSq > closeRangeSq)
        {
            Chase();
            dashTimer = 0f;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Run", false);

            if (player.position.x < transform.position.x)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else
            {
                transform.eulerAngles = Vector3.zero;
            }

            dashTimer += Time.deltaTime;

            if (dashTimer >= dashCooldown)
            {
                dashTimer = 0f;
                StartCoroutine(Dash());
            }
        }
    }

    void CheckPlayer()
    {
        if (alterao == true)
        {
            playerDetected = true;
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        playerDetected = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                playerDetected = true;
                player = colliders[i].transform;
            }
        }
    }

    void Chase()
    {
        animator.SetBool("Run", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        if (direction.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (direction.x > 0)
        {
            transform.eulerAngles = Vector3.zero;
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashDirection = (player.position - transform.position).normalized; //esto para guardar la pos del personaje
        animator.SetTrigger("Attack");
        rb.linearVelocity = dashDirection * dashSpeed;
        AudioManager.Instance.PlaySFX(attackSFX);

        yield return new WaitForSeconds(3f);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController collidedPlayer = collision.gameObject.GetComponent<PlayerController>();
            collidedPlayer.TakePlayerDamage(damage);
        }
    }

    public void TakeDamage(float _damage)
    {
        alterao = true;

        if (playerDetected == false)
        {
            playerDetected = true;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        currentLife--;
        UpdateHearts();

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

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentLife)
            {
                hearts[i].SetActive(true);
            }
            else
            {
                hearts[i].SetActive(false);
            }
        }
    }

    void Die()
    {
        AudioManager.Instance.PlaySFX(deathSFX);
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;

        Destroy(gameObject, 1f);
    }
}
