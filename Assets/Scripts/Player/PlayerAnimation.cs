using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator playerAnimation;

    private void Awake()
    {
        playerAnimation = GetComponent<Animator>();
    }

    public void SetDirection(Vector2 input)
    {
        Vector2 direction = input.normalized;

        playerAnimation.SetFloat("VelocityX", direction.x);
        playerAnimation.SetFloat("VelocityY", direction.y);
    }

    public void PlayRunAnimation()
    {
        playerAnimation.SetBool("isRunning", true);
    }

    public void StopRunAnimation()
    {
        playerAnimation.SetBool("isRunning", false);
    }

    public void SetSpeedMultiplier(float value)
    {
        playerAnimation.SetFloat("speedMultiplier", value);
    }
    public void SetIsRunning(bool isRunning)
    {
        playerAnimation.SetBool("isRunning", isRunning);
    }
}