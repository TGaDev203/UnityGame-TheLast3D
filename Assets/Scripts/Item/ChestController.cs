using System.Collections;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    [SerializeField] private Transform chestLid;
    [SerializeField] private float closeAngle;
    [SerializeField] private float openAngle;
    [SerializeField] private float openDuration;

    private bool isOpen = false;
    private Coroutine rotateRoutine;

    public void ToggleChest()
    {
        if (CompareTag("Locked") && !ItemPickup.PlayerHasKey())
        {
            SoundManager.Instance.PlayLockedSound();
            return;
        }

        else if (CompareTag("Locked") && ItemPickup.PlayerHasKey())
        {
            UnlockChest();
        }

        SetChestOpenState(!isOpen);
    }

    private void SetChestOpenState(bool open)
    {
        isOpen = open;

        float angle = isOpen ? openAngle : closeAngle;

        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);

        rotateRoutine = StartCoroutine(OpenChest(angle, openDuration));

        if (isOpen) SoundManager.Instance?.PlayOpenChestSound();
        else SoundManager.Instance?.PlayCloseChestSound();
    }

    private IEnumerator OpenChest(float angle, float duration)
    {
        Quaternion start = chestLid.localRotation;
        Quaternion end = Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            chestLid.localRotation = Quaternion.Slerp(start, end, elapsed / duration);
            yield return null;
        }

        chestLid.localRotation = end;
    }

    public bool IsOpen => isOpen;

    public void UnlockChest()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Padlock"))
            {
                child.tag = "Untagged";

                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = child.gameObject.AddComponent<Rigidbody>();

                rb.useGravity = true;
                rb.isKinematic = false;

                Destroy(child.gameObject, 2f);
                break;
            }
        }

        tag = "Unlocked";
    }

    public void LoadChestState(bool shouldBeOpen, bool wasUnlocked)
    {
        if (wasUnlocked && CompareTag("Locked"))
        {
            UnlockChest();
        }

        if (isOpen != shouldBeOpen)
        {
            SetChestOpenState(shouldBeOpen);
        }
    }
}