using UnityEngine;

// プレイヤーが触れると村↔洞窟へワープするトリガー。実際のロード/配置は GameManager が行う。
public class WarpTrigger : MonoBehaviour
{
    public enum Dest { Village, Cave }
    public Dest dest = Dest.Village;
    public string spawnName = "ChiefReturnPoint";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var gm = GameManager.Instance;
        if (gm == null) return;
        if (dest == Dest.Cave) gm.WarpToCave();
        else gm.WarpToVillage(spawnName);
    }
}
