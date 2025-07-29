using UnityEngine;
using System.Collections;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] private CanvasGroup canvasGroup;

    public static NotificationManager Instance { get; private set; }

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowMessage(string message)
    {
        ShowMessage(message, defaultDisplayDuration);
    }

    public void ShowMessage(string message, float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));

        yield return new WaitForSeconds(duration);

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.5f));

        notificationText.gameObject.SetActive(false);
        notificationText.text = string.Empty;
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void ShowWelcomeMessage()
    {
        ShowMessage("Welcome to the game!\nFind a way to escape. Be careful, monsters are lurking...");
    }

    public void ShowReturningPlayerMessage()
    {
        ShowMessage("Welcome back! Continue your escape... but beware the monsters.");
    }

    public void ShowPickedUpKey()
    {
        ShowMessage("You picked up a key!");
    }

    public void ShowPickedUpLighter()
    {
        ShowMessage("You picked up a lighter!");
    }

    public void ShowPickedUpDynamite()
    {
        ShowMessage("You picked up dynamite!");
    }

    public void ShowMissingItemWarning()
    {
        ShowMessage("Something is missing here...");
    }

    public void ShowReadyToIgniteMessage()
    {
        ShowMessage("You’re ready. Light it and run!");
    }

    public void ShowLockedMessage()
    {
        ShowMessage("It's locked.");
    }

    public void ShowUnlockedMessage()
    {
        ShowMessage("Unlocked!");
    }

    public void HideNotification()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        notificationText.gameObject.SetActive(false);
        notificationText.text = string.Empty;
    }
}