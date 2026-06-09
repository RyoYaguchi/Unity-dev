using UnityEngine;

[ExecuteInEditMode]
public class HD2DBillboard : MonoBehaviour
{
    private Camera mainCam;

    [Header("Wind Sway Settings")]
    public bool enableSway = false;
    public float swaySpeed = 1.8f;
    public float swayAmount = 2.5f;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null)
        {
            // Cylindrical billboarding: face camera but keep upright on Y-axis
            Vector3 targetDir = mainCam.transform.position - transform.position;
            targetDir.y = 0; // Lock Y rotation to keep sprites vertical
            if (targetDir != Vector3.zero)
            {
                // Align with camera orientation
                transform.rotation = Quaternion.LookRotation(-targetDir);
            }

            if (enableSway)
            {
                // Coordinate-based seed phase offset so assets don't sway in perfect unison
                float phaseOffset = transform.position.x * 0.4f + transform.position.z * 0.3f;
                float angle = Mathf.Sin(Time.time * swaySpeed + phaseOffset) * swayAmount;
                transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}
