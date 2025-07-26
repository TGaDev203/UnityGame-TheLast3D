using UnityEngine;

public class MainButtonManager : ButtonManagerBase
{
    public enum MainMenuButtonType
    {
        Continue = 0,
        NewGame = 1,
        Option = 2,
        Quit = 3
    }

    private void Start()
    {
        SetMouseOn();
        InitializeGameSettings();
        // SetButtonActive(0, SaveManager.SaveExists());
    }

    private void Update()
    {
        HandlePauseInput();
    }

    protected override void HandlePauseInput()
    {
        // if (player != null && player.IsDead()) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (optionMenu.activeSelf)
        {
            BackToMainMenu();
        }
    }

    protected override void OnButtonClicked(int index)
    {
        if (isButtonClicked) return;

        isButtonClicked = true;
        Invoke(nameof(ResetButtonClick), 0.5f);

        if (System.Enum.IsDefined(typeof(MainMenuButtonType), index))
        {
            MainMenuButtonType buttonType = (MainMenuButtonType)index;
            HandleMainMenuButton(buttonType);
        }
        else
        {
            Debug.LogWarning($"Invalid button index: {index}");
        }
    }

    private void HandleMainMenuButton(MainMenuButtonType buttonType)
    {

        switch (buttonType)
        {
            case MainMenuButtonType.Continue:
                ContinueGame();
                break;

            case MainMenuButtonType.NewGame:
                StartNewGame();
                break;

            case MainMenuButtonType.Option:
                SoundManager.Instance.PlayButtonEndSound();
                OptionMenu();
                break;

            case MainMenuButtonType.Quit:
                QuitGame();
                break;

            default:
                Debug.LogWarning("Unhandled button type: " + buttonType);
                break;
        }
    }

    protected override void OptionMenu()
    {
        SoundManager.Instance.PlayButtonEndSound();
        mainMenu.SetActive(false);
        optionMenu.SetActive(true);
    }
}