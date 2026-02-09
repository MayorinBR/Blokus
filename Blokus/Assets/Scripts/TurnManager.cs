using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public TextMeshProUGUI turnText; // Alterado para TextMeshProUGUI
    public Color[] playerColors;
    public static TurnManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateTurnUI();
    }

    public void UpdateTurnUI()
    {
        int currentPlayer = GameManager.Instance.currentPlayer;
        turnText.text = $"Player {currentPlayer + 1} turn";

        // Update both turn UI and pieces remaining
        if (TurnUI.Instance != null)
        {
            TurnUI.Instance.UpdateTurnUI(currentPlayer);
        }

        // Force update pieces count
        if (ScoreUI.Instance != null)
        {
            ScoreUI.Instance.UpdatePiecesRemaining();
        }
    }
}