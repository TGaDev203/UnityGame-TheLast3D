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

    public void SetVelocity(float velocity)
    {
        chrisWalkerAnimation.SetFloat("Velocity", velocity);
    }

    public void PlayLookAroundAnimation()
    {
        chrisWalkerAnimation.SetBool("isLooking", true);
    }

    public void StopLookAroundAnimation()
    {
        chrisWalkerAnimation.SetBool("isLooking", false);
    }

    public void PlayAttackAnimation()
    {
        chrisWalkerAnimation.SetBool("isAttacking", true);
    }

    public void StopAttackAnimation()
    {
        chrisWalkerAnimation.SetBool("isAttacking", false);
    }
}
