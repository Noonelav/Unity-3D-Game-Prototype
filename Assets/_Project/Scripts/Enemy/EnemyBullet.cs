using UnityEngine;

/// <summary>
/// 敌人子弹脚本
/// 碰到玩家 → 扣血；碰到其他碰撞体 → 销毁；超时自动销毁
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Settings")]
    public int   damage    = 10;
    public float lifetime  = 5f;   // 最长存活秒数

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 命中玩家
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            if (ph == null) ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // 忽略敌人自身（避免和发射者碰撞）
        if (other.GetComponentInParent<Enemy>() != null) return;

        // 命中场景中其他物体 → 销毁子弹
        Destroy(gameObject);
    }
}
