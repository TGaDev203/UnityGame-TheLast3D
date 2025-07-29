using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource backgroundAudioSource;
    public AudioSource soundEffectAudioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip button_01Sound;
    [SerializeField] private AudioClip button_02Sound;
    [SerializeField] private AudioClip beingHitSound;
    [SerializeField] private AudioClip closeDoorSound;
    [SerializeField] private AudioClip closeChestSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip endSound;
    [SerializeField] private AudioClip gamePlaySound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip knockDoorSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip mainMenuSound;
    [SerializeField] private AudioClip openDoorSound;
    [SerializeField] private AudioClip openChestSound;
    [SerializeField] private AudioClip pickupSound;

    [Header("Footstep")]
    [SerializeField] private AudioClip[] footStepSounds;
    [SerializeField] private float footstepInterval;
    private bool isFootstepSoundMuted = false;
    private bool isJumpSoundMuted = false;
    private float footstepTimer;
    private float nextFootstepTime;


    [Header("Tired Sound")]
    [SerializeField] private AudioClip tiredBreathSound;
    [SerializeField] private float tiredThreshold;
    [SerializeField] private float tiredCooldown;
    private float runningStartTime = -1f;
    private float nextTiredSoundTime = 0f;

    [Header("Internal State")]
    private const int MAINMENU_INDEX = 0;
    private const int GAMEPLAY_INDEX = 1;
    private FirstPersonController firstPersonController;

    public void PlayButton_01Sound() => PlayOneShotSound(button_01Sound);
    public void PlayButton_02Sound() => PlayOneShotSound(button_02Sound);
    public void PlayBeingHitSound() => PlayOneShotSound(beingHitSound);
    public void PlayCloseChestSound() => PlayOneShotSound(closeChestSound);
    public void PlayCloseDoorSound() => PlayOneShotSound(closeDoorSound);
    public void PlayDieSound() => PlayOneShotSound(dieSound);
    public void PlayExplosionSound() => PlayOneShotSound(explosionSound);
    public void PlayLockedSound() => PlayOneShotSound(lockedSound);
    public void PlayLandSound() => PlayOneShotSound(landSound);
    public void PlayOpenChestSound() => PlayOneShotSound(openChestSound);
    public void PlayOpenDoorSound() => PlayOneShotSound(openDoorSound);
    public void PlayPickupSound() => PlayOneShotSound(pickupSound);
    public void PlayTiredBreathSound() => PlayOneShotSound(tiredBreathSound);

    private void Awake()
    {
        if (Instance == null) Instance = this;

        else Destroy(gameObject);

        firstPersonController = FindAnyObjectByType<FirstPersonController>();

        backgroundAudioSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        soundEffectAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == GAMEPLAY_INDEX)
        {
            PlayOneShotSound(knockDoorSound);
        }

        PlayBackgroundSound();
    }

    private void Update()
    {
        if (footstepTimer > 0)
        {
            footstepTimer -= Time.deltaTime;
        }
    }

    public void PlayBackgroundSound()
    {
        if (backgroundAudioSource == null) return;

        backgroundAudioSource.loop = true;

        int scene = SceneManager.GetActiveScene().buildIndex;
        backgroundAudioSource.clip = (scene == MAINMENU_INDEX) ? mainMenuSound : gamePlaySound;
        backgroundAudioSource.Play();
    }

    public void PlayOneShotSound(AudioClip clip)
    {
        if (clip != null)
        {
            soundEffectAudioSource.clip = clip;
            soundEffectAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayFootStepSounds(bool isRunning)
    {
        if (isFootstepSoundMuted && firstPersonController.IsJumping()) return;

        float interval = isRunning ? footstepInterval * 0.6f : footstepInterval;

        if (isRunning)
        {
            if (runningStartTime < 0f)
                runningStartTime = Time.time;

            if (Time.time - runningStartTime >= tiredThreshold && Time.time >= nextTiredSoundTime)
            {
                if (tiredBreathSound != null)
                {
                    PlayTiredBreathSound();
                    nextTiredSoundTime = Time.time + tiredCooldown;
                }
            }
        }
        else
        {
            runningStartTime = -1f;
        }

        if (Time.time >= nextFootstepTime && footStepSounds.Length > 0)
        {
            int index = Random.Range(0, footStepSounds.Length);
            soundEffectAudioSource.PlayOneShot(footStepSounds[index]);
            nextFootstepTime = Time.time + interval;
        }
    }

    public void PlayJumpSound()
    {
        if (isJumpSoundMuted) return;
        isFootstepSoundMuted = true;
        PlayOneShotSound(jumpSound);
    }

    public void PlayChaseSound(AudioClip clip)
    {
        if (clip == null) return;

        if (soundEffectAudioSource.clip == clip && soundEffectAudioSource.isPlaying) return;

        soundEffectAudioSource.loop = true;
        soundEffectAudioSource.clip = clip;
        soundEffectAudioSource.Play();
    }

    public void StopChaseSound(AudioClip clip)
    {
        if (clip == null) return;

        if (soundEffectAudioSource.clip == clip && soundEffectAudioSource.isPlaying)
        {
            soundEffectAudioSource.loop = false;
            soundEffectAudioSource.Stop();
        }
    }

    public void StopCloseChestSound()
    {
        if (openChestSound != null)
        {
            soundEffectAudioSource.clip = openChestSound;
            soundEffectAudioSource.Stop();
        }
    }

    public void PlayVoice(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null) return;

        source.loop = true;
        source.clip = clip;
        source.Play();
    }

    public void PlayEndSound()
    {
        soundEffectAudioSource.Stop();
        if (endSound == null) return;

        backgroundAudioSource.loop = true;
        backgroundAudioSource.clip = endSound;
        backgroundAudioSource.Play();
    }

    public float GetBackgroundMusicVolume()
    {
        return backgroundAudioSource.volume;
    }

    public void SetBackgroundMusicVolume(float value)
    {
        backgroundAudioSource.volume = value;
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    public float GetSFXVolume()
    {
        return soundEffectAudioSource.volume;
    }


    public void SetSFXVolume(float value)
    {
        soundEffectAudioSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXMuted(bool value)
    {
        isFootstepSoundMuted = value;
        isJumpSoundMuted = value;
    }

    public void PlayChaseSound(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;

        if (source.clip == clip && source.isPlaying) return;

        source.loop = true;
        source.clip = clip;
        source.Play();
    }

    public void StopChaseSound(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;

        if (source.clip == clip && source.isPlaying)
        {
            source.loop = false;
            source.Stop();
        }
    }
}