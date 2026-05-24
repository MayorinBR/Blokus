using UnityEngine;
using TMPro;

/// <summary>
/// Legacy turn display component. Updates a simple text label with the active player's name.
/// In the current implementation, <see cref="TurnUI"/> handles all turn-related HUD updates;
/// this class delegates to it and exists for backward compatibility.
/// </summary>
public class TurnManager : MonoBehaviour
{
    /// <summary>Text element displaying the current turn label.</summary>
    public TextMeshProUGUI turnText;

    public Color[] playerColors;
    public static TurnManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateTurnUI();
    }

    /// <summary>
    /// Refreshes the turn text and delegates a full HUD update to <see cref="TurnUI"/>.
    /// </summary>
    public void UpdateTurnUI()
    {
        int currentPlayer = GameManager.Instance.currentPlayer;

        if (turnText != null)
        {
            string text = LocalizationManager.Instance != null
                ? string.Format(LocalizationManager.Instance.GetText(LocalizationKeys.TurnPlayer), currentPlayer + 1)
                : $"Player {currentPlayer + 1} Turn";

            turnText.text = text;
        }

        if (TurnUI.Instance != null)
            TurnUI.Instance.UpdateTurnUI(currentPlayer);

        if (ScoreUI.Instance != null)
            ScoreUI.Instance.UpdatePiecesRemaining();
    }
}