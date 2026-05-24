using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks and updates player scores throughout a match.
/// Scores start at zero and increase by the number of squares in each placed piece.
/// Bonus points are awarded for placing all pieces, with an extra bonus for placing I1 last.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int[] playerScores = new int[2];
    private bool isInitialized = false;

    private const int ALL_PIECES_BONUS = 15;
    private const int I1_BONUS = 5;

    private void Awake()
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

    /// <summary>
    /// Resets both player scores to zero. Safe to call multiple times — use this on game restart.
    /// </summary>
    public void InitializeScores()
    {
        playerScores[0] = 0;
        playerScores[1] = 0;
        isInitialized = true;

        Debug.Log("Scores initialized: P1=0, P2=0");
    }

    /// <summary>
    /// Adds the square count of the placed piece to the player's score and checks for bonuses.
    /// </summary>
    /// <param name="playerIndex">Zero-based index of the player who placed the piece.</param>
    /// <param name="pieceType">The type of piece that was placed.</param>
    public void PiecePlaced(int playerIndex, PieceManager.PieceType pieceType)
    {
        if (!isInitialized) InitializeScores();

        playerScores[playerIndex] += CountSquaresInPiece(pieceType);

        if (AllPiecesPlaced(playerIndex))
        {
            playerScores[playerIndex] += ALL_PIECES_BONUS;

            if (pieceType == PieceManager.PieceType.I1)
                playerScores[playerIndex] += I1_BONUS;
        }

        UpdateAllScoresUI();
    }

    /// <summary>
    /// Returns the current score for the given player.
    /// </summary>
    /// <param name="playerIndex">Zero-based player index.</param>
    /// <returns>Current score, or 0 if the index is out of range.</returns>
    public int GetPlayerScore(int playerIndex)
    {
        if (!isInitialized) InitializeScores();

        if (playerIndex >= 0 && playerIndex < playerScores.Length)
            return playerScores[playerIndex];

        return 0;
    }

    private int CountSquaresInPiece(PieceManager.PieceType pieceType)
    {
        bool[,] shape = PieceManager.pieceShapes[pieceType];
        int count = 0;

        for (int x = 0; x < shape.GetLength(0); x++)
            for (int y = 0; y < shape.GetLength(1); y++)
                if (shape[x, y]) count++;

        return count;
    }

    private bool AllPiecesPlaced(int playerIndex)
    {
        return GameManager.Instance.usedPieces[playerIndex].Count ==
               System.Enum.GetValues(typeof(PieceManager.PieceType)).Length;
    }

    private void UpdateAllScoresUI()
    {
        if (ScoreUI.Instance != null)
            ScoreUI.Instance.UpdateScores(playerScores);
    }
}