using UnityEngine;

// 武器=攻撃+、防具=防御+、やくそう=回復 を表す最小のアイテムデータ。
public enum ItemType { Weapon, Armor, Herb }

[System.Serializable]
public class Item
{
    public string name;
    public ItemType type;
    public int power; // Weapon: +ATK / Armor: +DEF / Herb: 回復量

    public Item(string name, ItemType type, int power)
    {
        this.name = name; this.type = type; this.power = power;
    }

    public Item Clone() { return new Item(name, type, power); }

    public string Label()
    {
        string suf = type == ItemType.Weapon ? "（攻+" + power + "）"
                   : type == ItemType.Armor ? "（防+" + power + "）"
                   : "（回復" + power + "）";
        return name + suf;
    }
}

// プロトタイプ用の固定アイテム集。バランスはT14で調整。
public static class ItemDatabase
{
    public static readonly Item KinoBou   = new Item("きのぼう",   ItemType.Weapon, 1);
    public static readonly Item DouSword  = new Item("どうのつるぎ", ItemType.Weapon, 2);
    public static readonly Item NunoFuku  = new Item("ぬののふく",   ItemType.Armor,  1);
    public static readonly Item KawaYoroi = new Item("かわのよろい", ItemType.Armor,  2);
    public static readonly Item Yakusou   = new Item("やくそう",     ItemType.Herb,   3);

    public static readonly Item[] All = { KinoBou, DouSword, NunoFuku, KawaYoroi, Yakusou };

    // 名前から新しいインスタンスを取得（宝箱配置などで使用）。
    public static Item Create(string name)
    {
        foreach (var it in All) if (it.name == name) return it.Clone();
        return null;
    }
}
