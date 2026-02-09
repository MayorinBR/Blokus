using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int[] playerScores = new int[2]; // Scores dos jogadores
    private bool isInitialized = false;

    private const int ALL_PIECES_BONUS = 15;
    private const int I1_BONUS = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeScores()
    {
        if (isInitialized) return;

        int totalSquares = 0;
        foreach (PieceManager.PieceType type in System.Enum.GetValues(typeof(PieceManager.PieceType)))
        {
            totalSquares += CountSquaresInPiece(type);
        }

        playerScores[0] = -totalSquares;
        playerScores[1] = -totalSquares;
        isInitialized = true;

        Debug.Log($"Scores iniciais calculados: J1={playerScores[0]}, J2={playerScores[1]}");
    }

    private int CalculateTotalSquaresForAllPieces()
    {
        int total = 0;
        foreach (PieceManager.PieceType type in System.Enum.GetValues(typeof(PieceManager.PieceType)))
        {
            total += CountSquaresInPiece(type);
        }
        return total;
    }

    private int CountSquaresInPiece(PieceManager.PieceType pieceType)
    {
        bool[,] shape = PieceManager.pieceShapes[pieceType];
        int count = 0;

        for (int x = 0; x < shape.GetLength(0); x++)
        {
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                if (shape[x, y]) count++;
            }
        }
        return count;
    }

    public void PiecePlaced(int playerIndex, PieceManager.PieceType pieceType)
    {
        if (!isInitialized) InitializeScores();

        // Atualiza a pontuação quando uma peça é colocada
        int squaresInPiece = CountSquaresInPiece(pieceType);
        playerScores[playerIndex] += squaresInPiece; // Remove os pontos negativos

        // Verifica bônus
        if (AllPiecesPlaced(playerIndex))
        {
            playerScores[playerIndex] += ALL_PIECES_BONUS; // Bônus por colocar todas as peças

            if (pieceType == PieceManager.PieceType.I1)
            {
                playerScores[playerIndex] += I1_BONUS; // Bônus adicional
            }
        }

        UpdateAllScoresUI();
    }

    private bool AllPiecesPlaced(int playerIndex)
    {
        return GameManager.Instance.usedPieces[playerIndex].Count ==
               System.Enum.GetValues(typeof(PieceManager.PieceType)).Length;
    }

    private void UpdateAllScoresUI()
    {
        if (ScoreUI.Instance != null)
        {
            ScoreUI.Instance.UpdateScores(playerScores);
        }
    }

    public int GetPlayerScore(int playerIndex)
    {
        if (!isInitialized)
            InitializeScores();

        if (playerIndex >= 0 && playerIndex < playerScores.Length)
            return playerScores[playerIndex];

        return 0;
    }
}