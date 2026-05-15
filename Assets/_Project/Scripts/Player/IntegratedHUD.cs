using UnityEngine;

/// <summary>
/// 整合 HUD - 左上角显示：头像 + 血条（带数字） + 体力条（无数字）
/// 用 OnGUI 绘制，不依赖场景 UI
/// </summary>
public class IntegratedHUD : MonoBehaviour
{
    [Header("位置 / 尺寸")]
    public int   marginLeft   = 30;
    public int   marginTop    = 30;
    public int   avatarSize   = 110;
    public int   barWidth     = 320;
    public int   barHeight    = 26;
    public int   barGap       = 8;     // 两根条之间的间距
    public int   panelPadding = 12;

    [Header("颜色")]
    public Color panelColor       = new Color(0, 0, 0, 0.65f);
    public Color healthFillColor  = new Color(0.85f, 0.18f, 0.18f, 1f);
    public Color sprintReadyColor = new Color(0.2f, 0.85f, 0.3f, 1f);
    public Color sprintUseColor   = new Color(1f, 0.55f, 0.1f, 1f);
    public Color sprintCDColor    = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color barBgColor       = new Color(0.13f, 0.05f, 0.05f, 1f);

    private PlayerHealth   playerHealth;
    private PlayerMovement playerMovement;
    private Texture2D      whiteTex;
    private GUIStyle       healthTextStyle, shadowStyle;

    void Awake()
    {
        playerHealth   = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    void InitStyles()
    {
        healthTextStyle = new GUIStyle();
        healthTextStyle.fontSize  = 18;
        healthTextStyle.fontStyle = FontStyle.Bold;
        healthTextStyle.alignment = TextAnchor.MiddleCenter;
        healthTextStyle.normal.textColor = Color.white;

        shadowStyle = new GUIStyle(healthTextStyle);
        shadowStyle.normal.textColor = Color.black;
    }

    void OnGUI()
    {
        if (healthTextStyle == null) InitStyles();

        // ── 面板尺寸 ──
        int panelW = avatarSize + barWidth + panelPadding * 3;
        int panelH = avatarSize + panelPadding * 2;
        int panelX = marginLeft;
        int panelY = marginTop;

        DrawRect(new Rect(panelX, panelY, panelW, panelH), panelColor);
        DrawBorder(new Rect(panelX, panelY, panelW, panelH), 2, Color.white);

        // ── 头像 ──
        Rect avatarRect = new Rect(panelX + panelPadding, panelY + panelPadding, avatarSize, avatarSize);
        DrawRect(avatarRect, new Color(0.05f, 0.05f, 0.07f, 1f));
        DrawBorder(avatarRect, 1, new Color(1, 1, 1, 0.3f));
        Texture2D avatar = IconLoader.Get("player");
        if (avatar != null)
            GUI.DrawTexture(avatarRect, avatar, ScaleMode.ScaleToFit);

        // ── 血条 ──
        int barX = panelX + panelPadding * 2 + avatarSize;
        int healthBarY = panelY + panelPadding + 18;
        DrawHealthBar(barX, healthBarY);

        // ── 体力条 ──
        int sprintBarY = healthBarY + barHeight + barGap;
        DrawSprintBar(barX, sprintBarY);
    }

    void DrawHealthBar(int x, int y)
    {
        int cur = playerHealth != null ? playerHealth.currentHealth : 100;
        int max = playerHealth != null ? playerHealth.maxHealth     : 100;
        float pct = max > 0 ? (float)cur / max : 0f;

        // 标签 HP
        GUI.Label(new Rect(x, y - 18, 40, 16), "HP", new GUIStyle {
            fontSize = 12, fontStyle = FontStyle.Bold,
            normal = new GUIStyleState { textColor = new Color(1, 0.7f, 0.7f) }
        });

        // 背景
        DrawRect(new Rect(x, y, barWidth, barHeight), barBgColor);
        DrawBorder(new Rect(x, y, barWidth, barHeight), 1, new Color(1, 1, 1, 0.4f));

        // 填充
        DrawRect(new Rect(x + 2, y + 2, (barWidth - 4) * Mathf.Clamp01(pct), barHeight - 4),
                 healthFillColor);

        // 数字（描边）
        string txt = $"{cur} / {max}";
        Rect textRect = new Rect(x, y, barWidth, barHeight);
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (dx != 0 || dy != 0)
                    GUI.Label(new Rect(textRect.x + dx, textRect.y + dy, textRect.width, textRect.height),
                              txt, shadowStyle);
        GUI.Label(textRect, txt, healthTextStyle);
    }

    void DrawSprintBar(int x, int y)
    {
        float pct;
        Color fillColor;
        string label;
        if (playerMovement == null)
        {
            pct = 1f;
            fillColor = sprintReadyColor;
            label = "STAMINA";
        }
        else if (playerMovement.OnCooldown)
        {
            pct = 1f - playerMovement.CooldownRemaining / playerMovement.sprintCooldown;
            fillColor = sprintCDColor;
            label = "EXHAUSTED";
        }
        else
        {
            pct = playerMovement.SprintRemaining / Mathf.Max(0.01f, playerMovement.SprintMax);
            fillColor = playerMovement.IsSprinting ? sprintUseColor : sprintReadyColor;
            label = playerMovement.IsSprinting ? "SPRINT" : "STAMINA";
        }

        // 标签
        GUI.Label(new Rect(x, y - 18, 100, 16), label, new GUIStyle {
            fontSize = 12, fontStyle = FontStyle.Bold,
            normal = new GUIStyleState { textColor = new Color(0.7f, 1, 0.7f) }
        });

        // 背景
        DrawRect(new Rect(x, y, barWidth, barHeight), new Color(0.05f, 0.13f, 0.05f, 1f));
        DrawBorder(new Rect(x, y, barWidth, barHeight), 1, new Color(1, 1, 1, 0.4f));

        // 填充（按比例缩短，不显示数字）
        DrawRect(new Rect(x + 2, y + 2, (barWidth - 4) * Mathf.Clamp01(pct), barHeight - 4),
                 fillColor);
    }

    // ──────────── 绘制工具 ────────────
    void DrawRect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, whiteTex);
        GUI.color = prev;
    }

    void DrawBorder(Rect r, int thickness, Color c)
    {
        DrawRect(new Rect(r.x, r.y, r.width, thickness), c);
        DrawRect(new Rect(r.x, r.y + r.height - thickness, r.width, thickness), c);
        DrawRect(new Rect(r.x, r.y, thickness, r.height), c);
        DrawRect(new Rect(r.x + r.width - thickness, r.y, thickness, r.height), c);
    }
}
