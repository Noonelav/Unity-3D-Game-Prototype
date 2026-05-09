using System.Collections;
using UnityEngine;

/// <summary>
/// 敌人受击视觉反馈：材质闪白
/// 挂在敌人根物体上，会自动找到所有子 Renderer
/// </summary>
public class EnemyHitEffect : MonoBehaviour
{
    [Header("闪白设置")]
    public Color  hitColor    = Color.white;
    public float  flashDuration = 0.1f;  // 闪白持续时间（秒）

    private Renderer[]  renderers;
    private Color[]     originalColors;
    private Coroutine   flashCoroutine;

    void Awake()
    {
        // 收集所有子物体的 Renderer
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            // 只记录第一个材质球的颜色（简单方案）
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
        }
    }

    /// <summary>被 Bullet.cs 调用</summary>
    public void PlayHit()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // 变白
        SetColor(hitColor);
        yield return new WaitForSeconds(flashDuration);

        // 恢复原色
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = originalColors[i];
        }

        flashCoroutine = null;
    }

    void SetColor(Color c)
    {
        foreach (var r in renderers)
        {
            if (r != null && r.material.HasProperty("_Color"))
                r.material.color = c;
        }
    }
}
