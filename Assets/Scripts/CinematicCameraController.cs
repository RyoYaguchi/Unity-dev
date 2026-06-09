using UnityEngine;

public class CinematicCameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10.5f, -12.5f); // Perfect JRPG tilt-shift camera offset (pulled back)
    public float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Set camera angle: tilted down 28 degrees on X, perfectly aligned with gameplay axis on Y
        transform.rotation = Quaternion.Euler(28f, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
