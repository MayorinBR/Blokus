using UnityEngine;
using TMPro;

/// <summary>
/// Manages two persistent status panels, one per player.
/// A panel activates when a player has no valid moves or no pieces remaining
/// and stays visible until <see cref="ResetPanels"/> is called on game restart.
/// All display strings are resolved through <see cref="LocalizationManager"/>.
/// </summary>
[DefaultExecutionOrder(-50)]
public class PlayerStatusUI : MonoBehaviour
{
    public static PlayerStatusUI Instance { get; private set; }

    [Header("Player 1 Panel")]
    [Tooltip("Panel shown when Player 1 is out of moves or pieces.")]
    public GameObject player1Panel;
    [Tooltip("Text element inside the Player 1 panel.")]
    public TextMeshProUGUI player1Text;

    [Header("Player 2 / AI Panel")]
    [Tooltip("Panel shown when Player 2 or the AI is out of moves or pieces.")]
    public GameObject player2Panel;
    [Tooltip("Text element inside the Player 2 / AI panel.")]
    public TextMeshProUGUI player2Text;

    private readonly string[] statusKeys = new string[2];
    private readonly bool[] panelActive = new bool[2];

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        HidePanel(0);
        HidePanel(1);
    }

    private void Start()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshAll;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshAll;
    }

    /// <summary>
    /// Activates the status panel for the given player and displays the localized message.
    /// The panel remains visible until <see cref="ResetPanels"/> is called.
    /// </summary>
    /// <param name="playerIndex">Zero-based player index (0 = Player 1, 1 = Player 2 / AI).</param>
    /// <param name="locKey">Localization key for the status message. Use <see cref="LocalizationKeys"/>.</param>
    public void ShowForPlayer(int playerIndex, string locKey)
    {
        if (playerIndex < 0 || playerIndex > 1) return;

        statusKeys[playerIndex] = locKey;
        panelActive[playerIndex] = true;

        RefreshPanel(playerIndex);
        GetPanel(playerIndex)?.SetActive(true);
    }

    /// <summary>
    /// Hides both status panels and clears their state.
    /// Call this when a new game starts.
    /// </summary>
    public void ResetPanels()
    {
        for (int i = 0; i < 2; i++)
        {
            panelActive[i] = false;
            statusKeys[i] = null;
            HidePanel(i);
        }
    }

    private void RefreshAll()
    {
        for (int i = 0; i < 2; i++)
            if (panelActive[i]) RefreshPanel(i);
    }

    private void RefreshPanel(int playerIndex)
    {
        TextMeshProUGUI label = playerIndex == 0 ? player1Text : player2Text;
        if (label == null || string.IsNullOrEmpty(statusKeys[playerIndex])) return;

        label.text = L(statusKeys[playerIndex]);

        if (GameManager.Instance != null)
            label.color = GameManager.Instance.playerColors[playerIndex];
    }

    private void HidePanel(int playerIndex) => GetPanel(playerIndex)?.SetActive(false);

    private GameObject GetPanel(int playerIndex) =>
        playerIndex == 0 ? player1Panel : player2Panel;

    private string L(string key) =>
        LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key) : key;
}