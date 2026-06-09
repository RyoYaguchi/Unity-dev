using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sunLight;
    
    [Range(0f, 24f)]
    public float timeOfDay = 8f; // Start in the morning at 08:00
    public float cycleLengthMinutes = 20f; // 20 real-world minutes for a full 24h day/night cycle
    public float cycleSpeed = 0.02f; // hours per second, auto-calculated from cycleLengthMinutes

    private List<Light> lanterns = new List<Light>();
    private List<Light> campfires = new List<Light>();

    // Lighting parameters for the 4 key phases of the day
    private struct LightingState
    {
        public Vector3 sunRotation;
        public Color sunColor;
        public float sunIntensity;
        public Color ambientColor;
        public Color fogColor;
        public bool lanternsActive;
    }

    private LightingState nightState = new LightingState
    {
        sunRotation = new Vector3(18f, -120f, 0f),
        sunColor = new Color(0.25f, 0.35f, 0.7f, 1f), // Moonlight deep blue
        sunIntensity = 0.15f,
        ambientColor = new Color(0.04f, 0.05f, 0.14f, 1f),
        fogColor = new Color(0.04f, 0.05f, 0.14f, 1f),
        lanternsActive = true
    };

    private LightingState morningState = new LightingState
    {
        sunRotation = new Vector3(12f, -150f, 0f),
        sunColor = new Color(1.0f, 0.65f, 0.45f, 1f), // Orange sunrise warm glow
        sunIntensity = 0.8f,
        ambientColor = new Color(0.18f, 0.16f, 0.22f, 1f),
        fogColor = new Color(0.18f, 0.16f, 0.22f, 1f),
        lanternsActive = false
    };

    private LightingState dayState = new LightingState
    {
        sunRotation = new Vector3(45f, -120f, 0f),
        sunColor = new Color(1.0f, 0.95f, 0.88f, 1f), // Warm white daylight
        sunIntensity = 1.4f,
        ambientColor = new Color(0.25f, 0.23f, 0.28f, 1f),
        fogColor = new Color(0.2f, 0.18f, 0.25f, 1f),
        lanternsActive = false
    };

    private LightingState eveningState = new LightingState
    {
        sunRotation = new Vector3(15f, -90f, 0f),
        sunColor = new Color(0.98f, 0.45f, 0.2f, 1f), // Deep red/orange sunset
        sunIntensity = 1.1f,
        ambientColor = new Color(0.22f, 0.15f, 0.18f, 1f),
        fogColor = new Color(0.22f, 0.15f, 0.18f, 1f),
        lanternsActive = true
    };

    void Start()
    {
        cycleSpeed = 24f / (cycleLengthMinutes * 60f);
        FindLanterns();
    }

    void Update()
    {
        // 1. Advance automatic cycle
        timeOfDay += Time.deltaTime * cycleSpeed;
        if (timeOfDay >= 24f) timeOfDay -= 24f;

        // 2. Handle manual quick skip with T key
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.tKey.wasPressedThisFrame)
        {
            FastForwardToNextPhase();
        }
#else
        if (Input.GetKeyDown(KeyCode.T))
        {
            FastForwardToNextPhase();
        }
#endif

        // 3. Interpolate environmental lighting based on current time
        UpdateLighting();

        // 4. Perlin flame flicker
        FlickerLanterns();
        FlickerCampfires();
    }

    void FindLanterns()
    {
        lanterns.Clear();
        campfires.Clear();
        Light[] allLights = FindObjectsByType<Light>(FindObjectsInactive.Include);
        foreach (Light l in allLights)
        {
            if (l.gameObject.name.StartsWith("LanternLight"))
            {
                lanterns.Add(l);
            }
            else if (l.gameObject.name.StartsWith("CampfireLight"))
            {
                campfires.Add(l);
                l.enabled = true;
            }
        }
    }

    void FastForwardToNextPhase()
    {
        // Cycle: Morning (6) -> Day (12) -> Evening (18) -> Night (0/24)
        if (timeOfDay >= 0f && timeOfDay < 6f)
        {
            timeOfDay = 6f; // Jump to Sunrise
        }
        else if (timeOfDay >= 6f && timeOfDay < 12f)
        {
            timeOfDay = 12f; // Jump to Noon
        }
        else if (timeOfDay >= 12f && timeOfDay < 18f)
        {
            timeOfDay = 18f; // Jump to Sunset
        }
        else
        {
            timeOfDay = 0f; // Jump to Midnight
        }
    }

    void UpdateLighting()
    {
        if (sunLight == null) return;

        LightingState from, to;
        float t = 0f;

        // Calculate interpolation parameters based on the 24h clock segments
        if (timeOfDay >= 0f && timeOfDay < 6f)
        {
            // Night (00:00) to Morning (06:00)
            from = nightState;
            to = morningState;
            t = timeOfDay / 6f;
        }
        else if (timeOfDay >= 6f && timeOfDay < 12f)
        {
            // Morning (06:00) to Day (12:00)
            from = morningState;
            to = dayState;
            t = (timeOfDay - 6f) / 6f;
        }
        else if (timeOfDay >= 12f && timeOfDay < 18f)
        {
            // Day (12:00) to Evening (18:00)
            from = dayState;
            to = eveningState;
            t = (timeOfDay - 12f) / 6f;
        }
        else
        {
            // Evening (18:00) to Night (24:00)
            from = eveningState;
            to = nightState;
            t = (timeOfDay - 18f) / 6f;
        }

        // Apply smooth step weighting
        t = Mathf.SmoothStep(0f, 1f, t);

        // Interpolate directional sun values
        sunLight.transform.rotation = Quaternion.Slerp(Quaternion.Euler(from.sunRotation), Quaternion.Euler(to.sunRotation), t);
        sunLight.color = Color.Lerp(from.sunColor, to.sunColor, t);
        sunLight.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, t);

        // Interpolate ambient environmental light and fog
        RenderSettings.ambientLight = Color.Lerp(from.ambientColor, to.ambientColor, t);
        RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, t);

        // Dynamic lantern trigger based on sunset / sunrise thresholds
        bool lanternsShouldBeActive = (timeOfDay >= 17.5f || timeOfDay <= 6.5f);
        foreach (Light l in lanterns)
        {
            if (l != null) l.enabled = lanternsShouldBeActive;
        }
    }

    void FlickerLanterns()
    {
        foreach (Light l in lanterns)
        {
            if (l != null && l.enabled)
            {
                float noise = Mathf.PerlinNoise(Time.time * 6.5f, l.transform.position.x * 17.5f);
                l.intensity = Mathf.Lerp(1.4f, 2.4f, noise);
            }
        }
    }

    void FlickerCampfires()
    {
        foreach (Light l in campfires)
        {
            if (l != null)
            {
                float noise = Mathf.PerlinNoise(Time.time * 7f, l.transform.position.x * 21.3f);
                l.intensity = Mathf.Lerp(1.8f, 2.8f, noise);
            }
        }
    }
}

