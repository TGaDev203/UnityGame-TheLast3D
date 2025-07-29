using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshObstacle navObstacle;
    [SerializeField] private Transform doorLeaf;
    private PlayerInteractor playerInteractor;

    [Header("Door Settings")]
    [SerializeField] private float openAngle;
    [SerializeField] private float closeAngle;
    [SerializeField] private float openDuration;
    [SerializeField] private bool invertRotation = false;

    [Header("Door State")]
    private bool isOpen = false;
    private Coroutine rotateRoutine;

    private void Awake()
    {
        if (doorLeaf != null && navObstacle == null)
            navObstacle = doorLeaf.GetComponent<NavMeshObstacle>();
    }

    private void Start()
    {
        playerInteractor = FindAnyObjectByType<PlayerInteractor>();
    }

    public void ToggleDoor()
    {
        if (CompareTag("MainDoor"))
        {
            Time.timeScale = 0f;
            playerInteractor.SetEndScreenActive();
            SoundManager.Instance.soundEffectAudioSource.Stop();
            SoundManager.Instance.PlayEndSound();
        }

        if (CompareTag("Locked"))
        {
            SoundManager.Instance.PlayLockedSound();
            NotificationManager.Instance.ShowLockedMessage();
            return;
        }

        isOpen = !isOpen;

        float targetAngle = isOpen ? openAngle : closeAngle;
        if (invertRotation) targetAngle *= -1;

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(RotateDoor(targetAngle, openDuration));
        if (navObstacle != null)
            navObstacle.enabled = !isOpen;
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