using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed;
    public float detectionRange;
    public float stopDistance;

    [Header("Vida")]
    public float maxLife;
    public float currentLife;
    [SerializeField]
    private Image lifeBar;

    [Header("Ataque")]
    public float damage;          
    public float attackCooldown;
    private float attackTime;
    private bool attacking;
    private bool alterao; 

    [Header("Animacion")]
    public Animator animator;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerController playerController;
    private bool playerDetected;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();

        currentLife = maxLife;
        UpdateLife();
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
            attacking = false;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Run", false);
            return;
        }

        float distanceSq = (player.position - transform.position).sqrMagnitude;
        float stopDistanceSq = stopDistance * stopDistance;

        if (attacking == false)
        {
            if (distanceSq <= stopDistanceSq)
            {
                attacking = true;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("Run", false);
                Attack();
            }
            else
            {
                Chase();
            }
        }
        else
        {
            if (distanceSq > stopDistanceSq)
            {
                attacking = false;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                Attack();
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
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (direction.x > 0)
        {
            transform.eulerAngles = Vector3.zero;
        }
    }

    void Attack()
    {
        if (Time.time >= attackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");
            attackTime = Time.time;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            playerController.TakePlayerDamage(damage);
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

        currentLife -= _damage;
        UpdateLife();
        animator.SetTrigger("Hit");

        if (currentLife <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Hit");
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
        Destroy(gameObject);
    }

    public void UpdateLife()
    {
        lifeBar.fillAmount = currentLife / maxLife;
    }
}