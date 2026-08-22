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
        if (playerController.isDead == true)
        {
            attacking = false;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Run", false);
            return;
        }
        base.Update();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController collidedPlayer = collision.gameObject.GetComponent<PlayerController>();
            collidedPlayer.SlowHit(slowMultiplier, slowDuration);
        }
    }
}
