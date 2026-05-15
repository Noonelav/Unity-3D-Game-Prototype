using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 图标加载器 - 从 Resources/Icons/ 加载物品图标
/// 文件名约定：Heal.png / Ammo.png / Key.png / Player.png
/// 找不到时返回 null（UI 自己画占位色块）
/// </summary>
public static class IconLoader
{
    private static Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

    public static Texture2D Get(string name)
    {
        if (cache.TryGetValue(name, out Texture2D tex) && tex != null) return tex;
        tex = Resources.Load<Texture2D>("Icons/" + name);
        if (tex != null) cache[name] = tex;
        return tex;
    }

    public static Texture2D ForItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.Heal: return Get("heal");
            case ItemType.Ammo: return Get("ammo");
            case ItemType.Key:  return Get("key1");  // 默认（无 keyId 时）
        }
        return null;
    }

    /// <summary>带 keyId 的版本，钥匙加载 key1/key2/key3</summary>
    public static Texture2D ForItem(Item item)
    {
        if (item == null) return null;
        if (item.type == ItemType.Key)
        {
            int id = Mathf.Clamp(item.keyId, 1, 3);
            return Get($"key{id}");
        }
        return ForItem(item.type);
    }

    public static Color FallbackColor(ItemType type)
    {
        switch (type)
        {
            case ItemType.Heal: return new Color(0.2f, 0.7f, 0.2f);
            case ItemType.Ammo: return new Color(0.95f, 0.7f, 0.1f);
            case ItemType.Key:  return new Color(0.75f, 0.55f, 0.15f);
        }
        return Color.gray;
    }
}
