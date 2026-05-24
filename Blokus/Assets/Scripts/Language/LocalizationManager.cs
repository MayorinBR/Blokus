using UnityEngine;
using System;

/// <summary>
/// Singleton that manages the active language and notifies listeners when it changes.
/// Persists across scenes via DontDestroyOnLoad.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Tooltip("Language used when no saved preference is found.")]
    public LanguageData defaultLanguage;

    [Tooltip("All available languages. Used to restore saved preference by language code.")]
    public LanguageData[] availableLanguages;

    /// <summary>Fired after the active language has changed and all caches are ready.</summary>
    public event Action OnLanguageChanged;

    private LanguageData currentLanguage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLanguage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns the translated string for the given key in the active language.
    /// Returns the key itself if no language is loaded yet.
    /// </summary>
    /// <param name="key">Localization key.</param>
    /// <returns>Translated string, or "MISSING: {key}" if the key does not exist.</returns>
    public string GetText(string key)
    {
        if (currentLanguage == null) return key;
        return currentLanguage.GetTranslation(key);
    }

    /// <summary>
    /// Switches the active language, rebuilds the lookup cache, saves the preference,
    /// and fires OnLanguageChanged so all LocalizedText components update themselves.
    /// </summary>
    /// <param name="newLanguage">The LanguageData asset to activate.</param>
    public void SetLanguage(LanguageData newLanguage)
    {
        if (newLanguage == null)
        {
            Debug.LogWarning("[LocalizationManager] SetLanguage called with null.");
            return;
        }

        currentLanguage = newLanguage;
        currentLanguage.BuildCache();

        PlayerPrefs.SetString("SelectedLanguage", newLanguage.languageCode);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Returns the currently active LanguageData asset.
    /// </summary>
    public LanguageData GetCurrentLanguage() => currentLanguage;

    // ── Private ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the saved language preference, or falls back to defaultLanguage.
    /// Called once in Awake before any other system tries to resolve keys.
    /// </summary>
    private void InitializeLanguage()
    {
        string savedCode = PlayerPrefs.GetString("SelectedLanguage", string.Empty);

        LanguageData resolved = null;

        if (!string.IsNullOrEmpty(savedCode) && availableLanguages != null)
        {
            foreach (LanguageData lang in availableLanguages)
            {
                if (lang != null && lang.languageCode == savedCode)
                {
                    resolved = lang;
                    break;
                }
            }
        }

        if (resolved == null) resolved = defaultLanguage;

        if (resolved == null)
        {
            Debug.LogError("[LocalizationManager] No language could be loaded. "
                         + "Assign defaultLanguage in the Inspector.");
            return;
        }

        // Set without firing the event — nothing is listening yet at Awake time.
        currentLanguage = resolved;
        currentLanguage.BuildCache();
    }
}