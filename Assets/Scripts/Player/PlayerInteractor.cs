using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public enum InteractableType { None, Door, Chest, Item }

    [Header("UI & Interaction")]
    [SerializeField] private Button padlockIcon_Closed;
    [SerializeField] private Button padlockIcon_Opened;
    [SerializeField] private Button handIcon;
    [SerializeField] private GameObject fireIcon;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionCheckDistance;
    [SerializeField] private Transform cameraTransform;

    [Header("Bomb Setup")]
    [SerializeField] private GameObject dynamitePrefab;
    [SerializeField] private Transform dynamitePlacePoint;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private GameObject stoneToDestroy;
    [SerializeField] private GameObject endScreen;

    private bool canToggle = true;
    private bool isPlayerNearby = false;
    private bool hasPlacedDynamite = false;
    private bool hasExploded = false;
    private InteractableType currentInteractableType = InteractableType.None;
    private ItemInteractor detectedItem;
    private ChestController detectedChest;
    private DoorController detectedDoor;
    private float interactCooldown = 1f;
    private GameObject placedDynamite;
    private bool isInDynamiteZone = false;

    private void Start()
    {
        ItemInteractor.hasDynamite = true;
        ItemInteractor.hasLighter = true;
    }

    private void Update()
    {
        CheckForInteractables();

        if (isInDynamiteZone && placedDynamite == null &&
            ItemInteractor.PlayerHasDynamite() && ItemInteractor.PlayerHasLighter())
        {
            PlaceDynamite();
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

            ItemInteractor item = hit.collider.GetComponentInChildren<ItemInteractor>();
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

    public void TryToggleInteractable()
    {
        if (!isPlayerNearby || !canToggle)
            return;

        switch (currentInteractableType)
        {
            case InteractableType.Door:
                if (detectedDoor != null)
                {
                    detectedDoor.ToggleDoor();
                    ShowInteractionUI();
                    StartCoroutine(InteractCooldownRoutine());
                }
                break;

            case InteractableType.Chest:
                if (detectedChest != null)
                {
                    detectedChest.ToggleChest();
                    ShowInteractionUI();
                    StartCoroutine(InteractCooldownRoutine());
                }
                break;

            case InteractableType.Item:
                if (detectedItem != null)
                {
                    detectedItem.Pickup();
                    StartCoroutine(InteractCooldownRoutine());
                }
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
               !handIcon.gameObject.activeInHierarchy;
    }

    private void PlaceDynamite()
    {
        if (hasPlacedDynamite) return;

        placedDynamite = Instantiate(dynamitePrefab, dynamitePlacePoint.position, Quaternion.identity);
        hasPlacedDynamite = true;

        // Debug.Log("Dynamite placed!");

        if (fireIcon != null)
        {
            fireIcon.SetActive(true);
        }
    }


    public void IgniteDynamite()
    {
        if (hasExploded || placedDynamite == null || !ItemInteractor.PlayerHasLighter()) return;

        Instantiate(explosionVFX, placedDynamite.transform.position, Quaternion.identity);
        Destroy(placedDynamite);
        placedDynamite = null;
        hasExploded = true;

        // Debug.Log("Dynamite exploded!");

        if (stoneToDestroy != null)
        {
            Destroy(stoneToDestroy);
        }

        if (fireIcon != null)
        {
            fireIcon.SetActive(false);
        }
    }

    public void SetEndScreenActive()
    {
        endScreen.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DynamiteZone"))
        {
            isInDynamiteZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DynamiteZone"))
        {
            isInDynamiteZone = false;
        }
    }
}