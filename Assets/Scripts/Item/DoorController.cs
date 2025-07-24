using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorLeaf;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private float openDuration = 1f;
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

        float targetAngle = isOpen ? openAngle : closeAngle;
        if (invertRotation) targetAngle *= -1;

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateDoor(targetAngle, openDuration));

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
