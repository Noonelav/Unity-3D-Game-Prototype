using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 准星控制器
/// - 游戏启动后隐藏系统鼠标光标
/// - 在鼠标位置显示自定义十字准星（纯代码生成，无需 Sprite）
/// - 瞄准时准星跟随鼠标，保证始终与角色朝向对齐
/// </summary>
public class CrosshairController : MonoBehaviour
{
    // ─────────────────────── Inspector 参数 ──────────────────────────
    [Header("准星外观")]
    public Color crosshairColor     = Color.white;
    public float lineLength         = 12f;   // 每条线的长度（像素）
    public float lineThickness      = 2f;    // 线宽（像素）
    public float centerGap          = 5f;    // 中心留空大小（像素）
    public bool  showCenterDot      = true;  // 是否显示中心点

    [Header("动态效果")]
    public bool  scaleOnShoot       = true;  // 射击时准星扩散
    public float shootExpandAmount  = 6f;    // 扩散像素
    public float expandRecoverSpeed = 8f;    // 恢复速度

    // ─────────────────────── 内部变量 ────────────────────────────────
    private RectTransform crosshairRoot;
    private RectTransform lineTop, lineBottom, lineLeft, lineRight;
    private float         currentExpand = 0f;

    // ─────────────────────── 初始化 ──────────────────────────────────
    void Awake()
    {
        HideCursor();
        BuildCrosshair();
    }

    // ─────────────────────── 每帧更新 ────────────────────────────────
    void Update()
    {
        // 如果光标可见（背包/搜索面板打开），隐藏准星 Canvas
        bool uiOpen = Cursor.visible;
        if (crosshairRoot != null && crosshairRoot.parent != null)
        {
            crosshairRoot.parent.gameObject.SetActive(!uiOpen);
            if (uiOpen) return;
        }

        // 准星跟随鼠标
        crosshairRoot.position = Input.mousePosition;

        // 射击扩散动画
        if (scaleOnShoot)
        {
            if (Input.GetMouseButtonDown(0))
                currentExpand = shootExpandAmount;
            else
                currentExpand = Mathf.Lerp(currentExpand, 0f, expandRecoverSpeed * Time.deltaTime);

            UpdateLinePositions();
        }
    }

    // ─────────────────────── 鼠标隐藏 ────────────────────────────────
    void HideCursor()
    {
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Confined; // 限制在窗口内，不锁定到中心
    }

    void OnDestroy()
    {
        // 游戏结束/退出时恢复鼠标
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) HideCursor();
    }

    // ─────────────────────── 构建准星 UI ─────────────────────────────
    void BuildCrosshair()
    {
        // ── Canvas（Screen Space Overlay，最顶层）──────────────────────
        GameObject canvasGO = new GameObject("[Crosshair Canvas]");
        DontDestroyOnLoad(canvasGO);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        // ── 准星根节点 ─────────────────────────────────────────────────
        GameObject root = new GameObject("CrosshairRoot");
        root.transform.SetParent(canvasGO.transform, false);
        crosshairRoot = root.AddComponent<RectTransform>();
        crosshairRoot.sizeDelta = Vector2.zero;

        // ── 四条线 ─────────────────────────────────────────────────────
        lineTop    = CreateLine(root.transform, "Top");
        lineBottom = CreateLine(root.transform, "Bottom");
        lineLeft   = CreateLine(root.transform, "Left");
        lineRight  = CreateLine(root.transform, "Right");

        // ── 中心点 ─────────────────────────────────────────────────────
        if (showCenterDot)
        {
            RectTransform dot = CreateLine(root.transform, "CenterDot");
            dot.sizeDelta        = new Vector2(lineThickness, lineThickness);
            dot.anchoredPosition = Vector2.zero;
        }

        UpdateLinePositions();
    }

    RectTransform CreateLine(Transform parent, string lineName)
    {
        GameObject go = new GameObject(lineName);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color          = crosshairColor;
        img.raycastTarget  = false;

        return go.GetComponent<RectTransform>();
    }

    /// <summary>根据 currentExpand 更新四条线的位置和尺寸</summary>
    void UpdateLinePositions()
    {
        float gap = centerGap + currentExpand;

        // 上
        lineTop.sizeDelta        = new Vector2(lineThickness, lineLength);
        lineTop.anchoredPosition = new Vector2(0f, gap + lineLength * 0.5f);

        // 下
        lineBottom.sizeDelta        = new Vector2(lineThickness, lineLength);
        lineBottom.anchoredPosition = new Vector2(0f, -(gap + lineLength * 0.5f));

        // 左
        lineLeft.sizeDelta        = new Vector2(lineLength, lineThickness);
        lineLeft.anchoredPosition = new Vector2(-(gap + lineLength * 0.5f), 0f);

        // 右
        lineRight.sizeDelta        = new Vector2(lineLength, lineThickness);
        lineRight.anchoredPosition = new Vector2(gap + lineLength * 0.5f, 0f);
    }
}
