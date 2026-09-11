using UnityEngine;

public class ParteFinal : MonoBehaviour
{
    public EnemyController enemigoFinal; 
    public GameObject[] pinchos;       

    private bool triggered;

    void Update()
    {
        if (triggered == true)
        {
            return;
        }

        if (enemigoFinal == null || enemigoFinal.currentLife <= 0)
        {
            triggered = true;
            Disappear();
        }
    }

    void Disappear()
    {
        for (int i = 0; i < pinchos.Length; i++)
        {
            pinchos[i].GetComponent<Collider2D>().enabled = false;

            Animator pinchosAnim = pinchos[i].GetComponent<Animator>();
            if (pinchosAnim != null)
            {
                pinchosAnim.SetTrigger("NoPinchos");
            }

            Destroy(pinchos[i], 2f);
        }
    }
}
