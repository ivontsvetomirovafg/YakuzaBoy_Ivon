using UnityEngine;

public class ParteFinal : MonoBehaviour
{
    public EnemyController enemyToWatch;
    public GameObject spike;
    public Shuriken shurikenDesactivar;

    private bool triggered;

    void Update()
    {
        if (triggered == true)
        {
            return;
        }

        if (enemyToWatch == null || enemyToWatch.currentLife <= 0)
        {
            triggered = true;
            Disappear();
        }
    }

    void Disappear()
    {
        Collider2D[] spikeColliders = spike.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < spikeColliders.Length; i++)
        {
            spikeColliders[i].enabled = false;
        }

        Animator spikeAnimator = spike.GetComponent<Animator>();
        if (spikeAnimator != null)
        {
            spikeAnimator.SetTrigger("Disappear");
        }
        
        if (shurikenDesactivar != null)
        {
            shurikenDesactivar.enabled = false; 
        }

        Destroy(spike, 2f);
    }
}
