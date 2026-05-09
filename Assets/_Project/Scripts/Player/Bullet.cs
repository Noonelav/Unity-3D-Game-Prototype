using UnityEngine;

/// <summary>
/// 子弹行为脚本
/// - 超时自动销毁
/// - 碰到敌人：调用 TakeDamage 并销毁自身
/// - 碰到其他物体（墙壁等）：直接销毁
/// - 不会伤害 Player 自身（通过 Tag 过滤）
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int   damage   = 10;
    public float lifetime = 5f;

    void Start()
    {
        // 超时销毁，防止子弹飞出地图
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject hit = collision.gameObject;

        // ── 忽略玩家自身 ──────────────────────────────────────
        if (hit.CompareTag("Player")) return;

        // ── 命中敌人 ──────────────────────────────────────────
        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            // 触发受击特效（如果有）
            hit.GetComponent<EnemyHitEffect>()?.PlayHit();
        }

        // ── 无论命中什么都销毁子弹 ────────────────────────────
        Destroy(gameObject);
    }

    // Trigger 版本，兼容 IsTrigger 碰撞体的敌人
    void OnTriggerEnter(Collider other)
    {
        GameObject hit = other.gameObject;

        if (hit.CompareTag("Player")) return;

        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            hit.GetComponent<EnemyHitEffect>()?.PlayHit();
        }

        Destroy(gameObject);
    }
}
