using System.Collections;
using UnityEngine;

// Branching dialogue: shows intro lines (advance with E), then a Yes/No choice
// (Y / N), then the matching result line. CJK-friendly word wrapping.
public class NPCChoiceDialogue : MonoBehaviour
{
    public Renderer promptRenderer;

    [TextArea]
    public string[] introLines = new string[] { "..." };
    [TextArea]
    public string choicePrompt = "[Y] はい   /   [N] いいえ";
    public string yesLine = "謝謝茄子!";
    public string noLine = "困ったなあ";

    [Header("Battle hook (optional)")]
    public bool startBattleOnYes = false;
    public string enemyName = "暴れ馬";
    public int enemyHealth = 3;
    public int enemyAtk = 1;
    public int enemyDef = 0;
    public int enemyExp = 1;
    [TextArea] public string winMessage = "勝利！";

    [Tooltip("Max full-width characters per bubble line before wrapping.")]
    public int maxCharsPerLine = 15;

    public bool isTalking = false;

    private enum State { Idle, Intro, Choice, Result }
    private State state = State.Idle;
    private int introIndex = 0;
    private bool inRange = false;
    private bool isTyping = false;

    private GameObject activeBubble;
    private TextMesh bubbleText;
    private Coroutine typeRoutine;

    void Start()
    {
        if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!inRange) return;
        if (BattleSystem.Instance != null && BattleSystem.Instance.battleActive) return;

#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        bool e = kb.eKey.wasPressedThisFrame;
        bool y = kb.yKey.wasPressedThisFrame;
        bool n = kb.nKey.wasPressedThisFrame;
#else
        bool e = Input.GetKeyDown(KeyCode.E);
        bool y = Input.GetKeyDown(KeyCode.Y);
        bool n = Input.GetKeyDown(KeyCode.N);
#endif

        switch (state)
        {
            case State.Idle:
                if (e) StartDialogue();
                break;
            case State.Intro:
                if (e && !isTyping) AdvanceIntro();
                break;
            case State.Choice:
                if (y)
                {
                    if (startBattleOnYes && BattleSystem.Instance != null)
                    {
                        isTalking = false; state = State.Idle; introIndex = 0;
                        if (activeBubble != null) Destroy(activeBubble);
                        BattleSystem.Instance.StartBattle(enemyName, enemyHealth, enemyAtk, enemyDef, enemyExp, winMessage);
                    }
                    else { state = State.Result; ShowLine(yesLine); }
                }
                else if (n) { state = State.Result; ShowLine(noLine); }
                break;
            case State.Result:
                if (e && !isTyping) EndDialogue();
                break;
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        state = State.Intro;
        introIndex = 0;
        if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
        CreateSpeechBubble();
        ShowLine(introLines.Length > 0 ? introLines[0] : "...");
    }

    void AdvanceIntro()
    {
        introIndex++;
        if (introIndex < introLines.Length)
        {
            ShowLine(introLines[introIndex]);
        }
        else
        {
            state = State.Choice;
            ShowLine(choicePrompt);
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        state = State.Idle;
        introIndex = 0;
        if (activeBubble != null) Destroy(activeBubble);
        if (inRange && promptRenderer != null) promptRenderer.gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (state == State.Idle && promptRenderer != null) promptRenderer.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
            if (state != State.Idle)
            {
                isTalking = false;
                state = State.Idle;
                introIndex = 0;
                if (activeBubble != null) Destroy(activeBubble);
            }
        }
    }

    void CreateSpeechBubble()
    {
        activeBubble = new GameObject("SpeechBubble");
        activeBubble.transform.position = transform.position + Vector3.up * 2.2f;
        activeBubble.AddComponent<HD2DBillboard>();

        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(backing.GetComponent<Collider>());
        backing.name = "Backing";
        backing.transform.SetParent(activeBubble.transform);
        backing.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        Renderer rend = backing.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        rend.material.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);
        backing.transform.localScale = new Vector3(4.8f, 2.6f, 1f);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(activeBubble.transform);
        txtObj.transform.localPosition = Vector3.zero;
        txtObj.transform.localScale = new Vector3(0.055f, 0.055f, 0.055f);

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
        isTyping = true;
        bubbleText.text = "";
        string formatted = WrapText(line, maxCharsPerLine);
        for (int i = 0; i <= formatted.Length; i++)
        {
            bubbleText.text = formatted.Substring(0, i);
            yield return new WaitForSeconds(0.022f);
        }
        isTyping = false;
    }

    // Character-count wrapping that also respects explicit '\n'. Works for CJK
    // text (which has no spaces) as well as spaced text.
    string WrapText(string text, int maxChars)
    {
        if (maxChars < 1) maxChars = 1;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int count = 0;
        foreach (char c in text)
        {
            if (c == '\n')
            {
                sb.Append('\n');
                count = 0;
                continue;
            }
            sb.Append(c);
            count++;
            if (count >= maxChars)
            {
                sb.Append('\n');
                count = 0;
            }
        }
        return sb.ToString();
    }
}
