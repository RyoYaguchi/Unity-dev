using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 永続シングルトン。プレイヤーの永続データ（HP/攻撃/防御/Lv/EXP/インベントリ/装備）と
// クエスト状態を保持し、戦闘・シーン移動をまたいで持ち越す。最初のシーンで自動生成。
public class GameManager : MonoBehaviour
{
    // Play中の再コンパイル（ドメインリロード）でstaticがnull化してもAwakeは再実行されないため、
    // null時は生存している実体を探して再取得する（DontDestroyOnLoadの実体も拾える）。
    static GameManager _instance;
    public static GameManager Instance
    {
        get { if (_instance == null) _instance = FindFirstObjectByType<GameManager>(); return _instance; }
        private set { _instance = value; }
    }

    public enum QuestState { NotOffered, Accepted, BossDefeated, Completed }

    [Header("Player stats")]
    public int level = 1;
    public int exp = 0;
    public int maxHealth = 5;
    public int curHealth = 5;
    public int baseAtk = 1;
    public int baseDef = 0;

    [Header("Inventory / Equipment")]
    public List<Item> inventory = new List<Item>();
    public Item equippedWeapon;
    public Item equippedArmor;

    [Header("Quest")]
    public QuestState quest = QuestState.NotOffered;

    // 洞窟は入場ごとに再生成されるため、開封済み宝箱・撃破済み敵を永続記録して
    // 再入場時に消し込む（アイテム/経験値の無限ファーム防止）。識別はGameObject名。
    [Header("Cave progress (anti-farm)")]
    public List<string> openedChests = new List<string>();
    public List<string> defeatedEnemies = new List<string>();

    public bool IsChestOpened(string id) { return openedChests.Contains(id); }
    public void MarkChestOpened(string id) { if (!string.IsNullOrEmpty(id) && !openedChests.Contains(id)) openedChests.Add(id); }
    public bool IsEnemyDefeated(string id) { return defeatedEnemies.Contains(id); }
    public void MarkEnemyDefeated(string id) { if (!string.IsNullOrEmpty(id) && !defeatedEnemies.Contains(id)) defeatedEnemies.Add(id); }

    public int TotalAtk { get { return baseAtk + (equippedWeapon != null ? equippedWeapon.power : 0); } }
    public int TotalDef { get { return baseDef + (equippedArmor != null ? equippedArmor.power : 0); } }

    // 最初のシーンロード前に自動生成（bootstrap）。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }

    public const string VillageScene = "SampleScene";
    public const string CaveSceneName = "CaveScene";
    public const string ChiefReturnPoint = "ChiefReturnPoint";
    private string pendingSpawn;

    void Awake()
    {
        // 重複インスタンスは破棄（シーンに置かれたものがあっても単一に保つ）。
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンロード後、指定スポーン地点へプレイヤーを配置し、操作を有効化。
    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(pendingSpawn)) return;
        var marker = GameObject.Find(pendingSpawn);
        var player = GameObject.Find("Player");
        if (marker != null && player != null)
        {
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            player.transform.position = marker.transform.position;
            Physics.SyncTransforms();
            var c = player.GetComponent("HD2DPlayerController") as MonoBehaviour;
            if (c != null) c.enabled = true;
        }
        pendingSpawn = null;
    }

    public void WarpToCave()
    {
        pendingSpawn = "EntranceSpawn";
        SceneManager.LoadScene(CaveSceneName);
    }

    public void WarpToVillage(string spawn)
    {
        pendingSpawn = string.IsNullOrEmpty(spawn) ? ChiefReturnPoint : spawn;
        SceneManager.LoadScene(VillageScene);
    }

    // 経験値付与：必要EXP=Lv×2。Lvアップで最大HP+2/現在HP+2。超過は持ち越し、複数Lvも連続処理。
    public void AddExp(int amount)
    {
        if (amount <= 0) return;
        exp += amount;
        while (exp >= level * 2)
        {
            exp -= level * 2;
            level++;
            maxHealth += 2;
            curHealth += 2;
        }
    }

    public void FullHeal() { curHealth = maxHealth; }

    public void Damage(int d) { curHealth = Mathf.Max(0, curHealth - d); }

    public void Heal(int h) { curHealth = Mathf.Min(maxHealth, curHealth + h); }

    public void AddItem(Item item) { if (item != null) inventory.Add(item); }

    // 装備（手動UIから呼ぶ）。所持品にあるもののみ装備可。
    public bool Equip(Item item)
    {
        if (item == null || !inventory.Contains(item)) return false;
        if (item.type == ItemType.Weapon) { equippedWeapon = item; return true; }
        if (item.type == ItemType.Armor) { equippedArmor = item; return true; }
        return false;
    }

    // やくそう使用：回復して所持品から消費。満タン/対象外はfalse。
    public bool UseHerb(Item item)
    {
        if (item == null || item.type != ItemType.Herb || !inventory.Contains(item)) return false;
        if (curHealth >= maxHealth) return false;
        Heal(item.power);
        inventory.Remove(item);
        return true;
    }
}
