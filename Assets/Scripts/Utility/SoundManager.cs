using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClip[] footStepSounds;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioSource soundEffectAudioSource;

    public void PlayFootStepSounds()
    {
        if (footStepSounds != null && footStepSounds.Length > 0)
        {
            int index = Random.Range(0, footStepSounds.Length);
            PlaySound(footStepSounds[index]);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            soundEffectAudioSource.PlayOneShot(clip);
        }
    }
}