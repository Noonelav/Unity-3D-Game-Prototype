using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人死亡后自动添加的尸体组件
/// 实现 IInteractable，玩家靠近按 E 可搜刮
/// </summary>
public class EnemyCorpse : MonoBehaviour, IInteractable
{
    // ─────────────────────── 战利品定义 ──────────────────────────────
    [System.Serializable]
    public struct LootEntry
    {
        public string itemName;
        [Range(0f, 1f)] public float dropChance;  // 0~1 掉落概率
        public int minAmount;
        public int maxAmount;
    }

    [Header("战利品表（运行时随机生成）")]
    public LootEntry[] lootTable = new LootEntry[]
    {
        new LootEntry { itemName = "弹药",   dropChance = 0.75f, minAmount = 5,  maxAmount = 15 },
        new LootEntry { itemName = "急救包", dropChance = 0.35f, minAmount = 1,  maxAmount = 1  },
        new LootEntry { itemName = "食物",   dropChance = 0.50f, minAmount = 1,  maxAmount = 3  },
    };

    [Header("尸体设置")]
    public float corpseDuration = 60f;   // 多少秒后消失
    public bool  isLooted       = false;

    // 运行时生成的实际战利品
    private List<(string name, int amount)> generatedLoot = new List<(string, int)>();
    private bool lootGenerated = false;

    // ─────────────────────── 初始化 ──────────────────────────────────
    void Start()
    {
        // 死亡后 corpseDuration 秒自动销毁
        Destroy(gameObject, corpseDuration);
    }

    /// <summary>在 Enemy.Die() 里调用，随机生成这具尸体的实际战利品</summary>
    public void GenerateLoot()
    {
        if (lootGenerated) return;
        lootGenerated = true;
        generatedLoot.Clear();

        foreach (var entry in lootTable)
        {
            if (Random.value <= entry.dropChance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                generatedLoot.Add((entry.itemName, amount));
            }
        }
    }

    // ─────────────────────── IInteractable ───────────────────────────
    public string GetPromptText()
    {
        if (isLooted) return "（已搜刮）";
        return "按 E 搜刮尸体";
    }

    public void Interact(GameObject player)
    {
        if (isLooted)
        {
            Debug.Log("[Corpse] 这具尸体已被搜刮过了。");
            return;
        }

        isLooted = true;

        if (generatedLoot.Count == 0)
        {
            Debug.Log("[Corpse] 什么都没有…");
            return;
        }

        // ── 将战利品给予玩家 ─────────────────────────────────────────
        PlayerStats stats = player.GetComponent<PlayerStats>();

        foreach (var (itemName, amount) in generatedLoot)
        {
            Debug.Log($"[Corpse] 获得：{itemName} x{amount}");

            if (stats != null)
            {
                switch (itemName)
                {
                    case "弹药":
                        stats.AddAmmo(amount);
                        break;
                    case "急救包":
                        player.GetComponent<PlayerHealth>()?.Heal(30);
                        break;
                    // 其他物品未来扩展到背包系统
                }
            }
        }

        // ── 视觉反馈：搜刮后尸体变灰 ────────────────────────────────
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = new Color(0.35f, 0.35f, 0.35f);
        }
    }
}
