using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator characterAnimator;

    private void Awake()
    {
        characterAnimator = GetComponent<Animator>();
    }

    public void PlayWalkingAnimation()
    {
        characterAnimator.SetBool("isWalking", true);
    }

    public void StopWalkingAmimation()
    {
        characterAnimator.SetBool("isWalking", false);
    }
}
