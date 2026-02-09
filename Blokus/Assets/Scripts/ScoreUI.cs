using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public static ScoreUI Instance;

    [Header("TextMeshPro References")]
    public TextMeshProUGUI[] playerScoreTexts;
    public TextMeshProUGUI[] piecesRemainingTexts;
    public GameObject gameOverPanel;
    public GameObject postGameOverPanel;
    public TextMeshProUGUI winnerText;

    [Header("Settings")]
    public Color[] playerColors;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Garante que o painel está desativado imediatamente
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Verificação redundante para garantir que ficou desativado
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(false);
        }

        UpdatePiecesRemaining();
    }

    public void UpdateScores(int[] scores)
    {
        // Esta função agora só será chamada no final do jogo
        if (gameOverPanel != null)
        {
            // Determina o vencedor
            string winnerMessage;
            if (scores[0] > scores[1])
            {
                winnerMessage = "Player 1 Wins!";
            }
            else if (scores[1] > scores[0])
            {
                winnerMessage = "Player 2 Wins!";
            }
            else
            {
                winnerMessage = "It's a Tie!";
            }

            winnerText.text = winnerMessage;

            // Atualiza os scores finais
            for (int i = 0; i < scores.Length && i < playerScoreTexts.Length; i++)
            {
                if (playerScoreTexts[i] != null)
                {
                    playerScoreTexts[i].text = $"Player {i + 1} Score: {scores[i]}";
                    playerScoreTexts[i].color = GameManager.Instance.playerColors[i];
                }
            }
        }
    }

    public void UpdatePiecesRemaining()
    {
        if (piecesRemainingTexts == null || piecesRemainingTexts.Length < 2) return;

        for (int i = 0; i < 2; i++)
        {
            if (piecesRemainingTexts[i] != null)
            {
                int remainingPieces = PiecePalette.Instance.GetAvailablePiecesForPlayer(i).Count;
                piecesRemainingTexts[i].text = $"{remainingPieces} Pieces";
                piecesRemainingTexts[i].color = GameManager.Instance.playerColors[i];
            }
        }
    }
}