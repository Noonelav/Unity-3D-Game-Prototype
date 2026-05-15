using UnityEngine;

/// <summary>
/// 容器搜索面板 - 格子布局，点击格子直接拾取到背包
/// </summary>
public class SearchUI : MonoBehaviour
{
    [Header("UI Settings")]
    public int  slotSize = 96;
    public int  slotGap  = 8;
    public int  columns  = 4;
    public int  rows     = 4;

    private Searchable  current;
    private GameObject  player;
    private bool        isOpen;
    private Texture2D   whiteTex;
    private GUIStyle    titleStyle, countStyle, hintStyle;

    void Awake()
    {
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    public void Open(Searchable s, GameObject p)
    {
        current = s;
        player  = p;
        isOpen  = true;
        SetCursorVisible(true);
    }

    public void Close()
    {
        current = null;
        player  = null;
        isOpen  = false;
        SetCursorVisible(false);
    }

    void SetCursorVisible(bool visible)
    {
        Cursor.visible   = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Confined;
    }

    void OnDisable()
    {
        if (isOpen) SetCursorVisible(false);
    }

    void Update()
    {
        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)))
            Close();
    }

    void InitStyles()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 28;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;

        countStyle = new GUIStyle();
        countStyle.fontSize = 18;
        countStyle.fontStyle = FontStyle.Bold;
        countStyle.alignment = TextAnchor.LowerRight;
        countStyle.normal.textColor = Color.white;

        hintStyle = new GUIStyle();
        hintStyle.fontSize = 14;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(1, 1, 1, 0.6f);
    }

    void OnGUI()
    {
        if (!isOpen || current == null) return;
        if (titleStyle == null) InitStyles();

        // 全屏暗色遮罩
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.65f));

        // 容器自定义格子数 / 列数（如尸体只显示 6 格）
        int useCols = current.columnsOverride > 0 ? current.columnsOverride : columns;
        int total   = current.slotCountOverride > 0 ? current.slotCountOverride : rows * columns;
        int useRows = Mathf.CeilToInt((float)total / useCols);

        // 面板尺寸
        int panelW = slotSize * useCols + slotGap * (useCols - 1) + 60;
        int panelH = slotSize * useRows + slotGap * (useRows - 1) + 130;
        int panelX = (Screen.width - panelW) / 2;
        int panelY = (Screen.height - panelH) / 2;

        DrawRect(new Rect(panelX, panelY, panelW, panelH), new Color(0.08f, 0.08f, 0.1f, 0.96f));
        DrawBorder(new Rect(panelX, panelY, panelW, panelH), 2, Color.white);

        // 标题
        GUI.Label(new Rect(panelX, panelY + 15, panelW, 36), current.containerName, titleStyle);
        DrawRect(new Rect(panelX + 30, panelY + 56, panelW - 60, 1), new Color(1, 1, 1, 0.3f));

        // 格子网格
        int startX = panelX + 30;
        int startY = panelY + 75;

        for (int i = 0; i < total; i++)
        {
            int col = i % useCols;
            int row = i / useCols;
            Rect cell = new Rect(
                startX + col * (slotSize + slotGap),
                startY + row * (slotSize + slotGap),
                slotSize, slotSize);

            DrawRect(cell, new Color(0.15f, 0.15f, 0.18f, 1f));
            DrawBorder(cell, 1, new Color(1, 1, 1, 0.15f));

            if (i < current.contents.Count)
            {
                var item = current.contents[i];
                DrawItemSlot(cell, item);

                // 直接点击拾取
                if (Event.current.type == EventType.MouseDown &&
                    cell.Contains(Event.current.mousePosition))
                {
                    current.TakeItem(i, player);
                    Event.current.Use();
                    GUIUtility.ExitGUI();
                }
            }
        }

        // 底部提示
        GUI.Label(new Rect(panelX, panelY + panelH - 30, panelW, 22),
            "Click to take  |  [Q] / [Esc] Close", hintStyle);
    }

    void DrawItemSlot(Rect cell, Item item)
    {
        Texture2D icon = IconLoader.ForItem(item);
        if (icon != null)
        {
            Rect iconRect = new Rect(cell.x + 6, cell.y + 6, cell.width - 12, cell.height - 12);
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }
        else
        {
            Color c = IconLoader.FallbackColor(item.type);
            DrawRect(new Rect(cell.x + 12, cell.y + 12, cell.width - 24, cell.height - 24), c);
        }

        // 数量（只在弹药上显示）
        if (item.type == ItemType.Ammo)
        {
            string txt = $"x{item.amount}";
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (dx != 0 || dy != 0)
                    {
                        var shadow = new GUIStyle(countStyle);
                        shadow.normal.textColor = Color.black;
                        GUI.Label(new Rect(cell.x + dx, cell.y + dy, cell.width - 6, cell.height - 4), txt, shadow);
                    }
            GUI.Label(new Rect(cell.x, cell.y, cell.width - 6, cell.height - 4), txt, countStyle);
        }
    }

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
