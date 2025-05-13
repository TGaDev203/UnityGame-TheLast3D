using UnityEngine;

public class ChrisWalkerAnimation : MonoBehaviour
{
    private Animator chrisWalkerAnimation;

    private void Awake()
    {
        chrisWalkerAnimation = GetComponent<Animator>();
    }

    public void PlayRunAnimation()
    {
        chrisWalkerAnimation.SetBool("isRunning", true);
    }

    public void StopRunAnimation()
    {
        chrisWalkerAnimation.SetBool("isRunning", false);
    }

    public void SetVelocity(float velocity)
    {
        chrisWalkerAnimation.SetFloat("Velocity", velocity);
    }
}
