using UnityEngine;

/// <summary>
/// 物品刷新点（站位符号） - 由 ItemSpawnManager 在游戏开始时填充物品
/// 共 3 类：
///   Ground = 地面散落（药 / 弹 / 钥匙均可）
///   Chest  = 木箱容器（只刷钥匙）
///   House  = 房屋光圈容器（药 / 弹 / 钥匙均可）
/// </summary>
public class ItemSpawnPoint : MonoBehaviour
{
    public enum SlotKind
    {
        Ground,
        Chest,
        House
    }

    [Header("基本配置")]
    public SlotKind kind = SlotKind.Ground;

    [Header("Container 专属（Chest / House）")]
    [Tooltip("容器最多装几件物品")]
    public int containerCapacity = 3;

    [Header("Ground / Chest 刷新范围（球形）")]
    [Tooltip("Ground = 物品在范围内随机；Chest = 木箱在范围内随机；House = 忽略，固定在中心")]
    public float spawnRadius = 3f;

    /// <summary>在 spawnRadius 内取一个随机的、贴地的位置</summary>
    public Vector3 GetRandomSpawnPosition()
    {
        if (kind == SlotKind.House || spawnRadius <= 0.01f)
            return transform.position;

        // 在水平圆内随机
        Vector2 r2 = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(r2.x, 0f, r2.y);

        // 从上方往下打射线找地面
        if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down,
                            out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            pos.y = hit.point.y;
        else
            pos.y = transform.position.y;

        return pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Color c = ColorFor(kind);

        if (kind == SlotKind.House)
        {
            // House：固定小球，不随 radius 变化
            c.a = 0.55f; Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, 0.7f);
            c.a = 1f;    Gizmos.color = c;
            Gizmos.DrawWireSphere(transform.position, 0.7f);
        }
        else
        {
            // Ground / Chest：球体大小 = 实际刷新范围
            float r = Mathf.Max(0.4f, spawnRadius);
            c.a = 0.30f; Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, r);
            c.a = 1f;    Gizmos.color = c;
            Gizmos.DrawWireSphere(transform.position, r);
            // 中心点小标记
            c.a = 1f;    Gizmos.color = c;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

        // 头顶标杆（方便远处看见）
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 3f);
        Gizmos.DrawSphere(transform.position + Vector3.up * 3f, 0.18f);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 3.4f, kind.ToString());
    }

    static Color ColorFor(SlotKind k)
    {
        switch (k)
        {
            case SlotKind.Ground: return new Color(0.2f, 0.9f, 0.2f);   // 绿
            case SlotKind.Chest:  return new Color(1f, 0.4f, 0.1f);     // 红橙（钥匙宝箱）
            case SlotKind.House:  return new Color(1f, 0.85f, 0.2f);    // 黄
        }
        return Color.white;
    }
#endif
}
