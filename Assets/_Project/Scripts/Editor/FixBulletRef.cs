using UnityEngine;
using UnityEditor;

/// <summary>
/// 修复 Player 1 的 bulletPrefab 引用
/// 菜单：Tools → Fix Bullet Reference
/// </summary>
public class FixBulletRef
{
    [MenuItem("Tools/Fix Bullet Reference")]
    public static void Fix()
    {
        // 找 Player 1
        GameObject player = GameObject.Find("Player 1");
        if (player == null)
        {
            Debug.LogError("[FixBulletRef] 场景中找不到 'Player 1'！");
            return;
        }

        PlayerAimAndShoot aim = player.GetComponent<PlayerAimAndShoot>();
        if (aim == null)
        {
            Debug.LogError("[FixBulletRef] Player 1 上没有 PlayerAimAndShoot 组件！");
            return;
        }

        // 加载子弹 Prefab 资源
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_ThirdParty/Prefabs_Old/BulletTemp.prefab");

        if (bulletPrefab == null)
        {
            Debug.LogError("[FixBulletRef] 找不到 BulletTemp.prefab！路径：Assets/_ThirdParty/Prefabs_Old/BulletTemp.prefab");
            return;
        }

        aim.bulletPrefab = bulletPrefab;
        aim.bulletSpeed  = 70f;
        aim.aimMask      = ~0;   // Everything：准星射线打所有层（含敌人头部）

        // 标记场景已修改
        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.scene);

        Debug.Log($"[FixBulletRef] ✅ 完成！bulletPrefab → {bulletPrefab.name}，aimMask → Everything");
    }
}
