using UnityEngine;
using UnityEngine.UI;

public class OptionMenuManager : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = SoundManager.Instance.GetBackgroundMusicVolume();
            bgmSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.Instance.SetBackgroundMusicVolume(value);
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.Instance.SetSFXVolume(value);
            });
        }
    }
}