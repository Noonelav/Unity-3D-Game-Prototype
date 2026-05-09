using UnityEngine;
using UnityEditor;

/// <summary>
/// 自动把场景里 EnemySpawner 下的子物体 SpawnPoint_* 连接到 EnemySpawner.spawnPoints
/// 菜单：Tools → Wire Enemy Spawner
/// </summary>
public class WireEnemySpawner
{
    [MenuItem("Tools/Wire Enemy Spawner")]
    public static void Wire()
    {
        // 找场景里的 EnemySpawner 物体
        GameObject spawnerObj = GameObject.Find("EnemySpawner");
        if (spawnerObj == null)
        {
            Debug.LogError("[WireEnemySpawner] 场景中找不到 'EnemySpawner' 物体！");
            return;
        }

        EnemySpawner spawner = spawnerObj.GetComponent<EnemySpawner>();
        if (spawner == null)
        {
            Debug.LogError("[WireEnemySpawner] EnemySpawner 物体上没有 EnemySpawner 脚本！");
            return;
        }

        // 收集所有名字以 SP_ 或 SpawnPoint_ 开头的子物体
        var points = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in spawnerObj.transform)
        {
            if (child.name.StartsWith("SP_") || child.name.StartsWith("SpawnPoint_"))
                points.Add(child);
        }

        if (points.Count == 0)
        {
            Debug.LogWarning("[WireEnemySpawner] 没有找到 SpawnPoint_* 子物体！");
            return;
        }

        // 连接 Enemy Prefab
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Project/Prefabs/Enemy.prefab");
        if (enemyPrefab != null)
            spawner.enemyPrefab = enemyPrefab;

        // 连接 SpawnPoints
        spawner.spawnPoints = points.ToArray();

        // 标记场景已修改（确保保存）
        EditorUtility.SetDirty(spawner);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            spawnerObj.scene);

        Debug.Log($"[WireEnemySpawner] ✅ 完成！" +
                  $"\n  - EnemyPrefab: {enemyPrefab?.name}" +
                  $"\n  - SpawnPoints ({points.Count}): " +
                  string.Join(", ", points.ConvertAll(p => p.name)));
    }
}
