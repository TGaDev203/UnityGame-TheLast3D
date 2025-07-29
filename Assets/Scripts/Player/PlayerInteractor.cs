using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public enum InteractableType { None, Door, Chest, Item }

    [Header("UI & Interaction")]
    [SerializeField] private Button padlockIcon_Closed;
    [SerializeField] private Button padlockIcon_Opened;
    [SerializeField] private Button handIcon;
    [SerializeField] private Button igniteIcon;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionCheckDistance;
    [SerializeField] private Transform cameraTransform;

    [Header("Bomb Setup")]
    [SerializeField] private GameObject dynamitePrefab;
    [SerializeField] private Transform dynamitePlacePoint;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private GameObject stoneToDestroy;
    [SerializeField] private GameObject endScreen;

    [Header("Runtime State")]
    private bool canToggle = true;
    private bool isPlayerNearby = false;
    private bool hasPlacedDynamite = false;
    private bool hasExploded = false;
    private bool isInDynamiteZone = false;
    private float interactCooldown = 1f;
    private GameObject placedDynamite;
    private InteractableType currentInteractableType = InteractableType.None;
    private ItemPickup detectedItem;
    private ChestController detectedChest;
    private DoorController detectedDoor;
    private PlayerInventory inventory;

    public bool HasPlacedDynamite => hasPlacedDynamite;
    public bool HasExploded => hasExploded;
    public bool IsInDynamiteZone => isInDynamiteZone;

    private void Start()
    {
        var checkpoint = SaveManager.Instance.LoadCheckpoint();
        if (checkpoint == null)
        {
            NotificationManager.Instance.ShowWelcomeMessage();
        }
        else
        {
            NotificationManager.Instance.ShowReturningPlayerMessage();
        }

        LoadCheckpointState();

        inventory = GetComponent<PlayerInventory>();

        if (hasExploded && stoneToDestroy != null)
        {
            Destroy(stoneToDestroy);
        }

        if (hasPlacedDynamite && !hasExploded && dynamitePlacePoint != null)
        {
            placedDynamite = Instantiate(dynamitePrefab, dynamitePlacePoint.position, Quaternion.identity);
        }
    }

    private void Update()
    {
        CheckForInteractables();

        if (isInDynamiteZone && placedDynamite == null &&
            inventory.hasDynamite && inventory.hasLighter && !hasExploded)
        {
            PlaceDynamite();
        }

        UpdateIgniteButtonVisibility();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DynamiteZone"))
        {
            isInDynamiteZone = true;

            if (hasExploded)
            {
                return;
            }

            if (inventory.hasDynamite && inventory.hasLighter && isInDynamiteZone)
            {
                NotificationManager.Instance.ShowReadyToIgniteMessage();
            }
            else
            {
                NotificationManager.Instance.ShowMissingItemWarning();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DynamiteZone"))
        {
            isInDynamiteZone = false;
        }
    }

    private void CheckForInteractables()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Max(interactionCheckDistance)))
        {
            float dist = Vector3.Distance(cameraTransform.position, hit.point);

            DoorController door = hit.collider.GetComponentInParent<DoorController>();
            if (door != null && dist <= interactionCheckDistance)
            {
                detectedDoor = door;
                detectedChest = null;
                detectedItem = null;
                currentInteractableType = InteractableType.Door;
                isPlayerNearby = true;
                ShowInteractionUI();
                return;
            }

            ChestController chest = hit.collider.GetComponentInParent<ChestController>();
            if (chest != null && dist <= interactionCheckDistance)
            {
                detectedChest = chest;
                detectedDoor = null;
                detectedItem = null;
                currentInteractableType = InteractableType.Chest;
                isPlayerNearby = true;
                ShowInteractionUI();
                return;
            }

            ItemPickup item = hit.collider.GetComponentInChildren<ItemPickup>();
            if (item != null && dist <= interactionCheckDistance)
            {
                detectedItem = item;
                detectedDoor = null;
                detectedChest = null;
                currentInteractableType = InteractableType.Item;
                isPlayerNearby = true;
                ShowInteractionUI();
                return;
            }
        }

        isPlayerNearby = false;
        currentInteractableType = InteractableType.None;
        detectedDoor = null;
        detectedChest = null;
        detectedItem = null;
        padlockIcon_Opened?.gameObject.SetActive(false);
        padlockIcon_Closed?.gameObject.SetActive(false);
        handIcon?.gameObject.SetActive(false);
    }

    private void UpdateIgniteButtonVisibility()
    {
        bool show = isInDynamiteZone &&
                    hasPlacedDynamite &&
                    !hasExploded &&
                    inventory.hasDynamite &&
                    inventory.hasLighter;

        igniteIcon?.gameObject.SetActive(show);
    }

    public void TryToggleInteractable()
    {
        if (!isPlayerNearby || !canToggle)
            return;

        switch (currentInteractableType)
        {
            case InteractableType.Door:
                detectedDoor?.ToggleDoor();
                ShowInteractionUI();
                StartCoroutine(InteractCooldownRoutine());
                break;

            case InteractableType.Chest:
                detectedChest?.ToggleChest();
                ShowInteractionUI();
                StartCoroutine(InteractCooldownRoutine());
                break;

            case InteractableType.Item:
                detectedItem?.Pickup();
                StartCoroutine(InteractCooldownRoutine());
                break;
        }
    }

    private IEnumerator InteractCooldownRoutine()
    {
        canToggle = false;
        yield return new WaitForSeconds(interactCooldown);
        canToggle = true;
    }

    private void ShowInteractionUI()
    {
        bool? isOpen = null;

        if (detectedDoor != null)
            isOpen = detectedDoor.IsOpen;
        else if (detectedChest != null)
            isOpen = detectedChest.IsOpen;

        bool showLockIcon = (detectedDoor != null || detectedChest != null);

        if (showLockIcon && isOpen.HasValue)
        {
            padlockIcon_Closed.gameObject.SetActive(!isOpen.Value);
            padlockIcon_Opened.gameObject.SetActive(isOpen.Value);
        }
        else
        {
            padlockIcon_Closed.gameObject.SetActive(false);
            padlockIcon_Opened.gameObject.SetActive(false);
        }

        handIcon.gameObject.SetActive(detectedItem != null);
    }

    public bool NoInteractionIconsActive()
    {
        return !padlockIcon_Closed.gameObject.activeInHierarchy &&
               !padlockIcon_Opened.gameObject.activeInHierarchy &&
               !handIcon.gameObject.activeInHierarchy &&
               !igniteIcon.gameObject.activeInHierarchy;
    }

    private void PlaceDynamite()
    {
        if (hasPlacedDynamite || hasExploded) return;

        placedDynamite = Instantiate(dynamitePrefab, dynamitePlacePoint.position, Quaternion.identity);
        hasPlacedDynamite = true;
    }

    public void IgniteDynamite()
    {
        if (hasExploded || placedDynamite == null || !inventory.hasLighter) return;

        Instantiate(explosionVFX, placedDynamite.transform.position, Quaternion.identity);
        Destroy(placedDynamite);
        placedDynamite = null;
        hasExploded = true;

        if (stoneToDestroy != null)
        {
            SoundManager.Instance.PlayExplosionSound();
            Destroy(stoneToDestroy);
        }

        igniteIcon?.gameObject.SetActive(false);

        SaveCheckpoint();
    }

    public void SetEndScreenActive()
    {
        endScreen.SetActive(true);
        SaveManager.Instance.DeleteCheckpoint();
    }

    public void RestoreStateFromCheckpoint(CheckpointData data)
    {
        hasPlacedDynamite = data.hasPlacedDynamite;
        hasExploded = data.hasExploded;

        inventory = GetComponent<PlayerInventory>();
        inventory.hasKey = data.hasKey;
        inventory.hasDynamite = data.hasDynamite;
        inventory.hasLighter = data.hasLighter;

        transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);

        if (hasExploded && stoneToDestroy != null)
        {
            Destroy(stoneToDestroy);
            igniteIcon?.gameObject.SetActive(false);
        }

        if (hasPlacedDynamite && !hasExploded && placedDynamite == null)
        {
            placedDynamite = Instantiate(dynamitePrefab, dynamitePlacePoint.position, Quaternion.identity);
        }

        ChestController[] allChests = FindObjectsByType<ChestController>(FindObjectsSortMode.None);
        foreach (var chest in allChests)
        {
            ChestSaveData saveData = chest.GetChestSaveData();
            if (saveData == null) continue;

            ChestStateData matchingData = data.chestStates.Find(c => c.chestID == saveData.chestID);
            if (matchingData != null)
            {
                ChestSaveData chestSaveData = new ChestSaveData
                {
                    chestID = matchingData.chestID,
                    isOpen = matchingData.isOpen,
                    wasUnlocked = matchingData.wasUnlocked,
                    padlockRemoved = matchingData.padlockRemoved
                };

                chest.LoadChestSaveData(chestSaveData);
            }
        }
        SoundManager.Instance.StopCloseChestSound();
    }

    public void SaveCheckpoint()
    {
        CheckpointData data = new CheckpointData
        {
            hasKey = inventory.hasKey,
            hasDynamite = inventory.hasDynamite,
            hasLighter = inventory.hasLighter,
            hasPlacedDynamite = hasPlacedDynamite,
            hasExploded = hasExploded,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            playerX = transform.position.x,
            playerY = transform.position.y,
            playerZ = transform.position.z,
            chestStates = new List<ChestStateData>()
        };

        ChestController[] allChests = FindObjectsByType<ChestController>(FindObjectsSortMode.None);
        foreach (var chest in allChests)
        {
            var chestData = chest.GetChestSaveData();

            data.chestStates.Add(new ChestStateData
            {
                chestID = chestData.chestID,
                isOpen = chestData.isOpen,
                wasUnlocked = chestData.wasUnlocked,
                padlockRemoved = chestData.padlockRemoved
            });
        }

        SaveManager.Instance.SaveCheckpoint(data);
    }

    private void LoadCheckpointState()
    {
        CheckpointData data = SaveManager.Instance.LoadCheckpoint();
        if (data == null) return;

        RestoreStateFromCheckpoint(data);
    }
}