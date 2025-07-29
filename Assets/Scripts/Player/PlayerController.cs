using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth;
    [SerializeField] private float damageCooldown;

    [Header("Death Settings")]
    [SerializeField] private Transform cameraDeathAnchor;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Avatar deathAvatar;
    [SerializeField] private GameObject crosshair;

    [Header("Fade Effect")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration;

    [Header("Runtime State")]
    private int currentHealth;
    private float damageTimer = 0f;
    private bool isDead = false;
    private BloodOverlay bloodOverlay;

    [Header("Public Accessors")]
    public bool IsDead => isDead;

    [Header("References")]
    private HealthBarManager healthBar;

    private void Awake()
    {
        bloodOverlay = FindAnyObjectByType<BloodOverlay>();
        healthBar = FindAnyObjectByType<HealthBarManager>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        bloodOverlay?.UpdateOverlay(currentHealth, maxHealth);
        SetFadeAlpha(0);
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    private void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && damageTimer <= 0f && !isDead)
        {
            int damage = 20;
            TakeDamage(damage);
            damageTimer = damageCooldown;

            SoundManager.Instance.PlayBeingHitSound();

            if (currentHealth - damage <= 0) SoundManager.Instance.PlayDieSound();
        }
    }

    private void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        healthBar.SetHealth(currentHealth);

        bloodOverlay?.UpdateOverlay(currentHealth, maxHealth);

        // Debug.Log($"Player took {damage} damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        crosshair.SetActive(false);

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.avatar = deathAvatar;
            animator.SetTrigger("Die");

            if (cameraDeathAnchor != null)
            {
                cameraTransform.SetParent(cameraDeathAnchor);
                cameraTransform.localPosition = Vector3.zero;
                cameraTransform.localRotation = Quaternion.identity;
            }
        }

        FirstPersonController fpc = GetComponent<FirstPersonController>();
        if (fpc != null)
        {
            fpc.SetBobAmountValue(0);
            fpc.DisableLookAround();
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        // Debug.Log("Player died!");
        DisableAllEnemyAnimations();

        StartCoroutine(FadeAndReloadSceneWithDelay());
    }

    private IEnumerator FadeAndReloadSceneWithDelay()
    {
        yield return new WaitForSeconds(7f);

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    private void DisableAllEnemyAnimations()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyAnimationController anim = enemy.GetComponent<EnemyAnimationController>();
            if (anim != null)
            {
                anim.StopAttack();
                anim.PerformPauseAction(false);
                anim.SetVelocity(0f);
            }
        }
    }
}