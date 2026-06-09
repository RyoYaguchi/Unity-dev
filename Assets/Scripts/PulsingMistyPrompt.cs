using UnityEngine;

public class PulsingMistyPrompt : MonoBehaviour
{
    public float floatSpeed = 2.2f;
    public float floatRange = 0.12f;
    public float pulseSpeed = 1.6f;
    public float pulseRange = 0.08f;

    private Vector3 startLocalPos;
    private Vector3 startScale;

    void Start()
    {
        startLocalPos = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        // Gently float up and down (slow sine wave)
        float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.localPosition = startLocalPos + new Vector3(0f, floatOffset, 0f);

        // Gently pulse scale (breathing/heartbeat effect)
        float pulseFactor = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseRange;
        transform.localScale = startScale * pulseFactor;
    }
}
