using System.Collections;
using UnityEngine;

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpen;
    public bool wasUnlocked;
    public bool padlockRemoved;
}

public class ChestController : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private Transform chestLid;
    [SerializeField] private float closeAngle;
    [SerializeField] private float openAngle;
    [SerializeField] private float openDuration;
    [SerializeField] private string chestID;

    [Header("Chest State")]
    private bool isOpen = false;
    private Coroutine rotateRoutine;

    public void ToggleChest()
    {
        if (CompareTag("Locked") && !ItemPickup.PlayerHasKey())
        {
            SoundManager.Instance.PlayLockedSound();
            NotificationManager.Instance.ShowLockedMessage();
            return;
        }

        else if (CompareTag("Locked") && ItemPickup.PlayerHasKey())
        {
            NotificationManager.Instance.ShowUnlockedMessage();
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

    public ChestSaveData GetChestSaveData()
    {
        bool hasPadlock = false;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Padlock"))
            {
                hasPadlock = true;
                break;
            }
        }

        return new ChestSaveData
        {
            chestID = chestID,
            isOpen = isOpen,
            wasUnlocked = CompareTag("Unlocked"),
            padlockRemoved = !hasPadlock
        };
    }

    public void LoadChestSaveData(ChestSaveData data)
    {
        if (data.wasUnlocked && CompareTag("Locked"))
        {
            UnlockChest();
        }

        if (isOpen != data.isOpen)
        {
            SetChestOpenState(data.isOpen);
        }

        if (data.padlockRemoved)
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Padlock"))
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }
}