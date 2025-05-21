using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator characterAnimation;

    private void Awake()
    {
        characterAnimation = GetComponent<Animator>();
    }

    public void SetDirection(Vector2 input)
    {
        Vector2 direction = input.normalized;

        characterAnimation.SetFloat("VelocityX", direction.x);
        characterAnimation.SetFloat("VelocityY", direction.y);
    }

    public void PlayRunAnimation()
    {
        characterAnimation.SetBool("isRunning", true);
    }

    public void StopRunAnimation()
    {
        characterAnimation.SetBool("isRunning", false);
    }
}
