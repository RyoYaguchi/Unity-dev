using System.Collections;
using UnityEngine;

public class InteractiveChest : MonoBehaviour
{
    public Sprite[] chestSprites; // Array of 3 sprites: [0] Closed, [1] Half Open, [2] Open
    public Renderer promptRenderer; // Renderer for the popup E indicator
    public string itemName = "どうのつるぎ"; // 入手アイテム名（ItemDatabase）

    private SpriteRenderer sr;
    private bool inRange = false;
    private bool isOpen = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // 既に開封済みの宝箱は再入場でも開封状態のまま（アイテム再取得不可）。
        var gm0 = GameManager.Instance;
        bool already = gm0 != null && gm0.IsChestOpened(gameObject.name);
        if (already) isOpen = true;
        if (chestSprites != null && chestSprites.Length > 0 && sr != null)
        {
            sr.sprite = (already && chestSprites.Length > 2) ? chestSprites[2] : chestSprites[0];
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
        if (sr == null) sr = GetComponent<SpriteRenderer>();   // Start未実行でも安全に

        // Frame 1: Opening (Half Open)
        if (sr != null && chestSprites != null && chestSprites.Length > 1)
        {
            sr.sprite = chestSprites[1];
            yield return new WaitForSeconds(0.12f);
        }

        if (this == null) yield break;   // 待機中にシーン遷移等で破棄されたら中断

        // Frame 2: Open
        if (sr != null && chestSprites != null && chestSprites.Length > 2)
        {
            sr.sprite = chestSprites[2];
        }

        // 実アイテムをインベントリへ付与
        var gm = GameManager.Instance;
        Item got = ItemDatabase.Create(itemName);
        if (gm != null && got != null)
        {
            gm.AddItem(got);
            gm.MarkChestOpened(gameObject.name);   // 開封を永続記録（再入場で再取得防止）
            SpawnFloatingText(got.Label() + " を てにいれた！");
        }
        else
        {
            SpawnFloatingText(itemName + " を てにいれた！");
        }
    }

    // テスト/外部から開封させる用。
    public void Open() { if (!isOpen) StartCoroutine(OpenChestRoutine()); }

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
