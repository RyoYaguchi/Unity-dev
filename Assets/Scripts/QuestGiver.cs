using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 村長のクエスト進行役。GameManager.QuestState で会話が分岐し、受注で洞窟へ自動移動する。
public class QuestGiver : MonoBehaviour
{
    public Renderer promptRenderer;
    public int maxCharsPerLine = 15;
    public bool isTalking = false;

    private bool inRange = false;
    private enum Step { Idle, Lines, Choice }
    private Step step = Step.Idle;

    private readonly List<string> queue = new List<string>();
    private int idx = 0;
    private bool hasChoice = false;
    private string choicePrompt = "";
    private System.Action onYes, onNo, onFinishNoChoice;

    private GameObject bubble;
    private TextMesh bubbleText;
    private Coroutine typeRoutine;
    private bool isTyping = false;

    void Start() { if (promptRenderer != null) promptRenderer.gameObject.SetActive(false); }

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
        switch (step)
        {
            case Step.Idle: if (e) Begin(); break;
            case Step.Lines: if (e && !isTyping) Advance(); break;
            case Step.Choice:
                if (y) { var a = onYes; EndAll(); if (a != null) a(); }
                else if (n) { var a = onNo; if (a != null) a(); else EndAll(); }
                break;
        }
    }

    void Begin()
    {
        var gm = GameManager.Instance;
        var st = gm != null ? gm.quest : GameManager.QuestState.NotOffered;
        queue.Clear(); idx = 0; hasChoice = false; onYes = null; onNo = null; onFinishNoChoice = null;
        isTalking = true; step = Step.Lines;
        if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
        CreateBubble();

        if (st == GameManager.QuestState.NotOffered)
        {
            queue.Add("村長: おお 旅人よ、よく来てくれた。");
            queue.Add("村長: ウリセンども が 村の野菜を 奪っていくのだ。");
            queue.Add("村長: 村はずれの洞窟に 巣食う奴らを 退治してくれぬか?");
            hasChoice = true;
            choicePrompt = "依頼を 受ける?\n[Y]はい   [N]いいえ";
            onYes = AcceptQuest;
            onNo = () => SimpleLine("村長: 気が変わったら 声をかけてくれ。");
        }
        else if (st == GameManager.QuestState.Accepted)
        {
            queue.Add("村長: ウリセンの討伐、頼んだぞ。");
            queue.Add("村長: 洞窟へ 向かうかね?");
            hasChoice = true;
            choicePrompt = "洞窟へ 行く?\n[Y]はい   [N]いいえ";
            onYes = GoToCave;
            onNo = () => SimpleLine("村長: 準備が できたら また来てくれ。");
        }
        else if (st == GameManager.QuestState.BossDefeated)
        {
            queue.Add("村長: おお、ボスの タクヤ を 倒したのか！");
            queue.Add("村長: これで 村の野菜は 安泰だ。本当に ありがとう！");
            queue.Add("村長: わしらは 君を 忘れない、英雄よ。");
            onFinishNoChoice = () => { if (GameManager.Instance != null) GameManager.Instance.quest = GameManager.QuestState.Completed; };
        }
        else
        {
            queue.Add("村長: 村を 救ってくれて ありがとう、英雄よ。");
        }
        ShowLine(queue[0]);
    }

    void Advance()
    {
        idx++;
        if (idx < queue.Count) { ShowLine(queue[idx]); return; }
        if (hasChoice) { step = Step.Choice; ShowLine(choicePrompt); }
        else { var f = onFinishNoChoice; EndAll(); if (f != null) f(); }
    }

    // 単発の返答行を表示（選択肢「いいえ」用）。送ると会話終了。
    void SimpleLine(string s)
    {
        queue.Clear(); queue.Add(s); idx = 0; hasChoice = false; onFinishNoChoice = null;
        step = Step.Lines; ShowLine(s);
    }

    // --- クエストアクション（テスト用に public） ---
    public void AcceptQuest()
    {
        var gm = GameManager.Instance; if (gm == null) return;
        gm.quest = GameManager.QuestState.Accepted;
        gm.WarpToCave();
    }

    public void GoToCave()
    {
        var gm = GameManager.Instance; if (gm == null) return;
        gm.WarpToCave();
    }

    void EndAll()
    {
        isTalking = false; step = Step.Idle; idx = 0; hasChoice = false;
        if (typeRoutine != null) { StopCoroutine(typeRoutine); typeRoutine = null; }
        isTyping = false;
        if (bubble != null) Destroy(bubble);
        bubbleText = null;
        if (inRange && promptRenderer != null) promptRenderer.gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (step == Step.Idle && promptRenderer != null) promptRenderer.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (promptRenderer != null) promptRenderer.gameObject.SetActive(false);
            if (step != Step.Idle) EndAll();
        }
    }

    // --- 吹き出し（NPCChoiceDialogue と同方式） ---
    void CreateBubble()
    {
        bubble = new GameObject("QuestBubble");
        bubble.transform.position = transform.position + Vector3.up * 2.2f;
        bubble.AddComponent<HD2DBillboard>();

        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(backing.GetComponent<Collider>());
        backing.transform.SetParent(bubble.transform);
        backing.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        var rend = backing.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        rend.material.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);
        backing.transform.localScale = new Vector3(4.8f, 2.6f, 1f);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(bubble.transform);
        txt.transform.localPosition = Vector3.zero;
        txt.transform.localScale = new Vector3(0.055f, 0.055f, 0.055f);
        bubbleText = txt.AddComponent<TextMesh>();
        bubbleText.fontSize = 24; bubbleText.color = Color.white;
        bubbleText.alignment = TextAlignment.Center; bubbleText.anchor = TextAnchor.MiddleCenter;
    }

    void ShowLine(string line)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(Typewriter(line));
    }

    IEnumerator Typewriter(string line)
    {
        isTyping = true;
        if (bubbleText == null) { isTyping = false; yield break; }
        bubbleText.text = "";
        string f = Wrap(line, maxCharsPerLine);
        for (int i = 0; i <= f.Length; i++)
        {
            if (bubbleText == null) { isTyping = false; yield break; }   // 吹き出し破棄後は中断
            bubbleText.text = f.Substring(0, i);
            yield return new WaitForSeconds(0.022f);
        }
        isTyping = false;
    }

    string Wrap(string text, int max)
    {
        if (max < 1) max = 1;
        var sb = new System.Text.StringBuilder();
        int c = 0;
        foreach (char ch in text)
        {
            if (ch == '\n') { sb.Append('\n'); c = 0; continue; }
            sb.Append(ch); c++;
            if (c >= max) { sb.Append('\n'); c = 0; }
        }
        return sb.ToString();
    }
}
