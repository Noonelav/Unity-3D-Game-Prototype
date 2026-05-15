using UnityEngine;

/// <summary>
/// 摄像机跟随玩家
/// 摄像机始终跟随玩家正面方向 — 玩家转身（鼠标准星方向变）摄像机一起转
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 拖 Player1 进来
    public Vector3   offset = new Vector3(0f, 15f, -10f);
    public float     pitch  = 60f;

    [Tooltip("旋转跟随的平滑系数（0 = 立刻跟上，越大越柔和）")]
    public float yawSmoothing = 8f;

    private float currentYaw;
    private bool  initialized;

    void LateUpdate()
    {
        if (target == null) return;

        // UI 打开（光标可见）时锁定视角，不再跟随玩家朝向
        bool uiOpen = Cursor.visible;

        // 玩家的 Y 轴朝向（鼠标准星位置控制）
        float targetYaw = target.eulerAngles.y;

        if (!initialized)
        {
            currentYaw = targetYaw;
            initialized = true;
        }
        else if (!uiOpen)
        {
            // 平滑过渡（绕近路插值，避免转角抽筋）
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw,
                yawSmoothing <= 0f ? 1f : Time.deltaTime * yawSmoothing);
        }
        // uiOpen 时保持 currentYaw 不变（视角锁定）

        // 把 offset 按 currentYaw 旋转到世界空间
        Quaternion yawRot = Quaternion.Euler(0f, currentYaw, 0f);
        Vector3 rotatedOffset = yawRot * offset;

        transform.position = target.position + rotatedOffset;
        transform.rotation = Quaternion.Euler(pitch, currentYaw, 0f);
    }
}
