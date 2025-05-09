using UnityEngine;

public class ChrisWalkerAnimation : MonoBehaviour
{
    private Animator enemyAnimation;

    private void Awake()
    {
        enemyAnimation = GetComponent<Animator>();
    }

    public void PlayRunAnimation()
    {
        enemyAnimation.SetBool("isRunning", true);
    }

    public void StopRunAnimation()
    {
        enemyAnimation.SetBool("isRunning", false);
    }
}
