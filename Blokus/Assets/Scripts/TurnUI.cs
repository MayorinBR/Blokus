using UnityEngine;
using TMPro;

public class TurnUI : MonoBehaviour
{
    public static TurnUI Instance;

    [Header("TextMeshPro References")]
    public TextMeshProUGUI[] playerTurnTexts; // Textos que mostram "Player X Turn"
    public TextMeshProUGUI[] piecesRemainingTexts; // Referência aos textos de peças restantes

    [Header("Format Settings")]
    public int normalFontSize = 24;
    public int activeFontSize = 32;
    public FontWeight normalFontWeight = FontWeight.Regular;
    public FontWeight activeFontWeight = FontWeight.Bold;

    [Header("Color Settings")]
    [Range(0f, 1f)]
    public float inactiveColorAlpha = 0.5f; // Transparência para jogadores inativos

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateTurnUI(int currentPlayer)
    {
        // Verifica se está no modo PvP ou PvAI
        bool isPvP = GameSettings.Instance != null && GameSettings.Instance.isPvP;

        for (int i = 0; i < playerTurnTexts.Length; i++)
        {
            if (playerTurnTexts[i] != null)
            {
                bool isActivePlayer = (i == currentPlayer);

                // Obtém a cor base do jogador do GameManager
                Color playerColor = GameManager.Instance.playerColors[i];

                if (isActivePlayer)
                {
                    // JOGADOR ATIVO
                    // Usa a cor do jogador com opacidade total
                    playerTurnTexts[i].color = playerColor;
                    playerTurnTexts[i].fontSize = activeFontSize;
                    playerTurnTexts[i].fontWeight = activeFontWeight;

                    // Define o texto baseado no modo de jogo
                    if (!isPvP && i == 1)
                    {
                        playerTurnTexts[i].text = "AI Turn";
                    }
                    else
                    {
                        playerTurnTexts[i].text = $"Player {i + 1} Turn";
                    }
                }
                else
                {
                    // JOGADOR INATIVO
                    // Usa a cor do jogador com transparência reduzida
                    Color inactiveColor = playerColor;
                    inactiveColor.a = inactiveColorAlpha;
                    playerTurnTexts[i].color = inactiveColor;
                    playerTurnTexts[i].fontSize = normalFontSize;
                    playerTurnTexts[i].fontWeight = normalFontWeight;

                    // Define o texto baseado no modo de jogo
                    if (!isPvP && i == 1)
                    {
                        playerTurnTexts[i].text = "AI";
                    }
                    else
                    {
                        playerTurnTexts[i].text = $"Player {i + 1}";
                    }
                }
            }
        }

        // Garante que as peças restantes são atualizadas
        if (ScoreUI.Instance != null)
        {
            ScoreUI.Instance.UpdatePiecesRemaining();
        }
    }
}