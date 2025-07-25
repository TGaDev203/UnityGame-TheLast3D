using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator playerAnimator;

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
    }

    public void SetDirection(Vector2 input)
    {
        Vector2 direction = input.normalized;

        playerAnimator.SetFloat("VelocityX", direction.x);
        playerAnimator.SetFloat("VelocityY", direction.y);
    }

    public void SetSpeedMultiplier(float value)
    {
        playerAnimator.SetFloat("speedMultiplier", value);
    }
    public void SetIsRunning(bool isRunning)
    {
        playerAnimator.SetBool("isRunning", isRunning);
    }

    public void SetJumpType(int type)
    {
        playerAnimator.SetInteger("jumpType", type);
    }
}