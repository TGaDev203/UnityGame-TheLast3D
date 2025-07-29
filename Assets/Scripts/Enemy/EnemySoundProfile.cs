using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Enemy Sound Profile")]
public class EnemySoundProfile : ScriptableObject
{
    [Header("Core Sounds")]
    public AudioClip chaseSound;
    public AudioClip attackSound;

    [Header("Optional Sounds")]
    public AudioClip voiceSound;
    public AudioClip pauseActionSound;
}