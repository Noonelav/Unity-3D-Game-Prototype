using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器工具 - 在场景里手动添加 / 清除站位符号
/// 菜单：
///   Tools → Spawn Points → Add Ground at Cursor / Add Chest / Add House
///   Tools → Spawn Points → Clear All
/// </summary>
public class SpawnPointGenerator
{
    [MenuItem("Tools/Spawn Points/Add Ground (at selection or origin)")]
    public static void AddGround()  => AddOne(ItemSpawnPoint.SlotKind.Ground);

    [MenuItem("Tools/Spawn Points/Add Chest")]
    public static void AddChest()   => AddOne(ItemSpawnPoint.SlotKind.Chest);

    [MenuItem("Tools/Spawn Points/Add House")]
    public static void AddHouse()   => AddOne(ItemSpawnPoint.SlotKind.House);

    [MenuItem("Tools/Spawn Points/Clear All")]
    public static void ClearAll()
    {
        var all = Object.FindObjectsByType<ItemSpawnPoint>(FindObjectsSortMode.None);
        if (all.Length == 0)
        {
            Debug.Log("[SpawnPoint] 场景里没有 SpawnPoint，无需清除。");
            return;
        }
        if (!EditorUtility.DisplayDialog("Clear All", $"删除 {all.Length} 个 SpawnPoint？", "Yes", "Cancel"))
            return;
        foreach (var p in all)
            if (p != null && p.gameObject != null)
                Undo.DestroyObjectImmediate(p.gameObject);
        Debug.Log($"[SpawnPoint] 已清除 {all.Length} 个 SpawnPoint。");
    }

    // ───────────────────────────────────────────────────────
    static void AddOne(ItemSpawnPoint.SlotKind kind)
    {
        Transform root = GetOrCreateRoot();

        // 默认位置：使用 SceneView 焦点位置（编辑器视角中心）
        Vector3 pos = Vector3.zero;
        if (SceneView.lastActiveSceneView != null)
        {
            pos = SceneView.lastActiveSceneView.pivot;
            // 简单贴地：从上往下打射线
            if (Physics.Raycast(pos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 500f))
                pos = hit.point;
        }

        string name = kind switch
        {
            ItemSpawnPoint.SlotKind.Ground => $"SP_Ground_{CountOf(kind) + 1:00}",
            ItemSpawnPoint.SlotKind.Chest  => $"SP_Chest_{CountOf(kind) + 1:00}",
            ItemSpawnPoint.SlotKind.House  => $"SP_House_{CountOf(kind) + 1:00}",
            _ => "SP_New"
        };
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Add SpawnPoint");
        go.transform.position = pos;
        go.transform.SetParent(root);

        var sp = Undo.AddComponent<ItemSpawnPoint>(go);
        sp.kind = kind;

        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
        Debug.Log($"[SpawnPoint] 添加 {kind} 在 {pos}（拖动调整位置）");
    }

    static Transform GetOrCreateRoot()
    {
        var existing = GameObject.Find("SpawnPoints_Root");
        if (existing != null) return existing.transform;
        var go = new GameObject("SpawnPoints_Root");
        Undo.RegisterCreatedObjectUndo(go, "Create SpawnPoints Root");
        return go.transform;
    }

    static int CountOf(ItemSpawnPoint.SlotKind kind)
    {
        int n = 0;
        foreach (var p in Object.FindObjectsByType<ItemSpawnPoint>(FindObjectsSortMode.None))
            if (p != null && p.kind == kind) n++;
        return n;
    }
}
