using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 一键生成敌人 Prefab
/// 菜单：Tools → Create Enemy Prefab
///
/// 结构：与玩家完全相同的角色模型（DefaultCharacterMeshPBR）
/// 区别：红色材质 + Enemy/EnemyHitEffect 脚本，无玩家控制脚本
/// </summary>
public class CreateEnemyPrefab
{
    // ── 资产路径 ─────────────────────────────────────────────────────
    const string MeshFbxPath        = "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Mesh/DefaultCharacterMeshPBR.fbx";
    const string AvatarFbxPath      = "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Mesh/DefaultCharacterMeshPolyart.fbx";
    const string ControllerPath     = "Assets/_Project/Materials/PlayerWalkController.controller";

    const string MatBody01Path      = "Assets/_Project/Materials/Character/PBR_Body01.mat";
    const string MatBody02Path      = "Assets/_Project/Materials/Character/PBR_Body02.mat";
    const string MatHead01Path      = "Assets/_Project/Materials/Character/PBR_Head01.mat";
    const string MatHead02Path      = "Assets/_Project/Materials/Character/PBR_Head02.mat";

    const string EnemyMatDir        = "Assets/_Project/Materials/Character";
    const string OutputPrefabPath   = "Assets/_Project/Prefabs/Enemy.prefab";

    // ─────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Create Enemy Prefab")]
    public static void CreatePrefab()
    {
        // ── 1. 加载 FBX 内的 Mesh ────────────────────────────────────
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(MeshFbxPath);

        Mesh bodyMesh01 = FindMesh(assets, "Body01");
        Mesh bodyMesh02 = FindMesh(assets, "Body02");
        Mesh headMesh01 = FindMesh(assets, "Head01");
        Mesh headMesh02 = FindMesh(assets, "Head02");

        if (bodyMesh01 == null || headMesh01 == null)
        {
            Debug.LogError("[CreateEnemyPrefab] 找不到角色 Mesh，请检查路径：" + MeshFbxPath);
            return;
        }

        // ── 2. 加载 Avatar & Controller ──────────────────────────────
        Avatar avatar = LoadAssetFromFbx<Avatar>(AvatarFbxPath, "DefaultCharacterMeshPolyartAvatar");
        RuntimeAnimatorController controller =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        if (avatar == null)
            Debug.LogWarning("[CreateEnemyPrefab] Avatar 加载失败，动画可能不正常。");
        if (controller == null)
            Debug.LogWarning("[CreateEnemyPrefab] AnimatorController 加载失败，Enemy 将没有动画。");

        // ── 3. 加载玩家骨骼参考（用于复制骨骼绑定） ─────────────────
        //    通过场景中的 Player 1 获取骨骼 Transform 引用
        GameObject playerObj = GameObject.Find("Player 1");
        if (playerObj == null)
        {
            Debug.LogError("[CreateEnemyPrefab] 场景中找不到 'Player 1'，请确保 MainLevel 场景已打开。");
            return;
        }

        // ── 4. 创建红色敌人材质 ──────────────────────────────────────
        Material matBody01Enemy = CreateEnemyMaterial("Enemy_Body01",
            AssetDatabase.LoadAssetAtPath<Material>(MatBody01Path), new Color(1f, 0.25f, 0.25f));
        Material matBody02Enemy = CreateEnemyMaterial("Enemy_Body02",
            AssetDatabase.LoadAssetAtPath<Material>(MatBody02Path), new Color(1f, 0.25f, 0.25f));
        Material matHead01Enemy = CreateEnemyMaterial("Enemy_Head01",
            AssetDatabase.LoadAssetAtPath<Material>(MatHead01Path), new Color(1f, 0.30f, 0.30f));
        Material matHead02Enemy = CreateEnemyMaterial("Enemy_Head02",
            AssetDatabase.LoadAssetAtPath<Material>(MatHead02Path), new Color(1f, 0.30f, 0.30f));

        // ── 5. 构建 GameObject 层级（镜像玩家结构）──────────────────
        GameObject enemyRoot = new GameObject("Enemy");
        enemyRoot.tag = "Enemy";

        // 从玩家复制骨骼子树（root / pelvis / ...）
        Transform playerRoot = playerObj.transform.Find("root");
        if (playerRoot == null)
        {
            Debug.LogError("[CreateEnemyPrefab] 找不到 Player 1/root，骨骼结构可能不同。");
            Object.DestroyImmediate(enemyRoot);
            return;
        }
        GameObject boneRoot = Object.Instantiate(playerRoot.gameObject, enemyRoot.transform);
        boneRoot.name = "root";
        // 清除 FirePoint（玩家专用）
        Transform firePoint = boneRoot.transform.Find("FirePoint");
        if (firePoint != null) Object.DestroyImmediate(firePoint.gameObject);

        // ── 6. 添加 SkinnedMeshRenderer（复制玩家绑定） ─────────────
        SkinnedMeshRenderer playerBody01SMR =
            playerObj.transform.Find("Body01")?.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer playerHead01SMR =
            playerObj.transform.Find("Head01")?.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer playerBody02SMR =
            playerObj.transform.Find("Body02")?.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer playerHead02SMR =
            playerObj.transform.Find("Head02")?.GetComponent<SkinnedMeshRenderer>();

        // 重新映射骨骼到 enemyRoot 下的同名骨骼
        Transform[] enemyBones = enemyRoot.GetComponentsInChildren<Transform>(true);

        AddSMR(enemyRoot, "Body01", bodyMesh01, playerBody01SMR, enemyBones,
               new Material[]{ matBody01Enemy });
        AddSMR(enemyRoot, "Body02", bodyMesh02, playerBody02SMR, enemyBones,
               new Material[]{ matBody02Enemy });
        AddSMR(enemyRoot, "Head01", headMesh01, playerHead01SMR, enemyBones,
               new Material[]{ matHead01Enemy });
        AddSMR(enemyRoot, "Head02", headMesh02, playerHead02SMR, enemyBones,
               new Material[]{ matHead02Enemy });

        // ── 7. 添加 Animator ────────────────────────────────────────
        Animator anim = enemyRoot.AddComponent<Animator>();
        if (avatar != null) anim.avatar = avatar;
        if (controller != null) anim.runtimeAnimatorController = controller;
        anim.applyRootMotion = false;   // 敌人不用根运动，用 CharacterController 驱动

        // ── 8. 添加 CharacterController ──────────────────────────────
        CharacterController cc = enemyRoot.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.radius = 0.35f;
        cc.height = 1.8f;

        // ── 9. 清理骨骼根节点上多余的玩家组件 ───────────────────────
        //    复制 Player 的 root 子物体时会带来 CharacterController / PlayerAimAndShoot
        CleanupPlayerComponents(boneRoot);

        // ── 10. 添加游戏逻辑组件 ──────────────────────────────────────
        enemyRoot.AddComponent<Enemy>();
        enemyRoot.AddComponent<EnemyHitEffect>();

        // ── 10. 保存为 Prefab ─────────────────────────────────────────
        string dir = Path.GetDirectoryName(OutputPrefabPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        AssetDatabase.Refresh();

        bool savedOk;
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(enemyRoot, OutputPrefabPath, out savedOk);

        Object.DestroyImmediate(enemyRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (savedOk)
        {
            Debug.Log($"[CreateEnemyPrefab] ✅ Enemy Prefab 已保存到：{OutputPrefabPath}");
            Selection.activeObject = savedPrefab;
            EditorGUIUtility.PingObject(savedPrefab);
        }
        else
        {
            Debug.LogError("[CreateEnemyPrefab] ❌ Prefab 保存失败！");
        }
    }

    // ─────────────────────── 工具函数 ────────────────────────────────

    static Mesh FindMesh(Object[] assets, string meshName)
    {
        foreach (var a in assets)
            if (a is Mesh m && m.name == meshName)
                return m;
        return null;
    }

    static T LoadAssetFromFbx<T>(string fbxPath, string assetName) where T : Object
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var a in assets)
            if (a is T t && t.name == assetName)
                return t;
        return null;
    }

    /// <summary>
    /// 以玩家 SMR 的骨骼绑定为模板，在 parent 下创建新 SMR 子物体，骨骼重新映射到 enemyBones
    /// </summary>
    static void AddSMR(GameObject parent, string name, Mesh mesh,
                        SkinnedMeshRenderer srcSMR, Transform[] enemyBones, Material[] mats)
    {
        if (mesh == null) return;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = mesh;
        smr.materials  = mats;

        if (srcSMR != null)
        {
            // 按骨骼名字重新映射
            Transform[] srcBones = srcSMR.bones;
            Transform[] newBones = new Transform[srcBones.Length];
            for (int i = 0; i < srcBones.Length; i++)
            {
                string boneName = srcBones[i]?.name;
                newBones[i] = FindBoneByName(enemyBones, boneName);
            }
            smr.bones = newBones;

            // 根骨骼
            string rootBoneName = srcSMR.rootBone?.name;
            smr.rootBone = FindBoneByName(enemyBones, rootBoneName);

            // 绑定姿势 & 局部 AABB
            smr.sharedMesh = mesh; // 绑定姿势来自 Mesh 本身，无需单独设置
        }
    }

    /// <summary>递归移除骨骼层级中所有玩家专用组件</summary>
    static void CleanupPlayerComponents(GameObject boneRoot)
    {
        string[] playerTypes = {
            "CharacterController", "PlayerMovement", "PlayerAimAndShoot",
            "PlayerHealth", "PlayerStats", "PlayerInteraction"
        };
        foreach (string typeName in playerTypes)
        {
            // 搜索当前物体及所有子物体
            foreach (var comp in boneRoot.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                if (comp.GetType().Name == typeName)
                    Object.DestroyImmediate(comp);
            }
        }
    }

    static Transform FindBoneByName(Transform[] bones, string boneName)
    {
        if (string.IsNullOrEmpty(boneName)) return null;
        foreach (var b in bones)
            if (b != null && b.name == boneName)
                return b;
        return null;
    }

    /// <summary>
    /// 复制一个材质球，改变颜色（用于做敌人专用红色版本）
    /// </summary>
    static Material CreateEnemyMaterial(string matName, Material source, Color tintColor)
    {
        string path = $"{EnemyMatDir}/Enemy_{matName}.mat";

        // 已存在则直接复用
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Material mat = source != null ? new Material(source) : new Material(Shader.Find("Standard"));
        mat.name = matName;

        // 叠加红色色调
        if (mat.HasProperty("_Color"))
        {
            Color orig = mat.color;
            mat.color  = orig * tintColor;
        }

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
