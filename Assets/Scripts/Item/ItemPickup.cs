using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Key, Dynamite, Lighter }
    public ItemType itemType;

    [Header("References")]
    private PlayerInventory inventory;
    private PlayerInteractor interactor;

    [Header("Runtime States")]
    private Vector3 originalPosition;
    private bool shouldHideDynamite = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        inventory = player?.GetComponent<PlayerInventory>();
        interactor = player?.GetComponent<PlayerInteractor>();

        if (inventory == null) return;

        originalPosition = transform.position;

        switch (itemType)
        {
            case ItemType.Key:
                if (inventory.hasKey) gameObject.SetActive(false);
                break;
            case ItemType.Lighter:
                if (inventory.hasLighter) gameObject.SetActive(false);
                break;
            case ItemType.Dynamite:
                if (inventory.hasDynamite)
                {
                    shouldHideDynamite = !IsPlayerInDynamiteZone();
                    UpdateDynamiteVisibility();
                }
                break;
        }
    }

    private void Update()
    {
        if (itemType == ItemType.Dynamite && inventory != null && inventory.hasDynamite)
        {
            bool inZone = IsPlayerInDynamiteZone();
            if (inZone != !shouldHideDynamite)
            {
                shouldHideDynamite = !inZone;
                UpdateDynamiteVisibility();
            }
        }
    }

    private void UpdateDynamiteVisibility()
    {
        if (shouldHideDynamite)
            transform.position = new Vector3(9999, 9999, 9999);
        else
            transform.position = originalPosition;
    }

    private bool IsPlayerInDynamiteZone()
    {
        return interactor != null && interactor.IsInDynamiteZone;
    }

    public void Pickup()
    {
        if (inventory == null) return;

        switch (itemType)
        {
            case ItemType.Key:
                inventory.hasKey = true;
                SoundManager.Instance.PlayPickupSound();
                NotificationManager.Instance.ShowPickedUpKey();
                break;
            case ItemType.Dynamite:
                inventory.hasDynamite = true;
                SoundManager.Instance.PlayPickupSound();
                NotificationManager.Instance.ShowPickedUpDynamite();
                break;
            case ItemType.Lighter:
                inventory.hasLighter = true;
                SoundManager.Instance.PlayPickupSound();
                NotificationManager.Instance.ShowPickedUpLighter();
                break;
        }

        interactor?.SaveCheckpoint();
        Destroy(gameObject);
    }

    public static bool PlayerHasKey()
    {
        var inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        return inventory != null && inventory.hasKey;
    }
}