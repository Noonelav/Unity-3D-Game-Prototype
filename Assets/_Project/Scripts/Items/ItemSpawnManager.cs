using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 物品刷新管理器 — 启动时扫描场景里所有 ItemSpawnPoint，按规则智能分配物品
///
/// 规则：
///   * 全图恰好 3 把钥匙
///   * 钥匙可刷在：Ground / Chest / House
///   * 治疗药品 / 弹药 只刷在：Ground / House（不在 Chest 里）
/// </summary>
public class ItemSpawnManager : MonoBehaviour
{
    [Header("Prefab 引用（拖入）")]
    public GameObject firstAidPrefab;
    public GameObject ammoPrefab;
    public GameObject keyPrefab;
    public GameObject chestPrefab;
    public GameObject searchRingPrefab;

    [Header("地图物品配置")]
    public int healCount = 6;
    public int ammoCount = 8;
    public int keyCount  = 3;     // 强制 3 把

    [Header("Item 数值")]
    public int healAmountPerPickup = 25;
    public int ammoAmountPerPickup = 30;

    void Start()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        var points = FindObjectsByType<ItemSpawnPoint>(FindObjectsSortMode.None);
        if (points.Length == 0)
        {
            Debug.LogWarning("[ItemSpawnManager] 场景里没有任何 ItemSpawnPoint！");
            return;
        }

        // 按类型分组
        var groundPts = new List<ItemSpawnPoint>();
        var chestPts  = new List<ItemSpawnPoint>();
        var housePts  = new List<ItemSpawnPoint>();
        foreach (var p in points)
        {
            if (p == null) continue;
            switch (p.kind)
            {
                case ItemSpawnPoint.SlotKind.Ground: groundPts.Add(p); break;
                case ItemSpawnPoint.SlotKind.Chest:  chestPts .Add(p); break;
                case ItemSpawnPoint.SlotKind.House:  housePts .Add(p); break;
            }
        }
        Shuffle(groundPts);
        Shuffle(chestPts);
        Shuffle(housePts);

        // 实例化所有 Chest / House 容器（每个 SpawnPoint 一个容器）
        // 用元组保存 (SpawnPoint, Searchable) 配对，方便后面查容量
        var chests = new List<(ItemSpawnPoint pt, Searchable s)>();
        foreach (var pt in chestPts)
        {
            var s = SpawnContainer(pt, chestPrefab);
            if (s != null) chests.Add((pt, s));
        }
        var houses = new List<(ItemSpawnPoint pt, Searchable s)>();
        foreach (var pt in housePts)
        {
            var s = SpawnContainer(pt, searchRingPrefab);
            if (s != null) houses.Add((pt, s));
        }

        // ── 1) 钥匙：仅在 Chest 容器里分配 3 把 ──
        // 每个 Chest 最多 1 把钥匙，保证分散
        var chestPool = new List<Searchable>();
        foreach (var (pt, s) in chests) chestPool.Add(s);
        Shuffle(chestPool);

        int keysPlaced = 0;
        var usedGroundForKey = new HashSet<ItemSpawnPoint>(); // 仍保留供 Heal/Ammo 兼容
        foreach (var s in chestPool)
        {
            if (keysPlaced >= keyCount) break;
            int id = keysPlaced + 1; // 1, 2, 3
            s.contents.Add(MakeKey(id));
            keysPlaced++;
        }
        if (keysPlaced < keyCount)
            Debug.LogWarning($"[ItemSpawnManager] 钥匙只放了 {keysPlaced}/{keyCount}，Chest 数量不够（需要至少 {keyCount} 个 Chest SpawnPoint）！");

        // ── 2) Heal：Ground + House（不放 Chest）──
        DistributeNonKey(ItemType.Heal, healCount, groundPts, houses, usedGroundForKey);

        // ── 3) Ammo：Ground + House（不放 Chest）──
        DistributeNonKey(ItemType.Ammo, ammoCount, groundPts, houses, usedGroundForKey);

        Debug.Log($"[ItemSpawnManager] 物品刷新完成。Keys={keysPlaced}/{keyCount}, Heals={healCount}, Ammo={ammoCount}");
    }

    struct KeySlot
    {
        public ItemSpawnPoint groundPoint;  // 地面单点
        public Searchable     container;    // 容器
    }

    void DistributeNonKey(ItemType type, int count,
                          List<ItemSpawnPoint> groundPts,
                          List<(ItemSpawnPoint pt, Searchable s)> houses,
                          HashSet<ItemSpawnPoint> usedGroundForKey)
    {
        // 候选池：Ground（没被钥匙占用的）+ House 容器（剩余容量）
        var pool = new List<System.Action>();
        foreach (var pt in groundPts)
        {
            if (usedGroundForKey.Contains(pt)) continue;
            var capturedPt = pt;
            pool.Add(() => SpawnGroundItem(capturedPt, type));
        }
        foreach (var (pt, s) in houses)
        {
            int cap = pt.containerCapacity;
            int remaining = cap - s.contents.Count;
            for (int i = 0; i < remaining; i++)
            {
                var captured = s;
                pool.Add(() => captured.contents.Add(MakeItem(type)));
            }
        }
        Shuffle(pool);

        int placed = 0;
        int idx = 0;
        while (placed < count && idx < pool.Count)
        {
            pool[idx]();
            placed++;
            idx++;
        }
    }

    Searchable SpawnContainer(ItemSpawnPoint pt, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[ItemSpawnManager] {pt.kind} 容器 Prefab 没设置！");
            return null;
        }
        // Chest 在范围内随机；House 固定在 SpawnPoint 中心
        Vector3 pos = pt.GetRandomSpawnPosition();
        var go = Instantiate(prefab, pos, pt.transform.rotation, pt.transform);
        go.name = $"{pt.kind}_Spawn";
        var s = go.GetComponent<Searchable>();
        if (s == null) return null;
        s.contents.Clear();  // 清空 Prefab 自带 contents
        return s;
    }

    void SpawnGroundItem(ItemSpawnPoint pt, ItemType type)
    {
        GameObject prefab = null;
        switch (type)
        {
            case ItemType.Heal: prefab = firstAidPrefab; break;
            case ItemType.Ammo: prefab = ammoPrefab;     break;
            case ItemType.Key:  prefab = keyPrefab;      break;
        }
        if (prefab == null)
        {
            Debug.LogWarning($"[ItemSpawnManager] {type} 的 Prefab 没设置！");
            return;
        }
        // Ground 在范围内随机；House 固定（House 不会进这分支）
        Vector3 pos = pt.GetRandomSpawnPosition();
        var go = Instantiate(prefab, pos, pt.transform.rotation, pt.transform);
        go.name = $"{type}_GroundItem";

        WorldItem wi = go.GetComponent<WorldItem>();
        if (wi != null) wi.item = MakeItem(type);
    }

    Item MakeItem(ItemType type)
    {
        Item it = new Item { type = type };
        switch (type)
        {
            case ItemType.Heal: it.displayName = "First Aid Kit"; it.amount = healAmountPerPickup; break;
            case ItemType.Ammo: it.displayName = "Ammo Box";       it.amount = ammoAmountPerPickup; break;
            case ItemType.Key:  it.displayName = "Key";             it.amount = 1; break;
        }
        return it;
    }

    Item MakeKey(int keyId)
    {
        return new Item
        {
            type        = ItemType.Key,
            displayName = $"Key #{keyId}",
            amount      = 1,
            keyId       = keyId
        };
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
