using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Start()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int height = Screen.height;
        int padding = 20;
        int fontSize = height * 4 / 100;

        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS", fps);

        Vector2 textSize = style.CalcSize(new GUIContent(text));

        Rect rect = new Rect(padding, padding, textSize.x + 10, textSize.y + 4);

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.5f); // Black, 50% transparent
        GUI.Box(rect, GUIContent.none);
        GUI.color = previousColor;

        // Draw the text
        GUI.Label(rect, text, style);
    }
}