using UnityEngine;

public class YellowNinjaController : EnemyController
{
    [Header("Golpe que ralentiza")]
    [SerializeField]
    private float slowMultiplier = 0.4f;
    [SerializeField]
    private float slowDuration = 5f;

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentLife <= 0)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController collidedPlayer = collision.gameObject.GetComponent<PlayerController>();
            collidedPlayer.SlowHit(slowMultiplier, slowDuration);
        }
    }
}
