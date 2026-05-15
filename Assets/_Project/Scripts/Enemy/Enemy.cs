using UnityEngine;

/// <summary>
/// 敌人 AI 主脚本
/// 状态机：Idle → Chase → Attack → Dead
/// 不依赖 NavMesh，直接向玩家移动（适合原型阶段）
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour
{
    // ──────────────────────────── 数值配置 ────────────────────────────
    [Header("Stats")]
    public int   maxHealth    = 50;
    public int   attackDamage = 10;
    public float moveSpeed    = 3f;
    public float gravity      = -9.81f;

    [Header("Detection")]
    public float detectionRange = 10f;  // 发现玩家的距离
    public float attackRange    = 1.8f; // 开始攻击的距离
    public float loseRange      = 14f;  // 超过此距离回到 Idle

    [Header("Attack")]
    public float attackCooldown = 1.2f; // 两次近战攻击间隔（秒）

    [Header("Ranged Attack")]
    public float      shootRange    = 8f;    // 进入射击状态的距离
    public float      shootCooldown = 1.5f;  // 射击间隔（秒）
    public int        shootDamage   = 10;    // 子弹伤害
    public float      bulletSpeed   = 18f;   // 子弹飞行速度
    public GameObject enemyBulletPrefab;     // 拖入 EnemyBullet prefab
    public Transform  firePoint;             // 枪口位置（可留空，自动用敌人中心+偏移）

    [Header("Death")]
    public float fallDuration  = 0.7f;  // 倒地动画时长（秒）
    public float corpseDuration = 60f;  // 尸体保留时间（秒）

    [Header("跳跃/障碍处理")]
    public float stepCheckDistance = 0.6f;  // 前方检测距离
    public float jumpForce         = 7f;    // 跳跃初速度
    public float jumpCooldown      = 0.5f;  // 跳跃冷却（秒）
    public float stuckThreshold    = 0.2f;  // 多长时间没动算卡住

    // ──────────────────────────── 状态枚举 ────────────────────────────
    public enum EnemyState { Idle, Chase, Shoot, Attack, Dead }
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    // ──────────────────────────── 内部变量 ────────────────────────────
    private int   currentHealth;
    private float lastAttackTime = -999f;
    private float lastShootTime  = -999f;
    private float verticalVelocity = 0f;

    // ── 卡死/跳跃检测 ──
    private Vector3 lastPosition;
    private float   stuckTimer = 0f;
    private float   lastJumpTime = -999f;

    private Transform       playerTransform;
    private CharacterController controller;
    private EnemyHealthBar  healthBar;   // 可选，有就更新

    // ──────────────────────────── 初始化 ──────────────────────────────
    void Awake()
    {
        controller  = GetComponent<CharacterController>();
        healthBar   = GetComponentInChildren<EnemyHealthBar>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        // 自动寻找玩家（通过 Tag）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning($"[Enemy] 找不到 Tag 为 'Player' 的对象，请确认玩家 Tag 已设置。");

        healthBar?.SetMaxHealth(maxHealth);
    }

    // ──────────────────────────── 主循环 ──────────────────────────────
    void Update()
    {
        if (CurrentState == EnemyState.Dead) return;
        if (playerTransform == null) return;

        ApplyGravity();

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        switch (CurrentState)
        {
            case EnemyState.Idle:   UpdateIdle(dist);   break;
            case EnemyState.Chase:  UpdateChase(dist);  break;
            case EnemyState.Shoot:  UpdateShoot(dist);  break;
            case EnemyState.Attack: UpdateAttack(dist); break;
        }
    }

    // ──────────────────────── 状态更新函数 ───────────────────────────
    void UpdateIdle(float dist)
    {
        if (dist <= detectionRange)
            TransitionTo(EnemyState.Chase);
    }

    void UpdateChase(float dist)
    {
        if (dist > loseRange)
        {
            TransitionTo(EnemyState.Idle);
            return;
        }

        // 进入近战范围 → Attack
        if (dist <= attackRange)
        {
            TransitionTo(EnemyState.Attack);
            return;
        }

        // 进入射击范围且有子弹Prefab → Shoot
        if (dist <= shootRange && enemyBulletPrefab != null)
        {
            TransitionTo(EnemyState.Shoot);
            return;
        }

        MoveTowardsPlayer();
        FacePlayer();
    }

    void UpdateShoot(float dist)
    {
        FacePlayer();

        // 玩家跑远 → Chase
        if (dist > loseRange)
        {
            TransitionTo(EnemyState.Idle);
            return;
        }

        // 玩家超出射击范围 → 继续追
        if (dist > shootRange)
        {
            TransitionTo(EnemyState.Chase);
            return;
        }

        // 玩家进入近战范围 → 近战
        if (dist <= attackRange)
        {
            TransitionTo(EnemyState.Attack);
            return;
        }

        // 站定射击
        if (Time.time - lastShootTime >= shootCooldown)
        {
            PerformShoot();
        }
    }

    void UpdateAttack(float dist)
    {
        FacePlayer();

        if (dist > attackRange)
        {
            TransitionTo(EnemyState.Chase);
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }
    }

    // ──────────────────────────── 动作 ───────────────────────────────
    void MoveTowardsPlayer()
    {
        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0f;
        dir.Normalize();

        // 水平移动
        controller.Move(dir * moveSpeed * Time.deltaTime);

        // ── 卡死/障碍检测：触发跳跃 ──
        if (controller.isGrounded && Time.time - lastJumpTime > jumpCooldown)
        {
            // 检测前方有没有障碍
            Vector3 footPos = transform.position + Vector3.up * 0.3f;
            bool obstacle = Physics.Raycast(footPos, dir, stepCheckDistance, ~0, QueryTriggerInteraction.Ignore);

            // 卡死计时
            float movedDist = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(lastPosition.x, 0, lastPosition.z));
            if (movedDist < 0.03f)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;

            // 触发跳跃：前方有障碍 或 卡住超过阈值
            if (obstacle || stuckTimer >= stuckThreshold)
            {
                Jump();
                stuckTimer = 0f;
            }
        }

        lastPosition = transform.position;
    }

    void Jump()
    {
        verticalVelocity = jumpForce;
        lastJumpTime = Time.time;
    }

    void FacePlayer()
    {
        Vector3 lookDir = playerTransform.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                10f * Time.deltaTime
            );
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void PerformAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log($"[Enemy] 近战攻击玩家！伤害：{attackDamage}");

        // 对玩家造成伤害
        PlayerHealth ph = playerTransform.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(attackDamage);
    }

    void PerformShoot()
    {
        lastShootTime = Time.time;

        // 发射点：有 firePoint 用 firePoint，否则用自身位置 + 1m高度
        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 1.2f;

        // 瞄准玩家胸口（+1m）
        Vector3 targetPos = playerTransform.position + Vector3.up * 1f;
        Vector3 dir = (targetPos - spawnPos).normalized;

        GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.LookRotation(dir));

        // 设置子弹伤害
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null) eb.damage = shootDamage;

        // 给子弹施加速度
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = dir * bulletSpeed;

        Debug.Log($"[Enemy] 射击！方向：{dir}");
    }

    void TransitionTo(EnemyState newState)
    {
        CurrentState = newState;
    }

    // ──────────────────────────── 受击 / 死亡 ────────────────────────
    /// <summary>被子弹或其他来源调用</summary>
    public void TakeDamage(int amount)
    {
        if (CurrentState == EnemyState.Dead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        healthBar?.SetHealth(currentHealth);

        // 被打到就追玩家
        if (CurrentState == EnemyState.Idle)
            TransitionTo(EnemyState.Chase);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        TransitionTo(EnemyState.Dead);
        Debug.Log($"[Enemy] {gameObject.name} 死亡！");

        // 停止动画
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        // 禁用 CharacterController，防止子弹/物理继续触发
        controller.enabled = false;

        // 禁用 EnemyHitEffect，尸体不需要受击闪烁
        var hitEffect = GetComponent<EnemyHitEffect>();
        if (hitEffect != null) hitEffect.enabled = false;

        // 生成战利品并添加尸体交互组件
        EnemyCorpse corpse = gameObject.AddComponent<EnemyCorpse>();
        corpse.corpseDuration = corpseDuration;
        corpse.GenerateLoot();

        StartCoroutine(DeathRoutine());
    }

    System.Collections.IEnumerator DeathRoutine()
    {
        // ── 阶段 1：倒地动画（向前旋转 90°）────────────────────────
        float elapsed    = 0f;
        Quaternion startRot = transform.rotation;
        // 朝当前朝向的右侧倒下（侧倒更自然）
        Quaternion fallRot  = startRot * Quaternion.Euler(90f, 0f, 0f);

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / fallDuration);
            transform.rotation = Quaternion.Slerp(startRot, fallRot, t);
            yield return null;
        }
        transform.rotation = fallRot;

        // ── 阶段 2：保持尸体，等待 corpseDuration 后销毁 ────────────
        // EnemyCorpse.Start() 里已经调用了 Destroy(gameObject, corpseDuration)
        // 这里不需要重复销毁
    }

    // ──────────────────────────── Gizmo 辅助 ─────────────────────────
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 黄色 = 检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 橙色 = 射击范围
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, shootRange);

        // 红色 = 近战范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 灰色 = 丢失范围
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
#endif
}
