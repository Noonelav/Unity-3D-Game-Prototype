using UnityEngine;

/// <summary>
/// 胜利结算画面 - 全屏 IMGUI 弹窗
/// 由 ExtractionGate 在玩家成功撤离时调用 Show()
/// </summary>
public class VictoryUI : MonoBehaviour
{
    private bool      shown = false;
    private Texture2D whiteTex;
    private GUIStyle  titleStyle, subtitleStyle, btnStyle;

    void Awake()
    {
        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    public void Show()
    {
        shown = true;
        Time.timeScale   = 0f;   // 暂停游戏
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void InitStyles()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 72;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        subtitleStyle = new GUIStyle();
        subtitleStyle.fontSize = 28;
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.normal.textColor = Color.white;

        btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize  = 22;
        btnStyle.fontStyle = FontStyle.Bold;
    }

    void OnGUI()
    {
        if (!shown) return;
        if (titleStyle == null) InitStyles();

        // 全屏暗色遮罩
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.85f));

        // 标题
        GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 120),
            "VICTORY", titleStyle);

        // 副标题
        GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 60),
            "Successfully Extracted from the Island!", subtitleStyle);

        // Restart 按钮
        float bw = 220, bh = 60;
        Rect btnRect = new Rect((Screen.width - bw) / 2, Screen.height * 0.6f, bw, bh);
        if (GUI.Button(btnRect, "Restart", btnStyle))
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // Quit 按钮
        Rect quitRect = new Rect((Screen.width - bw) / 2, Screen.height * 0.6f + bh + 20, bw, bh);
        if (GUI.Button(quitRect, "Quit", btnStyle))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    void DrawRect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, whiteTex);
        GUI.color = prev;
    }
}
