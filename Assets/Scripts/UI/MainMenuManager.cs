using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : BaseMenuManager
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    [Header("UI Elements")]
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject warningPopup;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject title;

    [Header("Callbacks")]
    private Action onConfirmNewGame;

    protected void Awake()
    {
        mainMenuPanel?.SetActive(true);

        mainMenuPanel?.SetActive(true);

        if (SaveManager.HasCheckpoint())
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
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

        if (SaveManager.HasCheckpoint())
        {
            ShowNewGameDataLossWarning(() =>
            {
                StartNewGame();
            });
        }
        else
        {
            StartNewGame();
        }
    }

    public void OnClickConfirmNewGame()
    {
        SoundManager.Instance.PlayButton_01Sound();
        warningPopup.SetActive(false);

        if (onConfirmNewGame != null)
        {
            onConfirmNewGame.Invoke();
            onConfirmNewGame = null;
        }

    }

    public void OnClickCancelNewGame()
    {
        SoundManager.Instance.PlayButton_02Sound();
        buttons.SetActive(true);
        title.SetActive(true);
        warningPopup.SetActive(false);
        onConfirmNewGame = null;
    }

    private void StartNewGame()
    {
        SaveManager.Instance.DeleteCheckpoint();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneBuildIndex: GAMEPLAY_INDEX);
    }

    public void OnclickQuit()
    {
        SoundManager.Instance.PlayButton_01Sound();
        Application.Quit();
    }

    public void ShowNewGameDataLossWarning(Action onConfirm)
    {
        onConfirmNewGame = onConfirm;

        buttons.SetActive(false);
        title.SetActive(false);
        warningPopup.SetActive(true);
    }

    protected override GameObject GetOptionsPanel() => optionsMenuPanel;
    protected override void OnAfterCloseOptions() => mainMenuPanel?.SetActive(true);
}