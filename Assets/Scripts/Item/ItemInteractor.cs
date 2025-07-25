using UnityEngine;
using UnityEngine.UI;

public class ItemInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fireButtonUI;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombPlacePoint;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private GameObject rockToDestroy;
    [SerializeField] private GameObject winScreen;

    private bool hasLighter = false;
    private bool hasBomb = false;
    private bool hasKey = false;
    private GameObject placedBomb;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryPlaceBomb();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryIgnite();
        }
    }

    private void TryInteract()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 3f))
        {
            GameObject obj = hit.collider.gameObject;

            switch (obj.tag)
            {
                case "Lighter":
                    hasLighter = true;
                    Destroy(obj);
                    Debug.Log("Picked up Lighter");
                    break;

                case "Bomb":
                    hasBomb = true;
                    Destroy(obj);
                    Debug.Log("Picked up Bomb");
                    break;

                case "Key":
                    hasKey = true;
                    Destroy(obj);
                    Debug.Log("Picked up Key");
                    break;

                case "Chest":
                    if (hasKey)
                    {
                        Destroy(obj);
                        Debug.Log("Chest opened!");
                    }
                    break;

                default:
                    break;
            }
        }
    }

    private void TryPlaceBomb()
    {
        if (hasBomb && placedBomb == null)
        {
            placedBomb = Instantiate(bombPrefab, bombPlacePoint.position, Quaternion.identity);
            Debug.Log("Bomb placed!");

            if (hasLighter)
            {
                fireButtonUI.gameObject.SetActive(true);
            }
        }
    }

    private void TryIgnite()
    {
        if (hasLighter && placedBomb != null)
        {
            Instantiate(explosionVFX, placedBomb.transform.position, Quaternion.identity);
            Destroy(placedBomb);
            placedBomb = null;

            if (rockToDestroy != null)
            {
                Destroy(rockToDestroy);
                Debug.Log("Rock destroyed!");
                EndGame();
            }

            fireButtonUI.gameObject.SetActive(false);
        }
    }

    private void EndGame()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            Debug.Log("You escaped! Game Over!");
        }
    }
}