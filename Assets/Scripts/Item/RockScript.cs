using UnityEngine;

public class RockScript : MonoBehaviour
{
    [SerializeField] private GameObject explosionFX;
    // [SerializeField] private AudioSource breakSound;

    public void Break()
    {
        Instantiate(explosionFX, transform.position, Quaternion.identity);

        // if (breakSound != null)
        //     breakSound.Play();

        gameObject.SetActive(false);

        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Break();
        }
    }
}
