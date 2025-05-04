using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClip[] footStepSounds;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioSource soundEffectAudioSource;
    [SerializeField] private float footstepInterval;
    private float footstepTimer;
    private float nextFootstepTime;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (footstepTimer > 0)
        {
            footstepTimer -= Time.deltaTime;
        }
    }

    public void PlayFootStepSounds()
    {
        if (Time.time >= nextFootstepTime && footStepSounds.Length > 0)
        {
            int index = Random.Range(0, footStepSounds.Length);
            soundEffectAudioSource.PlayOneShot(footStepSounds[index]);
            nextFootstepTime = Time.time + footstepInterval;
        }
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