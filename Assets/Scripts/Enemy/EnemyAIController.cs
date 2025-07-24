using System.Collections;
using UnityEngine;

public class EnemyAIController : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        TryPlayVoice();
        enemyAnim.SetVelocity(0.5f);
    }

    protected override void HandleChase()
    {
        base.HandleChase();
        enemyAnim.StopAttack();
        TryPlayChaseSound();
        currentVelocity = Mathf.Lerp(currentVelocity, 1f, Time.deltaTime * 5f);
        enemyAnim.SetVelocity(currentVelocity);
    }

    protected override void HandlePatrol()
    {
        base.HandlePatrol();
        enemyAnim.SetVelocity(0.5f);
        TryStopChaseSound();
    }

    protected override IEnumerator PerformLookAround()
    {
        enemyAnim.SetVelocity(0f);
        enemyAnim.PerformPauseAction(true);
        yield return base.PerformLookAround();
        enemyAnim.PerformPauseAction(false);
        enemyAnim.SetVelocity(0.5f);
    }

    protected override void AttackPlayer()
    {
        base.AttackPlayer();
        Debug.Log("Chris Walker attacks!");
        enemyAnim.SetVelocity(0f);
        enemyAnim.PerformPauseAction(false);
        enemyAnim.PlayAttack();
    }

    protected override IEnumerator WaitForAttackToFinish()
    {
        yield return base.WaitForAttackToFinish();
        enemyAnim.SetVelocity(1f);
    }
}