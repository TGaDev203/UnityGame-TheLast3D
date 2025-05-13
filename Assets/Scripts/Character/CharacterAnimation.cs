using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator characterAnimation;

    private void Awake()
    {
        characterAnimation = GetComponent<Animator>();
    }

    public void SetVelocity(float velocity)
    {
        characterAnimation.SetFloat("Velocity", velocity);
    }
}