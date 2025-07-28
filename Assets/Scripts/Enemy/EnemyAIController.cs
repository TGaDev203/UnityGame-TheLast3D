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
        SoundManager.Instance.PlayVoice(enemyAudioSource, soundProfile.voiceSound);

        enemyAnim.SetVelocity(0.5f);
    }

    protected override void HandleChase()
    {
        base.HandleChase();
        enemyAnim.StopAttack();
        SoundManager.Instance.PlayChaseSound(enemyAudioSource, soundProfile.chaseSound);
        currentVelocity = Mathf.Lerp(currentVelocity, 1f, Time.deltaTime * 5f);
        enemyAnim.SetVelocity(currentVelocity);
    }

    protected override void GoToNextPatrolPoint()
    {
        base.GoToNextPatrolPoint();
        enemyAnim.SetVelocity(0.5f);
        SoundManager.Instance.PlayVoice(enemyAudioSource, soundProfile.voiceSound);
    }

    protected override IEnumerator PerformPauseAction()
    {
        enemyAnim.SetVelocity(0f);
        enemyAnim.PerformPauseAction(true);
        yield return base.PerformPauseAction();
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