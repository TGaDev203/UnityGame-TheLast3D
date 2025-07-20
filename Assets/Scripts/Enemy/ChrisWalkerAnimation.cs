using UnityEngine;

public class ChrisWalkerAnimation : MonoBehaviour, IEnemyAnimation
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetVelocity(float velocity) => anim.SetFloat("Velocity", velocity);
    public void PlayAttack() => anim.SetBool("isAttacking", true);
    public void StopAttack() => anim.SetBool("isAttacking", false);
    public void PlayLookAround() => anim.SetBool("isLooking", true);
    public void StopLookAround() => anim.SetBool("isLooking", false);
}
