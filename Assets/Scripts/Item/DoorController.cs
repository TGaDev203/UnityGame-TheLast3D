using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Transform doorLeaf;
    [SerializeField] private float openAngle;
    [SerializeField] private float openDuration;
    [SerializeField] private bool invertRotation = false;

    private bool isOpen = false;
    private Coroutine rotateRoutine;

    public void ToggleDoor()
    {
        if (CompareTag("Locked"))
        {
            SoundManager.Instance.PlayLockedSound();
            return;
        }

        isOpen = !isOpen;

        float angle = isOpen ? openAngle : 0f;
        if (invertRotation) angle *= -1;

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateDoor(angle, openDuration));

        if (isOpen)
            SoundManager.Instance?.PlayOpenDoorSound();
        else
            SoundManager.Instance?.PlayCloseDoorSound();
    }

    private IEnumerator RotateDoor(float angle, float duration)
    {
        Quaternion start = doorLeaf.localRotation;
        Quaternion end = Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            doorLeaf.localRotation = Quaternion.Slerp(start, end, elapsed / duration);
            yield return null;
        }

        doorLeaf.localRotation = end;
    }

    public bool IsOpen => isOpen;
}
