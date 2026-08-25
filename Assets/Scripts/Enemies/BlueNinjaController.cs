using UnityEngine;
using System.Collections;

public class BlueNinjaController : EnemyController
{
    [Header("Rayo")]
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform firePoint;

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        if (currentLife <= 0)
        {
            return;
        }

        if (playerController.isDead == true)
        {
            attacking = false;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Run", false);
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

    void Attack()
    {
        if (Time.time >= attackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");
            attackTime = Time.time;
            StartCoroutine(AnimShoot());
        }
    }

    private IEnumerator AnimShoot()
    {
        yield return new WaitForSeconds(0.5f);
        Shoot();
    }

    void Shoot()
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
}
