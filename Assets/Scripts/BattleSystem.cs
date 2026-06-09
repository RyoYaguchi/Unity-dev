using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Minimal front-view turn-based battle on a black background, rendered with IMGUI
// (no Canvas needed). Each combatant has a "ヘルス" stat; at 0 it is 戦闘不能 (KO).
// The battle ends when one whole side is KO'd. Commands: たたかう / にげる.
public class BattleSystem : MonoBehaviour
{
    // Play中の再コンパイルでstaticがnull化した場合に備え、null時はシーン内の実体を再取得。
    static BattleSystem _instance;
    public static BattleSystem Instance
    {
        get { if (_instance == null) _instance = FindFirstObjectByType<BattleSystem>(); return _instance; }
        private set { _instance = value; }
    }

    [Header("Player side")]
    public string playerName = "蓮";
    public int playerMaxHealth = 5;
    public int playerAtk = 1;
    public int playerDef = 0;

    [Header("Visuals")]
    public Texture2D enemyTexture;   // shown on the black battle screen

    public bool battleActive = false;

    // One combatant. Health 0 = 戦闘不能.
    class Unit
    {
        public string name;
        public int max;
        public int health;
        public int atk;
        public int def;
        public bool Down { get { return health <= 0; } }
    }

    // damage = atk - def. If def exceeds atk by n (>=1), miss chance = 1 - 1/2^n.
    // On a connecting hit damage is at least 1.
    bool Resolve(Unit a, Unit d, out int dmg)
    {
        int raw = a.atk - d.def;
        if (raw >= 1) { dmg = raw; return true; }
        int excess = d.def - a.atk; // >= 0
        if (excess >= 1)
        {
            float missChance = 1f - 1f / Mathf.Pow(2f, excess);
            if (Random.value < missChance) { dmg = 0; return false; }
        }
        dmg = 1; return true;
    }

    private readonly List<Unit> allies = new List<Unit>();
    private readonly List<Unit> enemies = new List<Unit>();
    private Unit Player { get { return allies.Count > 0 ? allies[0] : null; } }
    private Unit Enemy { get { return enemies.Count > 0 ? enemies[0] : null; } }

    private enum Phase { Command, Animating, Win, Lose, Fled }
    private Phase phase = Phase.Command;
    private bool busy = false;
    private string winMessage = "勝利！";
    private int expReward = 0;
    private System.Action onWin, onLose;
    private readonly List<string> log = new List<string>();
    private MonoBehaviour playerController;

    void Awake() { Instance = this; }

    // プレイヤーの戦闘中ヘルスを GameManager に書き戻す（持ち越し）。
    void SyncHealthToGM()
    {
        var gm = GameManager.Instance;
        if (gm != null && Player != null) gm.curHealth = Mathf.Clamp(Player.health, 0, gm.maxHealth);
    }

    // プレイヤー数値は GameManager から取得（装備込み TotalAtk/TotalDef・curHealth）。
    // 勝利でexpGain付与、onWin/onLose は接触戦闘やシーン遷移の配線に使う。
    public void StartBattle(string enemyName, int enemyHealth, int eAtk, int eDef, int expGain, string winMsg,
                            System.Action onWinCb = null, System.Action onLoseCb = null)
    {
        var gm = GameManager.Instance;
        int pMax = gm != null ? gm.maxHealth : playerMaxHealth;
        int pCur = gm != null ? gm.curHealth : playerMaxHealth;
        int pAtk = gm != null ? gm.TotalAtk : playerAtk;
        int pDef = gm != null ? gm.TotalDef : playerDef;

        allies.Clear(); enemies.Clear();
        allies.Add(new Unit { name = playerName, max = pMax, health = pCur, atk = pAtk, def = pDef });
        int eh = Mathf.Max(1, enemyHealth);
        enemies.Add(new Unit { name = enemyName, max = eh, health = eh, atk = eAtk, def = eDef });

        expReward = expGain;
        onWin = onWinCb; onLose = onLoseCb;
        winMessage = string.IsNullOrEmpty(winMsg) ? "勝利！" : winMsg;
        battleActive = true; phase = Phase.Command; busy = false;
        log.Clear();
        AddLog(enemyName + " が しょうぶを しかけてきた！");
        FreezePlayer();
    }

    void FreezePlayer()
    {
        var p = GameObject.Find("Player");
        if (p != null)
        {
            var rb = p.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
            var c = p.GetComponent("HD2DPlayerController") as MonoBehaviour;
            if (c != null) { playerController = c; c.enabled = false; }
        }
    }

    void AddLog(string s) { log.Add(s); if (log.Count > 4) log.RemoveAt(0); }

    bool SideDown(List<Unit> side)
    {
        foreach (var u in side) if (!u.Down) return false;
        return true;
    }

    // ---- Commands ----
    void DoAttack() { if (!busy) StartCoroutine(AttackRoutine()); }
    IEnumerator AttackRoutine()
    {
        busy = true; phase = Phase.Animating;
        int dmg; bool hit = Resolve(Player, Enemy, out dmg);
        if (hit) { Enemy.health = Mathf.Max(0, Enemy.health - dmg); AddLog(Player.name + "の こうげき！ " + Enemy.name + "の ヘルスに " + dmg + "！"); }
        else AddLog(Player.name + "の こうげき！ しかし ミス！");
        yield return new WaitForSeconds(0.8f);
        if (Enemy.Down) AddLog(Enemy.name + "は 戦闘不能になった！");
        if (SideDown(enemies))
        {
            phase = Phase.Win; busy = true;
            SyncHealthToGM();                               // 戦闘後HPを先に持ち越し
            if (GameManager.Instance != null && expReward > 0)
            {
                GameManager.Instance.AddExp(expReward);     // Lvアップで最大HP+2/現在HP+2
                AddLog("経験値を " + expReward + " かくとく！");
            }
            AddLog(winMessage);
            yield break;
        }
        yield return EnemyTurn();
        if (phase != Phase.Lose) { busy = false; phase = Phase.Command; }
    }

    void DoFlee() { if (!busy) StartCoroutine(FleeRoutine()); }
    IEnumerator FleeRoutine()
    {
        busy = true; phase = Phase.Animating;
        if (Random.value > 0.5f)
        {
            AddLog("うまく にげだした！");
            yield return new WaitForSeconds(0.8f);
            phase = Phase.Fled;
        }
        else
        {
            AddLog("にげられなかった！");
            yield return new WaitForSeconds(0.6f);
            yield return EnemyTurn();
            if (phase != Phase.Lose) { busy = false; phase = Phase.Command; }
        }
    }

    // やくそうを1つ使って回復（1ターン消費）。所持品はGMから消費。戦闘中はUnitのHPを直接回復。
    void DoItem()
    {
        if (busy) return;
        var gm = GameManager.Instance;
        Item herb = null;
        if (gm != null) foreach (var it in gm.inventory) if (it.type == ItemType.Herb) { herb = it; break; }
        if (herb == null) { AddLog("やくそうが ない！"); return; }   // ターン消費なし
        StartCoroutine(ItemRoutine(herb));
    }

    IEnumerator ItemRoutine(Item herb)
    {
        busy = true; phase = Phase.Animating;
        GameManager.Instance.inventory.Remove(herb);
        int before = Player.health;
        Player.health = Mathf.Min(Player.max, Player.health + herb.power);
        AddLog(Player.name + "は " + herb.name + " を つかった！ ヘルス +" + (Player.health - before) + "！");
        yield return new WaitForSeconds(0.8f);
        yield return EnemyTurn();
        if (phase != Phase.Lose) { busy = false; phase = Phase.Command; }
    }

    IEnumerator EnemyTurn()
    {
        phase = Phase.Animating;
        if (Enemy.Down) yield break;
        int dmg; bool hit = Resolve(Enemy, Player, out dmg);
        if (hit) { Player.health = Mathf.Max(0, Player.health - dmg); AddLog(Enemy.name + "の こうげき！ " + Player.name + "の ヘルスに " + dmg + "！"); }
        else AddLog(Enemy.name + "の こうげき！ しかし ミス！");
        yield return new WaitForSeconds(0.8f);
        if (Player.Down) AddLog(Player.name + "は 戦闘不能になった……");
        if (SideDown(allies)) { phase = Phase.Lose; busy = true; AddLog("ぜんめつ……"); }
    }

    public void EndBattle()
    {
        if (phase == Phase.Fled) SyncHealthToGM();   // 逃走時も被弾分を持ち越し
        battleActive = false;
        if (playerController != null) playerController.enabled = true;

        if (phase == Phase.Win)
        {
            if (onWin != null) onWin();
        }
        else if (phase == Phase.Lose)
        {
            // 敗北の遷移はコールバックで配線（T6）。未配線なら全回復だけして詰み防止。
            if (onLose != null) onLose();
            else if (GameManager.Instance != null) GameManager.Instance.FullHeal();
        }
        onWin = null; onLose = null;
    }

    // ---- IMGUI overlay ----
    private GUIStyle boxStyle, labelStyle, btnStyle, titleStyle;
    void EnsureStyles()
    {
        if (boxStyle != null) return;
        int fs = Mathf.Max(14, Mathf.RoundToInt(Screen.height * 0.020f));
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = Solid(new Color(0.05f, 0.05f, 0.08f, 0.95f));
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fs, richText = true, wordWrap = true };
        labelStyle.normal.textColor = Color.white;
        titleStyle = new GUIStyle(labelStyle) { fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(fs * 1.15f) };
        btnStyle = new GUIStyle(GUI.skin.button) { fontSize = fs, fontStyle = FontStyle.Bold };
    }

    private Texture2D _solid; private Color _sc;
    Texture2D Solid(Color c)
    {
        if (_solid == null || _sc != c) { _solid = new Texture2D(1, 1); _solid.SetPixel(0, 0, c); _solid.Apply(); _sc = c; }
        return _solid;
    }

    void Bar(Rect r, float frac, Color fill)
    {
        Color old = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.7f); GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = fill;
        GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, (r.width - 4) * Mathf.Clamp01(frac), r.height - 4), Texture2D.whiteTexture);
        GUI.color = old;
    }

    void OnGUI()
    {
        if (!battleActive) return;
        EnsureStyles();
        float W = Screen.width, H = Screen.height;

        // Black battle background (front view)
        GUI.color = Color.black; GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture); GUI.color = Color.white;

        // Enemy image, centered upper area
        if (enemyTexture != null)
            GUI.DrawTexture(new Rect(W * 0.32f, H * 0.06f, W * 0.36f, H * 0.40f), enemyTexture, ScaleMode.ScaleToFit, true);

        // Enemy health (top-left)
        if (Enemy != null)
        {
            Rect eb = new Rect(W * 0.06f, H * 0.07f, W * 0.32f, H * 0.10f);
            GUI.Box(eb, GUIContent.none, boxStyle);
            GUI.Label(new Rect(eb.x + 14, eb.y + 8, eb.width - 28, eb.height * 0.45f),
                Enemy.name + (Enemy.Down ? "  <size=75%>戦闘不能</size>" : ""), titleStyle);
            GUI.Label(new Rect(eb.x + 14, eb.y + eb.height * 0.42f, eb.width - 28, eb.height * 0.3f),
                "<size=80%>ヘルス " + Enemy.health + "/" + Enemy.max + "</size>", labelStyle);
            Bar(new Rect(eb.x + 14, eb.y + eb.height * 0.78f, eb.width - 28, eb.height * 0.16f),
                (float)Enemy.health / Enemy.max, new Color(0.85f, 0.25f, 0.25f));
        }

        // Message log
        Rect lb = new Rect(W * 0.08f, H * 0.55f, W * 0.84f, H * 0.18f);
        GUI.Box(lb, GUIContent.none, boxStyle);
        GUI.Label(new Rect(lb.x + 18, lb.y + 12, lb.width - 36, lb.height - 24), string.Join("\n", log.ToArray()), labelStyle);

        // Player health (bottom-left)
        if (Player != null)
        {
            Rect pb = new Rect(W * 0.08f, H * 0.77f, W * 0.40f, H * 0.15f);
            GUI.Box(pb, GUIContent.none, boxStyle);
            GUI.Label(new Rect(pb.x + 16, pb.y + 10, pb.width - 32, pb.height * 0.4f),
                Player.name + (Player.Down ? "  <size=75%>戦闘不能</size>" : ""), titleStyle);
            GUI.Label(new Rect(pb.x + 16, pb.y + pb.height * 0.42f, pb.width - 32, pb.height * 0.3f),
                "<size=85%>ヘルス " + Player.health + "/" + Player.max + "</size>", labelStyle);
            Bar(new Rect(pb.x + 16, pb.y + pb.height * 0.80f, pb.width - 32, pb.height * 0.14f),
                (float)Player.health / Player.max, new Color(0.30f, 0.80f, 0.35f));
        }

        // Command / result (bottom-right)
        Rect cb = new Rect(W * 0.52f, H * 0.77f, W * 0.40f, H * 0.15f);
        GUI.Box(cb, GUIContent.none, boxStyle);
        float bw = (cb.width - 64) / 3f, bh = cb.height * 0.7f;

        if (phase == Phase.Command && !busy)
        {
            float by = cb.y + cb.height * 0.15f;
            if (GUI.Button(new Rect(cb.x + 16, by, bw, bh), "たたかう", btnStyle)) DoAttack();
            if (GUI.Button(new Rect(cb.x + 24 + bw, by, bw, bh), "アイテム", btnStyle)) DoItem();
            if (GUI.Button(new Rect(cb.x + 32 + bw * 2, by, bw, bh), "にげる", btnStyle)) DoFlee();
        }
        else if (phase == Phase.Win || phase == Phase.Lose || phase == Phase.Fled)
        {
            string msg = phase == Phase.Win ? "勝利！" : (phase == Phase.Lose ? "敗北……" : "逃走した");
            GUI.Label(new Rect(cb.x + 16, cb.y + 10, cb.width - 32, bh * 0.5f), msg, titleStyle);
            if (GUI.Button(new Rect(cb.x + 16, cb.y + cb.height * 0.45f, cb.width - 32, bh * 0.5f), "とじる", btnStyle)) EndBattle();
        }
        else
        {
            GUI.Label(new Rect(cb.x + 16, cb.y + cb.height * 0.35f, cb.width - 32, bh * 0.5f), "<size=80%>……</size>", labelStyle);
        }
    }
}
