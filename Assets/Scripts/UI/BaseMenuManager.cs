using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class BaseMenuManager : MonoBehaviour
{
    private Action onCloseOptions;
    protected const int MAINMENU_INDEX = 0;
    protected const int GAMEPLAY_INDEX = 1;

    public void ShowOptions(Action onClose = null)
    {
        GetOptionsPanel()?.SetActive(true);
        onCloseOptions = onClose;
    }

    public void HideOptions()
    {
        GetOptionsPanel()?.SetActive(false);
    }

    public void CloseOptions()
    {
        SoundManager.Instance.PlayButton_02Sound();
        GetOptionsPanel()?.SetActive(false);

        OnAfterCloseOptions();

        onCloseOptions?.Invoke();
        onCloseOptions = null;
    }

    protected abstract GameObject GetOptionsPanel();
    protected abstract void OnAfterCloseOptions();
}
