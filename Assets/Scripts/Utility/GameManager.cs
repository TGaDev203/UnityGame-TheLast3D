using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Start()
    {
        Application.targetFrameRate = 144;
        QualitySettings.vSyncCount = 0;

        CheckpointData data = SaveManager.Instance.LoadCheckpoint();
        if (data == null) return;

        if (data.sceneName != SceneManager.GetActiveScene().name) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // Debug.LogError("Player not found in scene.");
            return;
        }

        player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.LoadInventoryFromCheckpoint(data);
        }

        // Debug.Log("Checkpoint loaded.");
    }


    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int height = Screen.height;
        int padding = 50;
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
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = previousColor;

        GUI.Label(rect, text, style);
    }
}