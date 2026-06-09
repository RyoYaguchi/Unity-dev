using UnityEngine;

public class HouseDoorTransition : MonoBehaviour
{
    public Vector3 targetPosition;
    public string promptText = "...";

    private bool inRange = false;
    private GameObject promptBubble;

    void Start()
    {
        CreatePromptBubble();
    }

    void CreatePromptBubble()
    {
        promptBubble = new GameObject("DoorPromptBubble");
        promptBubble.transform.SetParent(transform);
        promptBubble.transform.localPosition = new Vector3(0f, 1.3f, -0.15f);
        promptBubble.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        promptBubble.AddComponent<HD2DBillboard>();
        promptBubble.AddComponent<PulsingMistyPrompt>();

        // Translucent blue-purple JRPG halo sphere
        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        backing.name = "Backing";
        backing.transform.SetParent(promptBubble.transform);
        backing.transform.localPosition = Vector3.zero;
        backing.transform.localScale = new Vector3(12f, 8f, 2f);
        Destroy(backing.GetComponent<Collider>());

        Renderer rend = backing.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        // Soft glowing JRPG cyan-blue mist
        rend.material.color = new Color(0.18f, 0.55f, 0.95f, 0.68f);

        // Three dots text mesh inside
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(promptBubble.transform);
        txtObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        txtObj.transform.localScale = Vector3.one;

        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = "...";
        tm.fontSize = 24;
        tm.color = Color.white;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;

        promptBubble.SetActive(false); // active when player is standing in front of the door
    }

    void Update()
    {
        if (inRange)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                TeleportPlayer();
            }
#else
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                TeleportPlayer();
            }
#endif
        }
    }

    void TeleportPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = targetPosition; // Force physics engine position update!
            }
            player.transform.position = targetPosition;
            Physics.SyncTransforms(); // Sync physics system with the new transform position instantly!
            Debug.Log($"HD-2D: Teleported player to {targetPosition}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (promptBubble != null) promptBubble.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (promptBubble != null) promptBubble.SetActive(false);
        }
    }
}
