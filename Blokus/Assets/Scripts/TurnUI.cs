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
    public int activeFontSize = 28;
    public FontWeight normalFontWeight = FontWeight.Regular;
    public FontWeight activeFontWeight = FontWeight.Bold;
    public Color activeColor = Color.yellow; // Cor destacada para o jogador atual

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
        for (int i = 0; i < playerTurnTexts.Length; i++)
        {
            if (playerTurnTexts[i] != null)
            {
                bool isActivePlayer = (i == currentPlayer);

                // Atualiza o texto do turno
                playerTurnTexts[i].text = $"Player {i + 1}: ";
                playerTurnTexts[i].fontSize = isActivePlayer ? activeFontSize : normalFontSize;
                playerTurnTexts[i].fontWeight = isActivePlayer ? activeFontWeight : normalFontWeight;
                playerTurnTexts[i].color = isActivePlayer ? activeColor : GameManager.Instance.playerColors[i];
            }
        }

        // Garante que as peças restantes são atualizadas
        ScoreUI.Instance.UpdatePiecesRemaining();
    }

}