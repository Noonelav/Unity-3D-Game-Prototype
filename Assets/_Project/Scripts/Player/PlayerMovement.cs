using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed       = 7f;        // 基础移动速度（已提升）
    public float sprintMultiplier= 1.5f;      // 冲刺倍率（+50%）
    public float sprintMaxTime   = 10f;       // 最长冲刺时间（秒）
    public float sprintCooldown  = 15f;       // 用完后冷却（秒）

    [Header("Physics")]
    public float gravity = -9.81f;
    public float jumpForce = 7f;

    public Camera playerCamera;   // 用于鼠标朝向

    private CharacterController controller;
    private Vector3 velocity;
    private Animator animator;
    private float currentSpeed = 0f;
    private bool  isJumping = false;
    private bool  isSprinting = false;

    // 冲刺状态
    private float sprintRemaining;            // 剩余可用秒数
    private float cooldownRemaining;          // 冷却剩余秒数
    public float SprintRemaining   => sprintRemaining;
    public float SprintMax         => sprintMaxTime;
    public float CooldownRemaining => cooldownRemaining;
    public bool  IsSprinting       => isSprinting;
    public bool  OnCooldown        => cooldownRemaining > 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // 如果没有Animator，尝试在子物体中找
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        sprintRemaining   = sprintMaxTime;
        cooldownRemaining = 0f;
    }

    void Update()
    {
        // UI 打开时禁止移动 / 跳跃
        if (Cursor.visible)
        {
            currentSpeed = 0f;
            UpdateAnimator();
            // 即便不能动，重力还得施加
            ApplyGravityOnly();
            return;
        }

        // HandleRotationToMouse();  // ← 已注释：由PlayerAimAndShoot统一控制方向
        HandleMovement();         // WASD 移动
        HandleJump();             // 空格键跳跃
        UpdateAnimator();         // 更新动画参数
    }

    void ApplyGravityOnly()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A D
        float vertical   = Input.GetAxisRaw("Vertical");   // W S

        // ── 把输入转换到玩家朝向的方向（W = 玩家正前方）──
        Vector3 forward = transform.forward;
        Vector3 right   = transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 move = forward * vertical + right * horizontal;
        if (move.sqrMagnitude > 1f) move.Normalize();

        // ── 冲刺逻辑 ──
        bool wantSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool moving = move.sqrMagnitude > 0.01f;

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining < 0f) cooldownRemaining = 0f;
            isSprinting = false;
        }
        else if (wantSprint && moving && sprintRemaining > 0f)
        {
            isSprinting = true;
            sprintRemaining -= Time.deltaTime;
            if (sprintRemaining <= 0f)
            {
                sprintRemaining   = 0f;
                cooldownRemaining = sprintCooldown;   // 冲刺用完进入冷却
                isSprinting = false;
            }
        }
        else
        {
            isSprinting = false;
            // 不冲刺时缓慢恢复（按比例：15 秒回满 10 秒条 = 0.667/s）
            if (cooldownRemaining <= 0f && sprintRemaining < sprintMaxTime)
            {
                sprintRemaining += (sprintMaxTime / sprintCooldown) * Time.deltaTime;
                sprintRemaining = Mathf.Min(sprintRemaining, sprintMaxTime);
            }
        }

        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        controller.Move(move * speed * Time.deltaTime);

        // 计算当前速度（用于动画）
        currentSpeed = move.magnitude * (isSprinting ? sprintMultiplier : 1f);

        // 简单重力
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleRotationToMouse()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = targetRot;
            }
        }
    }

    void HandleJump()
    {
        // 检测空格键按下
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 仅在地面上时才能跳跃
            if (controller.isGrounded)
            {
                velocity.y = jumpForce;
                isJumping = true;
            }
        }

        // 着陆时停止跳跃状态
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            isJumping = false;
        }
    }

    /// <summary>
    /// 更新Animator参数，控制Idle/Walk/Jump动画切换
    /// </summary>
    void UpdateAnimator()
    {
        if (animator == null) return;

        // 设置Speed参数 (0 = Idle, >0 = Walk)
        animator.SetFloat("Speed", currentSpeed);

        // 设置IsJumping参数 (true = Jump, false = Idle/Walk)
        animator.SetBool("IsJumping", isJumping);
    }
}
