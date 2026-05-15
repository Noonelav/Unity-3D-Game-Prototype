using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人死亡后自动添加的尸体组件
/// 自动挂上 Searchable，玩家可按 E 打开搜索面板
/// 最多 6 个物品槽位
/// </summary>
public class EnemyCorpse : MonoBehaviour
{
    [Header("尸体存活时间")]
    public float corpseDuration = 60f;

    [Header("最大物品槽位")]
    public int maxSlots = 6;

    [Header("掉落概率（每个格子独立判定）")]
    [Range(0f, 1f)] public float healChance = 0.30f;
    [Range(0f, 1f)] public float ammoChance = 0.55f;
    // 剩余概率为空格（什么都没有）

    [Header("数值范围")]
    public int healAmount    = 25;
    public int ammoAmountMin = 5;
    public int ammoAmountMax = 15;

    private bool lootGenerated = false;

    void Start()
    {
        // 尸体定时销毁
        Destroy(gameObject, corpseDuration);
    }

    /// <summary>在 Enemy.Die() 里调用</summary>
    public void GenerateLoot()
    {
        if (lootGenerated) return;
        lootGenerated = true;

        // 1) 保证有 trigger collider 让玩家能交互
        EnsureTriggerCollider();

        // 2) 添加 Searchable
        Searchable search = GetComponent<Searchable>();
        if (search == null) search = gameObject.AddComponent<Searchable>();
        search.containerName     = "Corpse";
        search.contents          = new List<Item>();
        search.slotCountOverride = maxSlots;   // 6 格
        search.columnsOverride   = 3;          // 3 列 × 2 行

        // 3) 自动找 SearchUI
        if (search.searchUI == null)
            search.searchUI = FindFirstObjectByType<SearchUI>(FindObjectsInactive.Include);

        // 4) 随机生成最多 maxSlots 个物品
        for (int i = 0; i < maxSlots; i++)
        {
            float r = Random.value;
            if (r < healChance)
            {
                search.contents.Add(new Item
                {
                    type        = ItemType.Heal,
                    displayName = "First Aid Kit",
                    amount      = healAmount
                });
            }
            else if (r < healChance + ammoChance)
            {
                int amt = Random.Range(ammoAmountMin, ammoAmountMax + 1);
                search.contents.Add(new Item
                {
                    type        = ItemType.Ammo,
                    displayName = "Ammo",
                    amount      = amt
                });
            }
            // 其他情况：空格子（这个 slot 没东西）
        }

        // 如果一件物品都没生成（小概率），保底给一个弹药
        if (search.contents.Count == 0)
        {
            search.contents.Add(new Item
            {
                type        = ItemType.Ammo,
                displayName = "Ammo",
                amount      = Random.Range(ammoAmountMin, ammoAmountMax + 1)
            });
        }
    }

    void EnsureTriggerCollider()
    {
        // CharacterController 已被 Enemy.Die() 禁用，需要补一个 trigger 给玩家检测
        Collider existing = GetComponent<Collider>();
        bool needTrigger = true;

        // 如果已有 Collider 且是 trigger，沿用
        if (existing != null && existing.isTrigger && existing.enabled)
        {
            needTrigger = false;
        }

        if (needTrigger)
        {
            BoxCollider bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size      = new Vector3(2f, 2f, 2f);
            bc.center    = new Vector3(0f, 1f, 0f);
        }
    }
}
