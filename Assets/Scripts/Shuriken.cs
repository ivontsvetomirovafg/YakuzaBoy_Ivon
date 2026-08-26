using UnityEngine;

public class Shuriken : MonoBehaviour
{
    public GameObject shurikenPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float fireTime;

    void Update()
    {
        if (Time.time >= fireTime + fireRate)
        {
            Shoot();
            fireTime = Time.time;
        }
    }

    void Shoot()
    {
        GameObject shurikenObj = Instantiate(shurikenPrefab, firePoint.position, Quaternion.identity);
        BulletController shuriken = shurikenObj.GetComponent<BulletController>();
        shuriken.damage = 1f; 

        Vector2 direction;
        if (transform.eulerAngles.y == 0)
        {
            direction = Vector2.left;
        }
        else
        {
            direction = Vector2.right;
        }
        shuriken.SetDirection(direction);
    }
}
