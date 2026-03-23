using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    public Camera playerCamera;   // 用于鼠标朝向

    private CharacterController controller;
    private Vector3 velocity;
    private Animator animator;
    private Vector3 currentMovement;

    // 动画参数名
    private readonly string SPEED_PARAM = "Speed";

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("PlayerMovement: Animator component not found on Player!");
        }
    }

    void Update()
    {
        HandleRotationToMouse();  // 鼠标控制朝向
        HandleMovement();         // WASD 世界坐标移动
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A D
        float vertical   = Input.GetAxisRaw("Vertical");   // W S

        // 以世界坐标为基准：X 左右，Z 前后
        Vector3 move = new Vector3(horizontal, 0f, vertical);
        if (move.sqrMagnitude > 1f) move.Normalize();

        currentMovement = move;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 简单重力
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 更新Animator动画参数
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // 计算移动速度（0 = 静止，1 = 全速移动）
        float speed = currentMovement.magnitude;

        // 设置Animator的Speed参数用于动画过渡
        animator.SetFloat(SPEED_PARAM, speed);
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
}
