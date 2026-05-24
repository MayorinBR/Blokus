using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the settings panel UI. Populates language selection buttons from
/// <see cref="LocalizationManager"/> and handles player color selection,
/// ensuring both players always have distinct colors.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("References")]
    public MainMenuManager mainMenuManager;

    [Header("Close")]
    public Button closeButton;

    [Header("Language")]
    [Tooltip("Parent transform where language buttons will be instantiated.")]
    public Transform languageButtonContainer;
    [Tooltip("Prefab used for each language button. Must have a Button and a TextMeshProUGUI child.")]
    public GameObject languageButtonPrefab;
    [Tooltip("Optional font applied to each language button label.")]
    public TMP_FontAsset languageButtonFont;

    [Header("Piece Color Selection")]
    public Image p1ColorPreview;
    public Button p1ColorPrevButton;
    public Button p1ColorNextButton;
    public TextMeshProUGUI p1ColorLabel;

    [Space]
    public Image p2ColorPreview;
    public Button p2ColorPrevButton;
    public Button p2ColorNextButton;
    public TextMeshProUGUI p2ColorLabel;

    private void OnEnable()
    {
        ResetAndRegisterButtons();
        PopulateLanguageButtons();

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshColorPreviews;

        RefreshColorPreviews();
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshColorPreviews;
    }

    /// <summary>
    /// Clears existing button listeners and assigns fresh ones.
    /// Called every time the panel is enabled to prevent duplicate callbacks.
    /// </summary>
    private void ResetAndRegisterButtons()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnClosePressed);

        p1ColorPrevButton.onClick.RemoveAllListeners();
        p1ColorPrevButton.onClick.AddListener(() => ChangeColor(0, -1));

        p1ColorNextButton.onClick.RemoveAllListeners();
        p1ColorNextButton.onClick.AddListener(() => ChangeColor(0, 1));

        p2ColorPrevButton.onClick.RemoveAllListeners();
        p2ColorPrevButton.onClick.AddListener(() => ChangeColor(1, -1));

        p2ColorNextButton.onClick.RemoveAllListeners();
        p2ColorNextButton.onClick.AddListener(() => ChangeColor(1, 1));
    }

    /// <summary>
    /// Instantiates one button per available language inside <see cref="languageButtonContainer"/>.
    /// Each button calls <see cref="LocalizationManager.SetLanguage"/> when clicked.
    /// Clears any previously instantiated buttons before rebuilding.
    /// </summary>
    private void PopulateLanguageButtons()
    {
        if (languageButtonContainer == null || languageButtonPrefab == null) return;
        if (LocalizationManager.Instance == null) return;

        foreach (Transform child in languageButtonContainer)
            Destroy(child.gameObject);

        foreach (LanguageData lang in LocalizationManager.Instance.availableLanguages)
        {
            if (lang == null) continue;

            GameObject buttonObj = Instantiate(languageButtonPrefab, languageButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
            {
                if (languageButtonFont != null) label.font = languageButtonFont;
                label.text = lang.languageName;
            }

            LanguageData captured = lang;
            button?.onClick.AddListener(() => LocalizationManager.Instance.SetLanguage(captured));
        }
    }

    /// <summary>
    /// Advances or retreats the color selection for a player, skipping any color
    /// already assigned to the opponent.
    /// </summary>
    /// <param name="playerIndex">Zero-based player index (0 = Player 1, 1 = Player 2).</param>
    /// <param name="direction">+1 to go forward, -1 to go backward through the color list.</param>
    private void ChangeColor(int playerIndex, int direction)
    {
        GameSettings gs = GameSettings.Instance;
        if (gs == null)
        {
            Debug.LogError("[SettingsManager] GameSettings.Instance is null.");
            return;
        }

        if (gs.pieceColorOptions.Length < 2)
        {
            Debug.LogWarning("[SettingsManager] Not enough color options to cycle.");
            return;
        }

        int total = gs.pieceColorOptions.Length;
        int current = playerIndex == 0 ? gs.player1ColorIndex : gs.player2ColorIndex;
        int opponent = playerIndex == 0 ? gs.player2ColorIndex : gs.player1ColorIndex;

        int next = (current + direction + total) % total;

        if (next == opponent)
            next = (next + direction + total) % total;

        if (playerIndex == 0)
            gs.player1ColorIndex = next;
        else
            gs.player2ColorIndex = next;

        Debug.Log($"[SettingsManager] Player {playerIndex + 1} color index: {next}.");

        RefreshColorPreviews();
        NotifyAppliers();
    }

    /// <summary>
    /// Synchronises the color preview images and index labels with the current <see cref="GameSettings"/> values.
    /// Also guards against both players sharing the same color index.
    /// </summary>
    private void RefreshColorPreviews()
    {
        GameSettings gs = GameSettings.Instance;
        if (gs == null) return;

        if (gs.player1ColorIndex == gs.player2ColorIndex && gs.pieceColorOptions.Length > 1)
            gs.player2ColorIndex = (gs.player1ColorIndex + 1) % gs.pieceColorOptions.Length;

        UpdatePlayerUI(0, p1ColorPreview, p1ColorLabel, gs);
        UpdatePlayerUI(1, p2ColorPreview, p2ColorLabel, gs);
    }

    private void UpdatePlayerUI(int playerIndex, Image preview, TextMeshProUGUI label, GameSettings gs)
    {
        int current = playerIndex == 0 ? gs.player1ColorIndex : gs.player2ColorIndex;

        if (preview != null) preview.color = gs.GetPlayerColor(playerIndex);
        if (label != null) label.text = $"{current + 1}/{gs.pieceColorOptions.Length}";
    }

    private void NotifyAppliers()
    {
        foreach (GameSettingsApplier applier in FindObjectsByType<GameSettingsApplier>(FindObjectsInactive.Exclude))
            applier.ApplyAll();

        if (mainMenuManager != null)
            mainMenuManager.RefreshPlayerColors();
    }

    private void OnClosePressed()
    {
        if (mainMenuManager != null)
            mainMenuManager.CloseSettings();
        else
            gameObject.SetActive(false);
    }
}