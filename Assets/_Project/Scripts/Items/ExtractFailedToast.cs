using UnityEngine;

/// <summary>
/// 屏幕中下方红色失败提示，3 秒后淡出
/// </summary>
public class ExtractFailedToast : MonoBehaviour
{
    private static ExtractFailedToast _instance;
    private string  message = "";
    private float   showUntil = 0f;
    private float   duration  = 3f;

    private Texture2D whiteTex;
    private GUIStyle  style;

    public static void Show(string msg)
    {
        if (_instance == null)
        {
            var go = new GameObject("[ExtractFailedToast]");
            _instance = go.AddComponent<ExtractFailedToast>();
            DontDestroyOnLoad(go);
        }
        _instance.message   = msg;
        _instance.showUntil = Time.unscaledTime + _instance.duration;
    }

    void Awake()
    {
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    void InitStyle()
    {
        style = new GUIStyle();
        style.fontSize = 26;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        if (Time.unscaledTime > showUntil) return;
        if (string.IsNullOrEmpty(message)) return;
        if (style == null) InitStyle();

        // 淡出效果
        float remain = showUntil - Time.unscaledTime;
        float alpha  = Mathf.Clamp01(remain / 0.5f); // 最后 0.5 秒淡出

        int w = 500, h = 70;
        int x = (Screen.width - w) / 2;
        int y = Screen.height - h - 80;

        Color bg = new Color(0.85f, 0.15f, 0.15f, 0.92f * alpha);
        DrawRect(new Rect(x, y, w, h), bg);
        DrawRect(new Rect(x, y, w, 3), new Color(1, 1, 1, alpha));
        DrawRect(new Rect(x, y + h - 3, w, 3), new Color(1, 1, 1, alpha));

        Color textCol = Color.white;
        textCol.a = alpha;
        var s2 = new GUIStyle(style);
        s2.normal.textColor = textCol;
        GUI.Label(new Rect(x, y, w, h), message, s2);
    }

    void DrawRect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, whiteTex);
        GUI.color = prev;
    }
}
