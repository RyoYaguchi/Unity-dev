using System.Collections.Generic;
using UnityEngine;

// 簡易インベントリ／ステータス UI（IMGUI オーバーレイ、BattleSystem と同方式）。
// I キーで開閉。Lv/EXP/HP/攻撃/防御を表示し、武器・防具を手動装備、やくそうを使用できる。
// GameManager と同様に自己ブートストラップで全シーンに常駐。戦闘中は開けない。
public class InventoryUI : MonoBehaviour
{
    // Play中の再コンパイルでstaticがnull化した場合に備え、null時は実体を再取得。
    static InventoryUI _instance;
    public static InventoryUI Instance
    {
        get { if (_instance == null) _instance = FindFirstObjectByType<InventoryUI>(); return _instance; }
        private set { _instance = value; }
    }

    private bool open = false;
    private MonoBehaviour frozenController; // メニュー中はプレイヤー操作を停止

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("InventoryUI");
            go.AddComponent<InventoryUI>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 戦闘中は開かない（戦闘は独自のアイテムコマンドを持つ）。
        var bs = BattleSystem.Instance;
        if (bs != null && bs.battleActive) { if (open) Close(); return; }

#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        bool toggle = kb.iKey.wasPressedThisFrame;
        bool close = kb.escapeKey.wasPressedThisFrame;
#else
        bool toggle = Input.GetKeyDown(KeyCode.I);
        bool close = Input.GetKeyDown(KeyCode.Escape);
#endif
        // 会話中は開かない（吹き出しと重なるのを防ぐ）。閉じる操作は常に許可。
        if (toggle) { if (open) Close(); else if (!AnyDialogueActive()) Open(); }
        else if (close && open) Close();
    }

    // 村長/NPCの会話が進行中か（Iキー押下時のみ走査）。
    bool AnyDialogueActive()
    {
        foreach (var d in FindObjectsByType<QuestGiver>(FindObjectsSortMode.None)) if (d.isTalking) return true;
        foreach (var d in FindObjectsByType<NPCChoiceDialogue>(FindObjectsSortMode.None)) if (d.isTalking) return true;
        foreach (var d in FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None)) if (d.isTalking) return true;
        return false;
    }

    void Open()
    {
        open = true;
        var p = GameObject.Find("Player");
        if (p != null)
        {
            var c = p.GetComponent("HD2DPlayerController") as MonoBehaviour;
            if (c != null && c.enabled) { frozenController = c; c.enabled = false; }
            var rb = p.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    void Close()
    {
        open = false;
        if (frozenController != null) { frozenController.enabled = true; frozenController = null; }
    }

    // ---- IMGUI ----
    private GUIStyle boxStyle, labelStyle, titleStyle, btnStyle, hintStyle;
    void EnsureStyles()
    {
        if (boxStyle != null) return;
        int fs = Mathf.Max(13, Mathf.RoundToInt(Screen.height * 0.018f));
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = Solid(new Color(0.06f, 0.06f, 0.10f, 0.96f));
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fs, richText = true, wordWrap = false };
        labelStyle.normal.textColor = Color.white;
        titleStyle = new GUIStyle(labelStyle) { fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(fs * 1.25f) };
        btnStyle = new GUIStyle(GUI.skin.button) { fontSize = fs, fontStyle = FontStyle.Bold };
        hintStyle = new GUIStyle(labelStyle) { fontSize = Mathf.RoundToInt(fs * 0.85f) };
        hintStyle.normal.textColor = new Color(0.75f, 0.75f, 0.8f);
    }

    private Texture2D _solid; private Color _sc;
    Texture2D Solid(Color c)
    {
        if (_solid == null || _sc != c) { _solid = new Texture2D(1, 1); _solid.SetPixel(0, 0, c); _solid.Apply(); _sc = c; }
        return _solid;
    }

    void OnGUI()
    {
        if (!open) return;
        var gm = GameManager.Instance;
        if (gm == null) return;
        EnsureStyles();

        float W = Screen.width, H = Screen.height;
        Rect panel = new Rect(W * 0.5f - W * 0.27f, H * 0.10f, W * 0.54f, H * 0.80f);
        GUI.Box(panel, GUIContent.none, boxStyle);

        float pad = panel.width * 0.04f;
        float x = panel.x + pad, y = panel.y + pad, w = panel.width - pad * 2f;
        float line = Mathf.Max(22f, H * 0.030f);

        GUI.Label(new Rect(x, y, w, line * 1.2f), "もちもの／ステータス", titleStyle);
        y += line * 1.4f;

        // --- ステータス ---
        int need = gm.level * 2;
        GUI.Label(new Rect(x, y, w, line), $"Lv {gm.level}    EXP {gm.exp}/{need}    HP {gm.curHealth}/{gm.maxHealth}", labelStyle);
        y += line;
        string wpn = gm.equippedWeapon != null ? gm.equippedWeapon.name : "なし";
        string arm = gm.equippedArmor != null ? gm.equippedArmor.name : "なし";
        GUI.Label(new Rect(x, y, w, line), $"こうげき {gm.TotalAtk}（武器:{wpn}）    ぼうぎょ {gm.TotalDef}（防具:{arm}）", labelStyle);
        y += line * 1.4f;

        GUI.Label(new Rect(x, y, w, line), "── どうぐ ──", hintStyle);
        y += line * 1.1f;

        // --- アイテム一覧（行ごとに装備/使用ボタン）---
        Item toEquip = null, toUse = null;
        if (gm.inventory.Count == 0)
        {
            GUI.Label(new Rect(x, y, w, line), "（なにも持っていない）", labelStyle);
            y += line;
        }
        else
        {
            float btnW = w * 0.22f;
            // 同一インスタンスを安定参照するためインデックスで回す
            for (int i = 0; i < gm.inventory.Count; i++)
            {
                Item it = gm.inventory[i];
                bool equipped = (it == gm.equippedWeapon || it == gm.equippedArmor);
                string mark = equipped ? "  <color=#7fd>[E]</color>" : "";
                GUI.Label(new Rect(x, y + 2, w - btnW - 8, line), it.Label() + mark, labelStyle);

                if (it.type == ItemType.Herb)
                {
                    GUI.enabled = gm.curHealth < gm.maxHealth;
                    if (GUI.Button(new Rect(x + w - btnW, y, btnW, line), "つかう", btnStyle)) toUse = it;
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = !equipped;
                    if (GUI.Button(new Rect(x + w - btnW, y, btnW, line), "そうび", btnStyle)) toEquip = it;
                    GUI.enabled = true;
                }
                y += line * 1.15f;
            }
        }

        // --- 閉じる / ヒント ---
        float closeW = w * 0.22f;
        GUI.Label(new Rect(x, panel.yMax - pad - line * 1.6f, w, line), "[I] とじる   [Esc] とじる", hintStyle);
        if (GUI.Button(new Rect(x + w - closeW, panel.yMax - pad - line, closeW, line), "とじる", btnStyle)) Close();

        // ループ後に状態変更（OnGUI中のリスト変更を避ける）
        if (toEquip != null) gm.Equip(toEquip);
        if (toUse != null) gm.UseHerb(toUse);
    }
}
