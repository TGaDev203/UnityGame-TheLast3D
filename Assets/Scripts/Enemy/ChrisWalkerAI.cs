using System.Collections;
using UnityEngine;

public class ChrisWalkerAI : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        SoundManager.Instance.PlayChrisWalkerVoiceAndChainSound(audioSource);
        enemyAnim.SetVelocity(0.5f);
    }

    protected override void HandleChase()
    {
        base.HandleChase();
        enemyAnim.StopAttack();
        SoundManager.Instance.PlayChrisWalkerChaseSound();
        currentVelocity = Mathf.Lerp(currentVelocity, 1f, Time.deltaTime * 5f);
        enemyAnim.SetVelocity(currentVelocity);
    }

    protected override void HandlePatrol()
    {
        base.HandlePatrol();
        enemyAnim.SetVelocity(0.5f);
        SoundManager.Instance.StopChrisWalkerChaseSound();
    }

    protected override IEnumerator PerformLookAround()
    {
        enemyAnim.SetVelocity(0f);
        enemyAnim.PlayLookAround();
        yield return base.PerformLookAround();
        enemyAnim.StopLookAround();
        enemyAnim.SetVelocity(0.5f);
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer();
        Debug.Log("Chris Walker attacks!");
        enemyAnim.SetVelocity(0f);
        enemyAnim.StopLookAround();
        enemyAnim.PlayAttack();
    }

    protected override IEnumerator WaitForAttackToFinish()
    {
        yield return base.WaitForAttackToFinish();
        enemyAnim.SetVelocity(1f);
    }
}