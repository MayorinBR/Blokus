using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays a temporary on-screen notification that fades out after a set duration.
/// Requires a <see cref="CanvasGroup"/> on the panel for the fade animation.
/// Call <see cref="Show"/> from any script to trigger a message.
/// </summary>
public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Root panel that wraps the notification. Must have a CanvasGroup component.")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Timing")]
    [Tooltip("Seconds the notification stays fully visible before fading.")]
    [SerializeField] private float displayDuration = 2.5f;
    [Tooltip("Duration of the fade-out in seconds.")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine activeCoroutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            canvasGroup = panel != null ? panel.GetComponent<CanvasGroup>() : null;
            if (panel != null) panel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Shows the notification with the given message.
    /// Interrupts and replaces any currently active notification.
    /// </summary>
    /// <param name="message">Text to display.</param>
    public void Show(string message)
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (messageText != null) messageText.text = message;
        if (panel != null) panel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        if (panel != null) panel.SetActive(false);
        activeCoroutine = null;
    }
}