using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpForce = 7f;           // 跳跃初速度

    public Camera playerCamera;   // 用于鼠标朝向

    private CharacterController controller;
    private Vector3 velocity;
    private Animator animator;
    private float currentSpeed = 0f;
    private bool isJumping = false;        // 追踪跳跃状态

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        // 如果没有Animator，尝试在子物体中找
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        // HandleRotationToMouse();  // ← 已注释：由PlayerAimAndShoot统一控制方向
        HandleMovement();         // WASD 世界坐标移动
        HandleJump();             // 空格键跳跃
        UpdateAnimator();         // 更新动画参数
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A D
        float vertical   = Input.GetAxisRaw("Vertical");   // W S

        // 以世界坐标为基准：X 左右，Z 前后
        Vector3 move = new Vector3(horizontal, 0f, vertical);
        if (move.sqrMagnitude > 1f) move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);
        
        // 计算当前速度（用于动画）
        currentSpeed = move.magnitude;

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
