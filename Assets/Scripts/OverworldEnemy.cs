using UnityEngine;

// 洞窟を歩き回る敵。プレイヤーと接触すると戦闘を開始する。勝利で消滅、敗北で村へ帰還＋全回復。
// isBoss=true ならボス（撃破でクエストを BossDefeated にして村へ帰還）。
public class OverworldEnemy : MonoBehaviour
{
    [Header("Battle stats")]
    public string enemyName = "ウリセン";
    public int health = 3;
    public int atk = 1;
    public int def = 0;
    public int expReward = 1;
    public string winMessage = "ウリセンを たおした！";
    public bool isBoss = false;

    [Header("Patrol")]
    public float patrolRange = 0.6f;
    public float moveSpeed = 1.2f;

    private Vector3 a, b;
    private bool toB = true;
    private bool engaged = false;
    private float cooldown = 0f;

    void Start()
    {
        // 撃破済みの雑魚は再入場でも復活させない（ボスはクエスト状態で管理するので対象外）。
        var gm0 = GameManager.Instance;
        if (!isBoss && gm0 != null && gm0.IsEnemyDefeated(gameObject.name)) { Destroy(gameObject); return; }

        a = transform.position - new Vector3(0f, 0f, patrolRange);
        b = transform.position + new Vector3(0f, 0f, patrolRange);
    }

    void Update()
    {
        var bs = BattleSystem.Instance;
        if (bs != null && bs.battleActive) return;       // 戦闘中は停止＆再接触しない
        if (engaged) { cooldown -= Time.deltaTime; if (cooldown <= 0f) engaged = false; return; } // 戦闘後クールダウン（連戦防止）

        if (patrolRange > 0.01f)
        {
            Vector3 tgt = toB ? b : a;
            transform.position = Vector3.MoveTowards(transform.position, tgt, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, tgt) < 0.05f) toB = !toB;
        }
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) Fight(); }

    void Fight()
    {
        if (engaged) return;
        var bs = BattleSystem.Instance;
        if (bs == null || bs.battleActive) return;
        engaged = true; cooldown = 1.5f;

        var gm = GameManager.Instance;
        System.Action onLose = () => { if (gm != null) { gm.FullHeal(); gm.WarpToVillage(GameManager.ChiefReturnPoint); } };
        System.Action onWin;
        if (isBoss)
            onWin = () => { if (gm != null) { gm.quest = GameManager.QuestState.BossDefeated; gm.WarpToVillage(GameManager.ChiefReturnPoint); } };
        else
            onWin = () => { if (gm != null) gm.MarkEnemyDefeated(gameObject.name); Destroy(gameObject); };

        bs.StartBattle(enemyName, health, atk, def, expReward, winMessage, onWin, onLose);
    }
}
