using System.Collections;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public Renderer promptRenderer;
    public string[] dialogueLines = new string[] {
        "NPC: Welcome to HD-2D Valley, traveler!",
        "NPC: This landscape is built from 3D cubes textured with pixel art.",
        "NPC: Press [T] on your keyboard to toggle Day and Night!",
        "NPC: Notice how the bridge lanterns cast warm 3D shadows at night..."
    };

    private bool inRange = false;
    public bool isTalking = false;
    private int currentLineIndex = 0;

    private GameObject activeBubble;
    private TextMesh bubbleText;
    private GameObject bubbleBacking;
    private Coroutine typeRoutine;

    void Start()
    {
        if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (inRange)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                if (!isTalking)
                {
                    StartDialogue();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isTalking)
                {
                    StartDialogue();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
#endif
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (!isTalking && promptRenderer != null) promptRenderer.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
            EndDialogue();
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        currentLineIndex = 0;
        if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
        
        CreateSpeechBubble();
        ShowLine(dialogueLines[currentLineIndex]);
    }

    void AdvanceDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            ShowLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        if (activeBubble != null)
        {
            Destroy(activeBubble);
        }
        if (inRange && promptRenderer != null)
        {
            promptRenderer.gameObject.SetActive(true);
        }
    }

    void CreateSpeechBubble()
    {
        activeBubble = new GameObject("SpeechBubble");
        activeBubble.transform.position = transform.position + Vector3.up * 2.2f;
        
        // Align upright with camera yaw
        activeBubble.AddComponent<HD2DBillboard>();

        // 1. Translucent dark backing panel
        bubbleBacking = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(bubbleBacking.GetComponent<Collider>()); // No physical collider
        bubbleBacking.name = "Backing";
        bubbleBacking.transform.SetParent(activeBubble.transform);
        bubbleBacking.transform.localPosition = new Vector3(0f, 0f, 0.05f); // Position slightly behind text Y-depth
        
        Renderer rend = bubbleBacking.GetComponent<Renderer>();
        // Universal Render Pipeline Unlit shader is safe and highly compatible
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        rend.material.color = new Color(0.08f, 0.08f, 0.1f, 0.85f); // Translucent JRPG blue-gray
        
        // Dimensions of bubble box
        bubbleBacking.transform.localScale = new Vector3(3.8f, 1.4f, 1f);

        // 2. Text Mesh text holder
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(activeBubble.transform);
        txtObj.transform.localPosition = Vector3.zero;
        txtObj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);

        bubbleText = txtObj.AddComponent<TextMesh>();
        bubbleText.fontSize = 24;
        bubbleText.color = Color.white;
        bubbleText.alignment = TextAlignment.Center;
        bubbleText.anchor = TextAnchor.MiddleCenter;
    }

    void ShowLine(string line)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypewriterRoutine(line));
    }

    IEnumerator TypewriterRoutine(string line)
    {
        bubbleText.text = "";
        
        // Manual wrapping for retro dialogue size
        string formattedLine = InsertLineBreaks(line, 24);

        for (int i = 0; i <= formattedLine.Length; i++)
        {
            bubbleText.text = formattedLine.Substring(0, i);
            yield return new WaitForSeconds(0.025f); // Typist tick speed
        }
    }

    string InsertLineBreaks(string text, int maxCharsPerLine)
    {
        string[] words = text.Split(' ');
        string result = "";
        string currentLine = "";

        foreach (string word in words)
        {
            if ((currentLine + word).Length > maxCharsPerLine)
            {
                result += currentLine.Trim() + "\n";
                currentLine = "";
            }
            currentLine += word + " ";
        }
        result += currentLine.Trim();
        return result;
    }
}
