using UnityEngine;

public class EnemyAnimationController : MonoBehaviour, IEnemyAnimation
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetVelocity(float velocity) => anim.SetFloat("Velocity", velocity);
    public void PlayAttack() => anim.SetBool("isAttacking", true);
    public void StopAttack() => anim.SetBool("isAttacking", false);
    public void PerformPauseAction(bool value) => anim.SetBool("isPerforming", value);
}