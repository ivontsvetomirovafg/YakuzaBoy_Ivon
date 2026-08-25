using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] 
    private Transform player;
    [SerializeField] 
    private Vector3 camOffset;

    [Header("Limites")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void Update()
    {
        float x = Mathf.Clamp(player.position.x, minX, maxX);
        float y = Mathf.Clamp(player.position.y, minY, maxY);

        transform.position = new Vector3(x, y + camOffset.y, camOffset.z);
    }

    public void SetLimits(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
    }

    public void MoveInstantly(Vector3 newPosition)
    {
        transform.position = new Vector3(newPosition.x, newPosition.y, camOffset.z);
    }
}
