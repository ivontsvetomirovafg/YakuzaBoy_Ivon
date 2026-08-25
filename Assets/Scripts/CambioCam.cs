using UnityEngine;

public class CambioCam : MonoBehaviour
{
    [Header("Limites Cam")]
    [SerializeField] 
    private float MinX;
    [SerializeField] 
    private float MaxX;
    [SerializeField] 
    private float MinY;
    [SerializeField] 
    private float MaxY;

    private CamController camController;

    private void Start()
    {
        camController = Camera.main.GetComponent<CamController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            camController.SetLimits(MinX, MaxX, MinY, MaxY);
        }
    }
}
