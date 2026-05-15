using UnityEngine;

/// <summary>
/// 物品数据类（可在 Inspector 配置）
/// </summary>
[System.Serializable]
public class Item
{
    public ItemType type;
    public string   displayName = "Item";
    [Tooltip("Heal=回血量, Ammo=弹药数, Key=数量(通常1)")]
    public int amount = 1;
    [Tooltip("Key 专用：1/2/3，对应 key1.png / key2.png / key3.png")]
    public int keyId  = 0;
    public Sprite icon;   // UI 显示用（后期可填）
}
