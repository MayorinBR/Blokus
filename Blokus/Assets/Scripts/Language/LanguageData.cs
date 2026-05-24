using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that holds all translation entries for one language.
/// Translations are stored as a serializable list (editable in the Inspector)
/// and cached in a Dictionary at runtime for O(1) lookups.
/// </summary>
[CreateAssetMenu(fileName = "New Language", menuName = "Localization/Language")]
public class LanguageData : ScriptableObject
{
    /// <summary>Display name of the language, e.g. "Português" or "English".</summary>
    public string languageName;

    /// <summary>IETF language tag, e.g. "pt-BR" or "en-US".</summary>
    public string languageCode;

    [Serializable]
    public struct TranslationEntry
    {
        public string key;
        [TextArea] public string value;
    }

    public List<TranslationEntry> translations = new List<TranslationEntry>();

    private Dictionary<string, string> cache;

    /// <summary>
    /// Builds the internal lookup cache from the translations list.
    /// Called automatically by LocalizationManager before first use.
    /// Safe to call multiple times; rebuilds the cache each time.
    /// </summary>
    public void BuildCache()
    {
        cache = new Dictionary<string, string>(translations.Count, StringComparer.Ordinal);

        foreach (TranslationEntry entry in translations)
        {
            if (string.IsNullOrEmpty(entry.key)) continue;

            if (!cache.ContainsKey(entry.key))
                cache[entry.key] = entry.value;
            else
                Debug.LogWarning($"[LanguageData] Duplicate key \"{entry.key}\" in {name}. First value kept.");
        }
    }

    /// <summary>
    /// Returns the translation for the given key.
    /// Returns "MISSING: {key}" if the key is not found, so missing entries
    /// are immediately visible during development.
    /// </summary>
    /// <param name="key">The localization key to look up.</param>
    /// <returns>Translated string, or a MISSING sentinel.</returns>
    public string GetTranslation(string key)
    {
        if (cache == null) BuildCache();

        if (cache.TryGetValue(key, out string value))
            return value;

        return $"MISSING: {key}";
    }

#if UNITY_EDITOR
    /// <summary>
    /// Invalidates the cache when the ScriptableObject is modified in the Editor,
    /// so changes to the translations list are reflected immediately in Play Mode.
    /// </summary>
    private void OnValidate()
    {
        cache = null;
    }
#endif
}