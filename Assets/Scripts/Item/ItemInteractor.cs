using UnityEngine;

public class ItemInteractor : MonoBehaviour
{
    public enum ItemType { None, Lighter, Dynamite, Key }

    [Header("Item Type")]
    public ItemType itemType;

    public static bool hasLighter = false;
    public static bool hasDynamite = false;
    private static bool hasKey = false;

    public static bool PlayerHasKey() => hasKey;
    public static bool PlayerHasDynamite() => hasDynamite;
    public static bool PlayerHasLighter() => hasLighter;

    private void Start()
    {
        SaveManager.Instance.ClearAllData();

        // LoadItemStates();
    }

    public void Pickup()
    {
        if (itemType == ItemType.None) return;

        switch (itemType)
        {
            case ItemType.Lighter:
                hasLighter = true;
                SaveManager.Instance.SaveBool("HasLighter", true);
                Debug.Log("Picked up Lighter");
                break;

            case ItemType.Dynamite:
                hasDynamite = true;
                SaveManager.Instance.SaveBool("HasDynamite", true);
                Debug.Log("Picked up Dynamite");
                break;

            case ItemType.Key:
                hasKey = true;
                SaveManager.Instance.SaveBool("HasKey", true);
                Debug.Log("Picked up Key");
                break;
        }

        Destroy(gameObject);
    }

    private void LoadItemStates()
    {
        hasLighter = SaveManager.Instance.LoadBool("HasLighter");
        hasDynamite = SaveManager.Instance.LoadBool("HasDynamite");
        hasKey = SaveManager.Instance.LoadBool("HasKey");
    }
}