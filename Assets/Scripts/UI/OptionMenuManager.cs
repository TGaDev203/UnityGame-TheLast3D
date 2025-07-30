using UnityEngine;
using UnityEngine.UI;

public class OptionMenuManager : MonoBehaviour
{
    [Header("Audio Slider")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider enemySoundSlider;

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

        if (enemySoundSlider != null)
        {
            enemySoundSlider.value = SoundManager.Instance.GetEnemyVolume();
            enemySoundSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.Instance.SetEnemyVolume(value);
            });
        }
    }
}