using System.Collections;
using UnityEngine;

public class BloodOverlay : MonoBehaviour
{
	[SerializeField] private GameObject[] overlayLevels;
	[SerializeField] private float fadeDelay = 0.5f;
	[SerializeField] private float autoHideDelay = 5f;

	private Coroutine fadeCoroutine;
	private Coroutine autoHideCoroutine;

	private void Start()
	{
		foreach (GameObject overlay in overlayLevels)
		{
			overlay.SetActive(false);
		}
	}

	public void UpdateOverlay(float currentHealth, float maxHealth)
	{
		int overlayIndex = GetOverlayIndex(currentHealth / maxHealth);

		if (fadeCoroutine != null)
			StopCoroutine(fadeCoroutine);

		fadeCoroutine = StartCoroutine(ShowOverlayWithFade(overlayIndex));

		if (autoHideCoroutine != null)
			StopCoroutine(autoHideCoroutine);

		autoHideCoroutine = StartCoroutine(AutoHideOverlayAfterDelay(autoHideDelay));
	}
	private int GetOverlayIndex(float healthPercent)
	{
		if (overlayLevels.Length == 0) return -1;

		int index = Mathf.FloorToInt((1f - healthPercent) * overlayLevels.Length);
		return Mathf.Clamp(index, 0, overlayLevels.Length - 1);
	}

	private IEnumerator ShowOverlayWithFade(int indexToShow)
	{
		yield return new WaitForSeconds(fadeDelay);

		for (int i = 0; i < overlayLevels.Length; i++)
		{
			overlayLevels[i].SetActive(i <= indexToShow);
		}
	}

	private IEnumerator AutoHideOverlayAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		yield return StartCoroutine(FadeOutOverlayStepByStep());
	}

	private IEnumerator FadeOutOverlayStepByStep()
	{
		for (int i = overlayLevels.Length - 1; i >= 0; i--)
		{
			overlayLevels[i].SetActive(false);

			yield return new WaitForSeconds(0.1f);
		}
	}
}