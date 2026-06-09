using System.Collections;
using UnityEngine;

// 村の全快ポイント。範囲内で E を押すと HP 全回復。
public class HealPoint : MonoBehaviour
{
    private bool inRange = false;

    void Update()
    {
        if (!inRange) return;
        if (BattleSystem.Instance != null && BattleSystem.Instance.battleActive) return;
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        bool e = kb != null && kb.eKey.wasPressedThisFrame;
#else
        bool e = Input.GetKeyDown(KeyCode.E);
#endif
        if (e) Heal();
    }

    public void Heal()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.FullHeal();
        SpawnFloatingText("ヘルスが 全回復した！");
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) inRange = true; }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) inRange = false; }

    void SpawnFloatingText(string text)
    {
        GameObject t = new GameObject("HealText");
        t.transform.position = transform.position + Vector3.up * 1.8f;
        t.AddComponent<HD2DBillboard>();
        var tm = t.AddComponent<TextMesh>();
        tm.text = text; tm.fontSize = 24; tm.color = new Color(0.4f, 1f, 0.5f);
        tm.alignment = TextAlignment.Center; tm.anchor = TextAnchor.MiddleCenter;
        t.transform.localScale = Vector3.one * 0.08f;
        StartCoroutine(Rise(t, tm));
    }

    IEnumerator Rise(GameObject go, TextMesh tm)
    {
        float e = 0f; Vector3 sp = go.transform.position;
        while (e < 1.6f)
        {
            if (go == null) yield break;
            e += Time.deltaTime;
            go.transform.position = sp + Vector3.up * (e / 1.6f * 1.2f);
            Color c = tm.color; c.a = Mathf.Lerp(1f, 0f, e / 1.6f); tm.color = c;
            yield return null;
        }
        Destroy(go);
    }
}
