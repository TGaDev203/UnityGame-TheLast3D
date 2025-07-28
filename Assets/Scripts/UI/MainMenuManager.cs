using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : BaseMenuManager
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    protected void Awake()
    {
        mainMenuPanel?.SetActive(true);
    }

    public void ShowMainMenu()
    {
        HideOptions();
        mainMenuPanel?.SetActive(true);
    }

    public void ShowOptionsFromMain()
    {
        SoundManager.Instance.PlayButton_01Sound();
        mainMenuPanel?.SetActive(false);
        ShowOptions(() =>
        {
            mainMenuPanel?.SetActive(true);
        });
    }

    public void OnClickContinue()
    {
        SoundManager.Instance.PlayButton_01Sound();
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("ContinueFromCheckpoint", 1);

        SceneManager.LoadScene(sceneBuildIndex: GAMEPLAY_INDEX);
    }

    public void OnclickNewGame()
    {
        SoundManager.Instance.PlayButton_01Sound();
                SaveManager.Instance.DeleteCheckpoint();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneBuildIndex: GAMEPLAY_INDEX);
    }

    public void OnclickQuit()
    {
        SoundManager.Instance.PlayButton_01Sound();
        Application.Quit();
    }

    protected override GameObject GetOptionsPanel() => optionsMenuPanel;
    protected override void OnAfterCloseOptions() => mainMenuPanel?.SetActive(true);
}