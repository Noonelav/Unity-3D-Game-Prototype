using UnityEngine;

/// <summary>
/// 让物体绕Y轴自转 — 用于让物品显眼
/// </summary>
public class Spin : MonoBehaviour
{
    public Vector3 axis  = Vector3.up;
    public float   speed = 90f; // 度/秒
    public float   bobAmount = 0.15f; // 上下浮动幅度
    public float   bobSpeed  = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime, Space.World);

        // 上下浮动
        if (bobAmount > 0f)
        {
            float y = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = startPos + Vector3.up * y;
        }
    }
}
