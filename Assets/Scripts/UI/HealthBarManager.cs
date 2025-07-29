using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Image fill;
    [SerializeField] private Slider slider;

    [Header("Timing Settings")]
    [SerializeField] private float delayBeforeStart;
    [SerializeField] private float smoothDuration;
    private Coroutine smoothRoutine;


    public void SetHealth(float health)
    {
        if (smoothRoutine != null)
            StopCoroutine(smoothRoutine);

        smoothRoutine = StartCoroutine(SmoothHealthChange(health));
    }

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    private IEnumerator SmoothHealthChange(float targetHealth)
    {
        yield return new WaitForSeconds(delayBeforeStart);

        float currentHealth = slider.value;
        float elapsed = 0f;

        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(currentHealth, targetHealth, elapsed / smoothDuration);
            yield return null;
        }

        slider.value = targetHealth;
    }
}