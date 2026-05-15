using UnityEngine;

/// <summary>
/// 让光圈呼吸 / 自旋
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class SearchRingPulse : MonoBehaviour
{
    public float pulseSpeed   = 2f;    // 呼吸频率
    public float minAlpha     = 0.35f;
    public float maxAlpha     = 0.85f;
    public float rotateSpeed  = 30f;   // 自转角度/秒
    public bool  scalePulse   = true;
    public float scaleAmount  = 0.05f; // 脉动 ±5%

    private Material  mat;
    private Color     baseColor;
    private Vector3   baseScale;

    void Start()
    {
        var mr = GetComponent<MeshRenderer>();
        // 用实例材质避免修改共享材质
        mat = mr.material;
        if (mat.HasProperty("_BaseColor"))
            baseColor = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color"))
            baseColor = mat.GetColor("_Color");
        else
            baseColor = Color.yellow;

        baseScale = transform.localScale;
    }

    void Update()
    {
        // 呼吸 alpha
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        Color c = baseColor;
        c.a = alpha;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);

        // 自转
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // 脉动缩放
        if (scalePulse)
        {
            float s = 1f + (t * 2f - 1f) * scaleAmount;
            transform.localScale = new Vector3(baseScale.x * s, baseScale.y, baseScale.z * s);
        }
    }
}
