using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioSource backgroundAudioSource;
    public AudioSource soundEffectAudioSource;
    
    [SerializeField] private AudioClip buttonProgressSound;
    [SerializeField] private AudioClip buttonEndSound;
    [SerializeField] private AudioClip beingHitSound;
    [SerializeField] private AudioClip closeDoorSound;
    [SerializeField] private AudioClip closeChestSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip gamePlaySound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip knockDoorSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip mainMenuSound;
    [SerializeField] private AudioClip openDoorSound;
    [SerializeField] private AudioClip openChestSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip[] footStepSounds;
    [SerializeField] private float footstepInterval;

    [Header("Footstep")]
    private float footstepTimer;
    private float nextFootstepTime;

  [Header("Tired Sound")]
    [SerializeField] private AudioClip tiredBreathSound;
    [SerializeField] private float tiredThreshold = 6f;
    [SerializeField] private float tiredCooldown = 3f;
    private float runningStartTime = -1f;
    private float nextTiredSoundTime = 0f;

    public void PlayButtonProgressSound() => PlaySound(buttonProgressSound);
    public void PlayButtonEndSound() => PlaySound(buttonEndSound);
    public void PlayBeingHitSound() => PlaySound(beingHitSound);
    public void PlayCloseChestSound() => PlaySound(closeChestSound);
    public void PlayCloseDoorSound() => PlaySound(closeDoorSound);
    public void PlayDieSound() => PlaySound(dieSound);
    public void PlayJumpSound() => PlaySound(jumpSound);
    public void PlayPickupSound() => PlaySound(pickupSound);
    public void PlayLockedSound() => PlaySound(lockedSound);
    public void PlayLandSound() => PlaySound(landSound);
    public void PlayOpenChestSound() => PlaySound(openChestSound);
    public void PlayOpenDoorSound() => PlaySound(openDoorSound);
    public void PlayTiredBreathSound() => PlaySound(tiredBreathSound);

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

public void PlayFootStepSounds(bool isRunning, bool isTired)
{
    float interval = isRunning ? footstepInterval * 0.6f : footstepInterval;

    if (isRunning)
    {
        // Nếu mệt và đủ thời gian → phát tiếng thở
        if (isTired && Time.time >= nextTiredSoundTime)
        {
            if (tiredBreathSound != null)
            {
                PlayTiredBreathSound();
                nextTiredSoundTime = Time.time + tiredCooldown;
            }
        }
    }

    if (Time.time >= nextFootstepTime && footStepSounds.Length > 0)
    {
        int index = Random.Range(0, footStepSounds.Length);
        soundEffectAudioSource.PlayOneShot(footStepSounds[index]);
        nextFootstepTime = Time.time + interval;
    }
}


    public void PlayBackgroundSound()
    {
        if (backgroundAudioSource == null) return;

        backgroundAudioSource.loop = true;

        string scene = SceneManager.GetActiveScene().name;
        backgroundAudioSource.clip = (scene == "MainMenu_Scene") ? mainMenuSound : gamePlaySound;
        backgroundAudioSource.Play();
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

}