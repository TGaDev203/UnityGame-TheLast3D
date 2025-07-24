public interface IEnemyAnimation
{
    void SetVelocity(float velocity);
    void PlayAttack();
    void StopAttack();
    void PerformPauseAction(bool value);
}