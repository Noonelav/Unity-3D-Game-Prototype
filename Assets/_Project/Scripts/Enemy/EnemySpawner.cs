using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人波次生成器
/// 将此脚本放在场景中任意空物体上，配置好 enemyPrefab 和 spawnPoints 即可
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ──────────────────────────── 配置 ────────────────────────────────
    [Header("Prefab")]
    public GameObject enemyPrefab;          // 敌人 Prefab

    [Header("生成点")]
    public Transform[] spawnPoints;         // 场景中的生成位置（空物体）

    [Header("波次设置")]
    public int   enemiesPerWave     = 5;    // 每波敌人数量
    public float timeBetweenWaves   = 8f;   // 两波之间的间隔（秒）
    public float timeBetweenSpawns  = 0.5f; // 同一波内每个敌人的生成间隔
    public int   maxEnemiesOnField  = 15;   // 场上最多存在的敌人数量
    public bool  infiniteWaves      = true; // 是否无限波次

    [Header("波次缩放（难度递增）")]
    public int   waveEnemyIncrement = 2;    // 每波增加的敌人数量
    public float maxEnemyScalePerWave = 2f; // 每波敌人血量/移速最大倍率

    // ──────────────────────────── 运行时状态 ──────────────────────────
    public int  CurrentWave    { get; private set; } = 0;
    public bool IsSpawning     { get; private set; } = false;

    private List<Enemy> activeEnemies = new List<Enemy>();

    // ──────────────────────────── 初始化 ──────────────────────────────
    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] 未设置 enemyPrefab！");
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] 没有设置 spawnPoints，将使用 EnemySpawner 自身位置生成。");
        }

        StartCoroutine(SpawnLoop());
    }

    // ──────────────────────────── 波次循环 ────────────────────────────
    IEnumerator SpawnLoop()
    {
        while (infiniteWaves || CurrentWave < 10)   // 10 波上限（可改）
        {
            // 等待上一波清空或超时再开始下一波
            yield return new WaitForSeconds(timeBetweenWaves);
            yield return StartCoroutine(SpawnWave());
        }

        Debug.Log("[EnemySpawner] 所有波次已完成！");
    }

    IEnumerator SpawnWave()
    {
        CurrentWave++;
        int count = enemiesPerWave + (CurrentWave - 1) * waveEnemyIncrement;
        float scaleFactor = Mathf.Min(1f + (CurrentWave - 1) * 0.15f, maxEnemyScalePerWave);

        Debug.Log($"[EnemySpawner] ── 第 {CurrentWave} 波开始，共 {count} 只敌人，强度倍率 x{scaleFactor:F2} ──");
        IsSpawning = true;

        for (int i = 0; i < count; i++)
        {
            // 超出上限时等待
            CleanDeadEnemies();
            while (activeEnemies.Count >= maxEnemiesOnField)
            {
                CleanDeadEnemies();
                yield return new WaitForSeconds(1f);
            }

            SpawnOne(scaleFactor);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        IsSpawning = false;
        Debug.Log($"[EnemySpawner] 第 {CurrentWave} 波生成完毕。");
    }

    // ──────────────────────────── 单个生成 ────────────────────────────
    void SpawnOne(float scaleFactor)
    {
        Vector3 spawnPos = GetRandomSpawnPoint();
        GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 按波次缩放血量和移速（难度递增）
            enemy.maxHealth   = Mathf.RoundToInt(enemy.maxHealth   * scaleFactor);
            enemy.moveSpeed   = enemy.moveSpeed * Mathf.Min(scaleFactor, 1.5f); // 速度最多 1.5x
            enemy.attackDamage = Mathf.RoundToInt(enemy.attackDamage * scaleFactor);

            activeEnemies.Add(enemy);
        }
    }

    // ──────────────────────────── 工具函数 ────────────────────────────
    Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            return spawnPoints[idx].position;
        }
        return transform.position;
    }

    void CleanDeadEnemies()
    {
        activeEnemies.RemoveAll(e => e == null);
    }

    // ──────────────────────────── 公共方法 ────────────────────────────
    /// <summary>立即开始下一波（可绑定 UI 按钮）</summary>
    public void ForceNextWave()
    {
        if (!IsSpawning)
            StartCoroutine(SpawnWave());
    }

    // ──────────────────────────── Gizmo ──────────────────────────────
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var sp in spawnPoints)
        {
            if (sp != null)
            {
                Gizmos.DrawWireSphere(sp.position, 0.5f);
                Gizmos.DrawLine(transform.position, sp.position);
            }
        }
    }
#endif
}
