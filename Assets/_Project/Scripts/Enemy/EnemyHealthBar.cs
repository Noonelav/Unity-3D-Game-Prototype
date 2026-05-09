using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人头顶血条（World Space Canvas）
/// 将此脚本挂在敌人子物体上的 Canvas → Slider
/// Canvas 需设置为 World Space，并 Billboard 朝向摄像机
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI 引用")]
    public Slider slider;

    [Header("Billboard")]
    public bool faceCamera = true;  // 始终朝向摄像机

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;

        // 如果没有手动赋值，尝试在自身/子物体中找 Slider
        if (slider == null)
            slider = GetComponentInChildren<Slider>();
    }

    void LateUpdate()
    {
        if (faceCamera && mainCamera != null)
        {
            // 让血条始终面朝摄像机（Billboard 效果）
            transform.rotation = Quaternion.LookRotation(
                transform.position - mainCamera.transform.position
            );
        }
    }

    public void SetMaxHealth(int max)
    {
        if (slider == null) return;
        slider.maxValue = max;
        slider.value    = max;
    }

    public void SetHealth(int current)
    {
        if (slider == null) return;
        slider.value = current;
    }
}
