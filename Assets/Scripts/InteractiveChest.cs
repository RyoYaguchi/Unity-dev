using System.Collections;
using UnityEngine;

public class InteractiveChest : MonoBehaviour
{
    public Sprite[] chestSprites; // Array of 3 sprites: [0] Closed, [1] Half Open, [2] Open
    public Renderer promptRenderer; // Renderer for the popup E indicator

    private SpriteRenderer sr;
    private bool inRange = false;
    private bool isOpen = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (chestSprites != null && chestSprites.Length > 0 && sr != null)
        {
            sr.sprite = chestSprites[0]; // Set closed initially
        }
        if (promptRenderer != null)
        {
            promptRenderer.gameObject.SetActive(false); // Hide prompt initially
        }
    }

    void Update()
    {
        if (inRange && !isOpen)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                StartCoroutine(OpenChestRoutine());
            }
#else
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(OpenChestRoutine());
            }
#endif
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            inRange = true;
            if (promptRenderer != null) promptRenderer.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
        }
    }

    IEnumerator OpenChestRoutine()
    {
        isOpen = true;
        inRange = false;
        if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);

        // Frame 1: Opening (Half Open)
        if (chestSprites != null && chestSprites.Length > 1)
        {
            sr.sprite = chestSprites[1];
            yield return new WaitForSeconds(0.12f);
        }

        // Frame 2: Open
        if (chestSprites != null && chestSprites.Length > 2)
        {
            sr.sprite = chestSprites[2];
        }

        // Spawn beautiful retro text that floats up and fades
        SpawnFloatingText("Obtained: Hero's Sword!");
    }

    void SpawnFloatingText(string text)
    {
        GameObject textObj = new GameObject("FloatingText");
        textObj.transform.position = transform.position + Vector3.up * 1.5f;

        // Keep it facing the camera upright
        textObj.AddComponent<HD2DBillboard>();

        // TextMesh setup
        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 24;
        tm.color = new Color(1f, 0.85f, 0.2f, 1f); // Gorgeous retro gold
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        
        // Scale down to prevent blurry pixel stretch
        textObj.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        StartCoroutine(MoveAndFadeText(textObj, tm));
    }

    IEnumerator MoveAndFadeText(GameObject go, TextMesh tm)
    {
        float duration = 1.6f;
        float elapsed = 0f;
        Vector3 startPos = go.transform.position;

        while (elapsed < duration)
        {
            if (go == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smoothly ease up
            go.transform.position = startPos + Vector3.up * t * 1.3f;

            // Linear alpha fade out
            Color c = tm.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            tm.color = c;

            yield return null;
        }

        Destroy(go);
    }
}
