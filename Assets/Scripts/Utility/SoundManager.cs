using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] public AudioSource backgroundAudioSource;
    [SerializeField] public AudioSource soundEffectAudioSource;
    [SerializeField] private AudioClip beingHitSound;
    [SerializeField] private AudioClip closeDoorSound;
    [SerializeField] private AudioClip closeChestSound;
    [SerializeField] private AudioClip gamePlaySound;
    [SerializeField] private AudioClip knockDoorSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip mainMenuSound;
    [SerializeField] private AudioClip openDoorSound;
    [SerializeField] private AudioClip openChestSound;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private AudioClip[] footStepSounds;
    [SerializeField] private float footstepInterval;
    private float footstepTimer;
    private float nextFootstepTime;

    public void PlayOpenDoorSound() => PlaySound(openDoorSound);
    public void PlayCloseDoorSound() => PlaySound(closeDoorSound);
    public void PlayOpenChestSound() => PlaySound(openChestSound);
    public void PlayCloseChestSound() => PlaySound(closeChestSound);
    public void PlayLockedSound() => PlaySound(lockedSound);
    public void PlayBeingHitSound() => PlaySound(beingHitSound);
    public void PlayPlayerDeathSound() => PlaySound(playerDeath);

    private void Awake()
    {
        if (Instance == null) Instance = this;

        else Destroy(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Gameplay_Scene")
        {
            PlaySound(knockDoorSound);
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

    public void PlayFootStepSounds(bool isRunning)
    {
        float interval = isRunning ? footstepInterval * 0.6f : footstepInterval;

        if (Time.time >= nextFootstepTime && footStepSounds.Length > 0)
        {
            int index = Random.Range(0, footStepSounds.Length);
            soundEffectAudioSource.PlayOneShot(footStepSounds[index]);
            nextFootstepTime = Time.time + interval;
        }
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

    public void PlayVoice(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null) return;

        source.loop = true;
        source.clip = clip;
        source.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            soundEffectAudioSource.clip = clip;
            soundEffectAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayBackgroundSound()
    {
        if (backgroundAudioSource == null) return;

        backgroundAudioSource.loop = true;

        string scene = SceneManager.GetActiveScene().name;
        backgroundAudioSource.clip = (scene == "Main_Scene") ? mainMenuSound : gamePlaySound;
        backgroundAudioSource.Play();
    }
}