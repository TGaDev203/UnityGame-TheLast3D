using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Key, Dynamite, Lighter }
    public ItemType itemType;

    private void Start()
    {
        PlayerInventory inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        switch (itemType)
        {
            case ItemType.Key:
                if (inventory.hasKey) gameObject.SetActive(false);
                break;
            case ItemType.Lighter:
                if (inventory.hasLighter) gameObject.SetActive(false);
                break;
        }
    }

    public void Pickup()
    {
        PlayerInventory inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        switch (itemType)
        {
            case ItemType.Key:
                inventory.hasKey = true;
                break;
            case ItemType.Dynamite:
                inventory.hasDynamite = true;
                break;
            case ItemType.Lighter:
                inventory.hasLighter = true;
                break;
        }

        var interactor = inventory.GetComponent<PlayerInteractor>();
        if (interactor != null)
        {
            interactor.SaveCheckpoint();
        }

        Destroy(gameObject);
    }

    public static bool PlayerHasKey()
    {
        var inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        return inventory != null && inventory.hasKey;
    }
}
