using UnityEngine;
using TMPro;

/// <summary>
/// Attach to any GameObject that has a TextMeshProUGUI component.
/// Automatically sets the text to the resolved translation for localizationKey
/// and updates it whenever the active language changes.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("The localization key whose value will be displayed.")]
    public string localizationKey;

    private TextMeshProUGUI textElement;
    private bool isSubscribed = false;

    private void Awake()
    {
        textElement = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        Subscribe();
        UpdateText();
    }

    private void OnEnable()
    {
        // Re-subscribe if the GameObject was disabled and re-enabled
        // (e.g. a panel toggled off/on after Start already ran).
        Subscribe();
        UpdateText();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Forces an immediate text refresh. Call this if you change
    /// localizationKey at runtime.
    /// </summary>
    public void Refresh()
    {
        UpdateText();
    }

    private void Subscribe()
    {
        if (isSubscribed) return;
        if (LocalizationManager.Instance == null) return;

        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;
        if (LocalizationManager.Instance == null) return;

        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        isSubscribed = false;
    }

    private void UpdateText()
    {
        if (textElement == null) return;
        if (LocalizationManager.Instance == null) return;

        textElement.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}