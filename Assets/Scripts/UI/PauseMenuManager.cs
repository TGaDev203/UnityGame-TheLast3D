using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : BaseMenuManager
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    [Header("State Flags")]
    private bool isPaused = false;

    public bool IsPaused() => isPaused;

    protected void Awake()
    {
        pauseMenuPanel?.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        isPaused = true;
        SoundManager.Instance.PlayButton_02Sound();
        SoundManager.Instance.PauseAllSounds();
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
            isPaused = false;
            SoundManager.Instance.PlayButton_01Sound();
            SoundManager.Instance.ResumeAllSounds();
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    protected override GameObject GetOptionsPanel() => optionsMenuPanel;
    protected override void OnAfterCloseOptions() => pauseMenuPanel?.SetActive(true);
}