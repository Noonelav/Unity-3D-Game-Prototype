using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家背包 — 储存治疗药品、弹药、钥匙
/// 按 4 使用治疗药品（调用 PlayerHealth.Heal）
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("药品配置")]
    public int healAmountFixed = 25;   // 每次使用药品固定回复血量

    // 已拾取的物品（按类型聚合）
    public List<Item> healItems = new List<Item>();  // 每个元素 = 1 个治疗药包
    public List<Item> keys      = new List<Item>();  // 已拾取钥匙列表（保留 keyId 用于显示）
    public int        ammo      = 0;                 // 累计弹药数
    public int        keyCount  => keys.Count;       // 已拾取钥匙数（便于其他代码访问）

    [Header("Refs")]
    public BackpackUI backpackUI;   // Tab 背包面板（可选，自动找）

    private PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (backpackUI == null) backpackUI = FindFirstObjectByType<BackpackUI>();
    }

    void Update()
    {
        // 按 4 使用治疗药品
        if (Input.GetKeyDown(KeyCode.Alpha4))
            UseHeal();
    }

    // ──────────────────────── 添加物品 ────────────────────────
    public void AddItem(Item item)
    {
        switch (item.type)
        {
            case ItemType.Heal:
                healItems.Add(item);
                Debug.Log($"[Inventory] 拾取治疗药品 (+{item.amount}HP)");
                break;
            case ItemType.Ammo:
                ammo += item.amount;
                Debug.Log($"[Inventory] 拾取弹药 +{item.amount}（共 {ammo}）");
                break;
            case ItemType.Key:
                keys.Add(item);
                Debug.Log($"[Inventory] 拾取钥匙 #{item.keyId}！现有 {keyCount}/3");
                break;
        }
        backpackUI?.Refresh();
    }

    // ──────────────────────── 使用治疗药品 ────────────────────
    public void UseHeal()
    {
        if (healItems.Count == 0)
        {
            Debug.Log("[Inventory] 没有治疗药品！");
            return;
        }
        if (playerHealth == null) return;

        // 如果满血，提示但不浪费
        if (playerHealth.currentHealth >= playerHealth.maxHealth)
        {
            Debug.Log("[Inventory] 已经满血了！");
            return;
        }

        playerHealth.Heal(healAmountFixed);
        healItems.RemoveAt(0);
        Debug.Log($"[Inventory] 使用治疗药品 +{healAmountFixed}HP（剩余 {healItems.Count}）");
        backpackUI?.Refresh();
    }

    public int HealCount => healItems.Count;
}
