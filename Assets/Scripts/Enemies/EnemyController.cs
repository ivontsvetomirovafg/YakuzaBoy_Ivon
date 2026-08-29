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
    public int maxLife = 3;
    public int currentLife;
    public GameObject[] hearts;

    [Header("Ataque")]
    public float damage;
    public float attackCooldown;
    public float attackTime;
    public bool attacking;
    public bool alterao;

    [Header("Animacion")]
    public Animator animator;

    public Rigidbody2D rb;
    public Transform player;
    public PlayerController playerController;
    public bool playerDetected;
    public bool otherNinja = false; 

    [Header("Audio")]
    public AudioClip deathSFX;
    public AudioClip attackSFX;
    public AudioClip hitSFX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();

        currentLife = maxLife;
    }

    public void Update()
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

    public void CheckPlayer()
    {
        if (alterao == true)
        {
            playerDetected = true;
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        //playerDetected = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                playerDetected = true;
                player = colliders[i].transform;
            }
        }
    }

    public void Chase()
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
            AudioManager.Instance.PlaySFX(attackSFX);

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
        if (otherNinja == true)
        {
            AudioManager.Instance.PlaySFX(deathSFX);
            animator.SetTrigger("Death");
            rb.linearVelocity = Vector2.zero;
            this.enabled = false;

            Destroy(gameObject, 1.5f);
        }

        else 
        {
            AudioManager.Instance.PlaySFX(deathSFX);
            animator.SetTrigger("Hit");
            rb.linearVelocity = Vector2.zero;
            this.enabled = false;

            Destroy(gameObject, 0.5f);
        }
        
    }
}