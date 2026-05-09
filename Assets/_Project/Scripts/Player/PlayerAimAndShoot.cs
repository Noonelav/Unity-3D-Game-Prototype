using UnityEngine;
using System.Collections;

public class PlayerAimAndShoot : MonoBehaviour
{
    // ─────────────────────── 射击参数 ────────────────────────────────
    [Header("Aim Settings")]
    public LayerMask groundMask;            // 地面层（用于角色朝向计算）
    public LayerMask aimMask = ~0;          // 准星射线碰撞层（默认 Everything）

    [Header("Shoot Settings")]
    public GameObject bulletPrefab;
    public Transform  firePoint;
    public float      bulletSpeed    = 70f;
    public float      shootCooldown  = 0.15f;

    // ─────────────────────── 内部变量 ────────────────────────────────
    private Animator  animator;
    private float     lastShootTime      = -99f;
    private Coroutine shootAnimCoroutine;

    // 供外部读取当前瞄准世界坐标（CrosshairController 或其他系统使用）
    public Vector3 AimWorldPoint { get; private set; }

    // ─────────────────────── 初始化 ──────────────────────────────────
    void Start()
    {
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
    }

    // ─────────────────────── 主循环 ──────────────────────────────────
    void Update()
    {
        AimTowardsMouse();
        HandleShooting();
    }

    // ─────────────────────── 瞄准（即时朝向 + 精确击中点） ──────────
    void AimTowardsMouse()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // ── 准星精确点：对所有物体做 Raycast（敌人/地形/道具都算）──
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, aimMask))
        {
            AimWorldPoint = hit.point;
        }
        else
        {
            // 兜底：延伸射线到 500 单位远处
            AimWorldPoint = ray.GetPoint(500f);
        }

        // ── 角色朝向：只看水平方向，忽略 Y 轴 ──────────────────────
        Vector3 lookDir = AimWorldPoint - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    // ─────────────────────── 射击 ────────────────────────────────────
    void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (Time.time - lastShootTime < shootCooldown) return;

        lastShootTime = Time.time;
        Shoot();

        if (animator != null)
        {
            if (shootAnimCoroutine != null) StopCoroutine(shootAnimCoroutine);
            shootAnimCoroutine = StartCoroutine(PlayShootAnimation());
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 生成位置：firePoint 的 XZ，但 Y 固定在角色腰部以上，避免入地
        Vector3 spawnPos = firePoint.position;
        spawnPos.y = transform.position.y + 1.0f;

        // 飞行方向：从生成点指向准星精确击中点（3D 射线命中位置）
        Vector3 aimDir = (AimWorldPoint - spawnPos).normalized;
        if (aimDir == Vector3.zero) aimDir = transform.forward;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos,
                                        Quaternion.LookRotation(aimDir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = aimDir * bulletSpeed;
    }

    IEnumerator PlayShootAnimation()
    {
        animator.SetBool("IsShooting", true);
        yield return new WaitForSeconds(0.6f);
        animator.SetBool("IsShooting", false);
        shootAnimCoroutine = null;
    }
}
