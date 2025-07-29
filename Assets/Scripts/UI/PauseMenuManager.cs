using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : BaseMenuManager
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    protected void Awake()
    {
        pauseMenuPanel?.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        SoundManager.Instance.PlayButton_02Sound();
        SoundManager.Instance.SetSFXMuted(true);
        Time.timeScale = 0f;
        HideOptions();
        pauseMenuPanel?.SetActive(true);
    }

    public void ShowOptionsFromPause()
    {

        SoundManager.Instance.PlayButton_01Sound();
        pauseMenuPanel?.SetActive(false);
        ShowOptions(() =>
        {
            pauseMenuPanel?.SetActive(true);
        });
    }

    public void BackToMainMenu()
    {
        SoundManager.Instance.PlayButton_01Sound();
        SceneManager.LoadScene(sceneBuildIndex: MAINMENU_INDEX);
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel.activeSelf)
        {
            SoundManager.Instance.PlayButton_01Sound();
            SoundManager.Instance.SetSFXMuted(false);
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    protected override GameObject GetOptionsPanel() => optionsMenuPanel;
    protected override void OnAfterCloseOptions() => pauseMenuPanel?.SetActive(true);
}