using UnityEngine;

public class PauseButtonManager : ButtonManagerBase
{
    private enum PauseButton
    {
        Option = 0,
        Main = 1
    }

    private void Start()
    {
        InitializeGameSettings();
    }

    private void Update()
    {
        HandlePauseInput();
    }

    protected override void HandlePauseInput()
    {
        // if (playerController != null && playerController.IsDead()) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (pauseMenu.activeSelf)
        {
            Resume();
            Time.timeScale = 1f;
        }

        else if (optionMenu.activeSelf)
        {
            pauseMenu.SetActive(true);
            optionMenu.SetActive(false);
        }

        else
        {
            Time.timeScale = 0f;
            Pause();
        }
    }

    protected override void OnButtonClicked(int index)
    {
        switch (index)
        {
            case (int)PauseButton.Option:
                OptionMenu();
                break;

            case (int)PauseButton.Main:
                // playerController.SavePlayerData();
                LoadMainScene();
                break;

            default:
                Debug.LogWarning("Unknown button index: " + index);
                break;
        }
    }

    protected override void OptionMenu()
    {
        SoundManager.Instance.PlayButtonEndSound();
        pauseMenu.SetActive(false);
        optionMenu.SetActive(true);
    }
}