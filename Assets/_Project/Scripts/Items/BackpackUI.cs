using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家背包 UI（Tab 开关）
/// 左半：玩家立绘  右半：物品网格（点击 = 使用）
/// </summary>
public class BackpackUI : MonoBehaviour
{
    [Header("UI Settings")]
    public int       slotSize       = 96;   // 每个格子像素
    public int       slotGap        = 8;    // 格子间距
    public int       columns        = 4;    // 网格列数

    private bool     isOpen;
    private PlayerInventory inv;
    private Texture2D whiteTex;
    private GUIStyle titleStyle, countStyle, hintStyle, useStyle;

    void Awake()
    {
        inv = FindFirstObjectByType<PlayerInventory>();
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    void Update()
    {
        bool prev = isOpen;
        if (Input.GetKeyDown(KeyCode.Tab))
            isOpen = !isOpen;
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            isOpen = false;

        // 切换时控制光标显示
        if (prev != isOpen) SetCursorVisible(isOpen);
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

    public void Refresh() { /* 数据每帧重读 */ }

    void InitStyles()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 32;
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

        useStyle = new GUIStyle();
        useStyle.fontSize = 12;
        useStyle.fontStyle = FontStyle.Bold;
        useStyle.alignment = TextAnchor.MiddleCenter;
        useStyle.normal.textColor = new Color(1, 1, 0.5f, 0.9f);
    }

    void OnGUI()
    {
        if (!isOpen) return;
        if (titleStyle == null) InitStyles();
        if (inv == null) inv = FindFirstObjectByType<PlayerInventory>();
        if (inv == null) return;

        // ── 全屏暗色遮罩 ──
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.75f));

        // ── 主面板 ──
        int panelW = Mathf.Min(1100, Screen.width - 80);
        int panelH = Mathf.Min(640,  Screen.height - 80);
        int panelX = (Screen.width - panelW) / 2;
        int panelY = (Screen.height - panelH) / 2;
        DrawRect(new Rect(panelX, panelY, panelW, panelH), new Color(0.08f, 0.08f, 0.1f, 0.96f));
        DrawBorder(new Rect(panelX, panelY, panelW, panelH), 2, Color.white);

        // ── 标题栏 ──
        GUI.Label(new Rect(panelX, panelY + 15, panelW, 40), "BACKPACK", titleStyle);
        DrawRect(new Rect(panelX + 40, panelY + 60, panelW - 80, 1), new Color(1, 1, 1, 0.3f));

        // ── 左 / 右 分区 ──
        int contentY = panelY + 80;
        int contentH = panelH - 110;
        int leftW    = (int)(panelW * 0.4f);
        int rightW   = panelW - leftW;

        // 左：玩家立绘
        DrawPlayerPanel(new Rect(panelX + 10, contentY, leftW - 20, contentH));

        // 右：物品网格
        DrawItemGrid(new Rect(panelX + leftW, contentY, rightW - 10, contentH));

        // ── 底部提示 ──
        GUI.Label(new Rect(panelX, panelY + panelH - 28, panelW, 20),
            "Click First Aid to heal  |  [4] Quick-use  |  [Tab/Esc] Close", hintStyle);
    }

    void DrawPlayerPanel(Rect r)
    {
        DrawRect(r, new Color(0.05f, 0.05f, 0.07f, 1f));
        DrawBorder(r, 1, new Color(1, 1, 1, 0.25f));

        Texture2D pTex = IconLoader.Get("player");
        if (pTex != null)
        {
            // 保持图片宽高比
            float aspect = (float)pTex.width / pTex.height;
            float maxH = r.height - 40;
            float maxW = r.width  - 40;
            float h    = maxH, w = h * aspect;
            if (w > maxW) { w = maxW; h = w / aspect; }
            float ix = r.x + (r.width  - w) / 2;
            float iy = r.y + (r.height - h) / 2;
            GUI.DrawTexture(new Rect(ix, iy, w, h), pTex, ScaleMode.ScaleToFit);
        }
        else
        {
            // 占位
            GUI.Label(r, "Player\n(put Player.png in Resources/Icons)",
                new GUIStyle { fontSize = 16, alignment = TextAnchor.MiddleCenter,
                               normal = new GUIStyleState { textColor = Color.gray },
                               wordWrap = true });
        }
    }

    void DrawItemGrid(Rect area)
    {
        DrawRect(area, new Color(0.05f, 0.05f, 0.07f, 1f));
        DrawBorder(area, 1, new Color(1, 1, 1, 0.25f));

        // 把背包内容平铺成 List<Item>（每个 Heal 单独一格，Ammo 用一格 + 数量，Key 一格 + 数量）
        var slots = BuildSlotList(inv);

        // 网格起点
        int startX = (int)area.x + 16;
        int startY = (int)area.y + 16;

        // 8 个格子（4 列 × 2 行 起步）
        int cols = columns;
        int rows = Mathf.Max(3, Mathf.CeilToInt((float)slots.Count / cols));

        for (int i = 0; i < rows * cols; i++)
        {
            int col = i % cols;
            int row = i / cols;
            Rect cell = new Rect(
                startX + col * (slotSize + slotGap),
                startY + row * (slotSize + slotGap),
                slotSize, slotSize);

            // 格子背景
            DrawRect(cell, new Color(0.15f, 0.15f, 0.18f, 1f));
            DrawBorder(cell, 1, new Color(1, 1, 1, 0.15f));

            if (i < slots.Count)
            {
                var slot = slots[i];
                DrawItemSlot(cell, slot);

                // Heal 槽位可点击使用
                if (slot.type == ItemType.Heal &&
                    Event.current.type == EventType.MouseDown &&
                    cell.Contains(Event.current.mousePosition))
                {
                    inv.UseHeal();
                    Event.current.Use();
                }
            }
        }
    }

    void DrawItemSlot(Rect cell, SlotData s)
    {
        // 钥匙根据 keyId 加载对应图标
        Texture2D icon = (s.type == ItemType.Key && s.keyId > 0)
            ? IconLoader.Get($"key{Mathf.Clamp(s.keyId, 1, 3)}")
            : IconLoader.ForItem(s.type);
        if (icon != null)
        {
            // 内缩 6 像素
            Rect iconRect = new Rect(cell.x + 6, cell.y + 6, cell.width - 12, cell.height - 12);
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }
        else
        {
            // 占位色块
            Color c = IconLoader.FallbackColor(s.type);
            DrawRect(new Rect(cell.x + 12, cell.y + 12, cell.width - 24, cell.height - 24), c);
        }

        // 数量（右下角，只显示弹药数）
        if (s.type == ItemType.Ammo)
        {
            string txt = $"x{s.count}";
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

        // Heal 槽位下方提示 "USE"
        if (s.type == ItemType.Heal)
        {
            GUI.Label(new Rect(cell.x, cell.y + cell.height - 18, cell.width, 16), "CLICK TO USE", useStyle);
        }
    }

    // ──────────── 数据整理 ────────────
    struct SlotData { public ItemType type; public int count; public int keyId; }

    List<SlotData> BuildSlotList(PlayerInventory inv)
    {
        var list = new List<SlotData>();
        // 每个 Heal 显示一个独立格子
        foreach (var h in inv.healItems)
            list.Add(new SlotData { type = ItemType.Heal, count = h.amount });
        if (inv.ammo > 0)
            list.Add(new SlotData { type = ItemType.Ammo, count = inv.ammo });
        // 每把钥匙单独一格（用各自的 keyId 图标）
        foreach (var k in inv.keys)
            list.Add(new SlotData { type = ItemType.Key, count = 1, keyId = k.keyId });
        return list;
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
