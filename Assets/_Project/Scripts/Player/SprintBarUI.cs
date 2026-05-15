using UnityEngine;

/// <summary>
/// 屏幕右下角冲刺条 - 自动找 PlayerMovement
/// 绿色 = 可用；橙色 = 冲刺中；红色 = 冷却中
/// </summary>
public class SprintBarUI : MonoBehaviour
{
    public int marginRight  = 30;
    public int marginBottom = 30;
    public int barWidth     = 260;
    public int barHeight    = 18;

    private PlayerMovement player;
    private Texture2D       whiteTex;
    private GUIStyle        labelStyle;

    void Awake()
    {
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    void OnGUI()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMovement>();
            if (player == null) return;
        }
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle();
            labelStyle.fontSize  = 14;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = Color.white;
        }

        int x = Screen.width - barWidth - marginRight;
        int y = Screen.height - barHeight - marginBottom;

        // 背景
        DrawRect(new Rect(x, y, barWidth, barHeight), new Color(0, 0, 0, 0.65f));
        DrawRect(new Rect(x, y, barWidth, 2), Color.white);
        DrawRect(new Rect(x, y + barHeight - 2, barWidth, 2), Color.white);
        DrawRect(new Rect(x, y, 2, barHeight), Color.white);
        DrawRect(new Rect(x + barWidth - 2, y, 2, barHeight), Color.white);

        // 填充
        float pct;
        Color fillColor;
        string label;
        if (player.OnCooldown)
        {
            pct = 1f - player.CooldownRemaining / 15f;   // 15 秒冷却
            fillColor = new Color(0.85f, 0.2f, 0.2f, 1f);
            label = $"COOLDOWN {player.CooldownRemaining:F1}s";
        }
        else
        {
            pct = player.SprintRemaining / Mathf.Max(0.01f, player.SprintMax);
            fillColor = player.IsSprinting
                ? new Color(1f, 0.55f, 0.1f, 1f)        // 橙
                : new Color(0.2f, 0.85f, 0.3f, 1f);     // 绿
            label = $"SPRINT {player.SprintRemaining:F1}s";
        }

        int innerW = barWidth - 6;
        int innerH = barHeight - 6;
        DrawRect(new Rect(x + 3, y + 3, innerW * Mathf.Clamp01(pct), innerH), fillColor);

        // 文字
        GUI.Label(new Rect(x + 10, y - 22, 260, 20), label, labelStyle);
    }

    void DrawRect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, whiteTex);
        GUI.color = prev;
    }
}
