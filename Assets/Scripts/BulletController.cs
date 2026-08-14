using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bala")]
    public float speed = 15f;
    public float damage;
    public float lifeTime = 5f;

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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                //enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject); 
        }
    }
}
