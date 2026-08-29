using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bala")]
    public float speed = 15f;
    public float damage;
    public float lifeTime = 5f;
    public bool enemyBullet;

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifeTime); 
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyBullet == false)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                ShifuController shifu = collision.gameObject.GetComponent<ShifuController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                if (shifu != null)
                {
                    shifu.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                playerController.TakePlayerDamage(damage);
                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}
