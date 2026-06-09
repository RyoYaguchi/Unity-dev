using UnityEngine;

public class WaterFlowAnimator : MonoBehaviour
{
    public float speedX = 0.03f; // Flow speed on X
    public float speedY = 0.08f; // Flow speed on Y
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend != null && rend.material != null)
        {
            float offsetX = Time.time * speedX;
            float offsetY = Time.time * speedY;
            // URP uses _BaseMap as the main texture reference
            rend.material.SetTextureOffset("_BaseMap", new Vector2(offsetX, offsetY));
        }
    }
}
