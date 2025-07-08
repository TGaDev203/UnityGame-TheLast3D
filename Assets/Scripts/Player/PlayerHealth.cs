using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Avatar deathAvatar;
    [SerializeField] private Transform cameraDeathAnchor;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform cameraTransform;
    private BloodOverlay bloodOverlay;
    private bool isDead = false;
    private int currentHealth;

    public bool IsDead => isDead;

    private void Awake()
    {
        bloodOverlay = FindAnyObjectByType<BloodOverlay>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        bloodOverlay?.UpdateOverlay(currentHealth, maxHealth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            SoundManager.Instance.PlayBeingHitSound();
            TakeDamage(20);
        }
    }

    private void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);
        bloodOverlay.UpdateOverlay(currentHealth, maxHealth);

        Debug.Log($"Player took {damage} damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.avatar = deathAvatar;
            animator.SetTrigger("Die");

            if (cameraDeathAnchor != null)
            {
                cameraTransform.transform.SetParent(cameraDeathAnchor);
                cameraTransform.localPosition = Vector3.zero;

                cameraTransform.localRotation = Quaternion.identity;
            }

            GetComponent<FirstPersonController>().SetBobAmountValue(0);
            GetComponent<FirstPersonController>().DisableLookAround();
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        isDead = true;
        Debug.Log("Player died!");

        DisableAllEnemyAnimations();

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    private void DisableAllEnemyAnimations()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            IEnemyAnimation anim = enemy.GetComponent<IEnemyAnimation>();
            if (anim != null)
            {
                anim.StopAttack();
                anim.StopLookAround();
                anim.SetVelocity(0f);
            }
        }
    }
}