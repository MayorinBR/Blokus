using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls the AI player's turn. Evaluates available pieces and board positions
/// to find and execute a valid move, prioritising larger pieces and strategic corners.
/// Notifies <see cref="TurnUI"/> while processing and skips the turn if no move is found.
/// </summary>
public class AIController : MonoBehaviour
{
    public static AIController Instance;

    [Tooltip("Set to false to disable the AI without removing the component.")]
    public bool isActive = true;

    [Tooltip("Seconds the AI waits before making its move. Populated from GameSettings at startup.")]
    public float moveDelay;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (GameSettings.Instance != null)
            moveDelay = GameSettings.Instance.aiDelay;
    }

    /// <summary>
    /// Returns all piece types that the given player has not yet placed.
    /// </summary>
    /// <param name="playerIndex">Zero-based player index.</param>
    /// <returns>List of available piece types.</returns>
    public List<PieceManager.PieceType> GetAvailablePieces(int playerIndex)
    {
        var available = new List<PieceManager.PieceType>();

        foreach (PieceManager.PieceType type in System.Enum.GetValues(typeof(PieceManager.PieceType)))
        {
            if (GameManager.Instance.CanUsePiece(type, playerIndex))
                available.Add(type);
        }

        return available;
    }

    /// <summary>
    /// Triggers the AI move coroutine for the given player.
    /// Switches turns immediately if the game is in PvP mode or the AI is inactive.
    /// </summary>
    /// <param name="aiPlayerIndex">Zero-based index of the AI player.</param>
    public void MakeAIMove(int aiPlayerIndex)
    {
        if (GameSettings.Instance.isPvP || !isActive)
        {
            GameManager.Instance.SwitchPlayer();
            return;
        }

        StartCoroutine(AIMoveRoutine(aiPlayerIndex));
    }

    /// <summary>
    /// Stops all running AI coroutines. Call this before resetting the game.
    /// </summary>
    public void ResetAI()
    {
        StopAllCoroutines();
    }

    private IEnumerator AIMoveRoutine(int aiPlayerIndex)
    {
        if (TurnUI.Instance != null)
            TurnUI.Instance.ShowAIThinking(true);

        yield return new WaitForSeconds(moveDelay);

        List<PieceManager.PieceType> availablePieces = GetAvailablePieces(aiPlayerIndex);

        if (availablePieces.Count == 0)
        {
            Debug.Log("AI has no available pieces. Skipping turn.");
            PlayerStatusUI.Instance?.ShowForPlayer(aiPlayerIndex, LocalizationKeys.StatusNoPieces);

            if (TurnUI.Instance != null) TurnUI.Instance.ShowAIThinking(false);
            GameManager.Instance.SwitchPlayer();
            yield break;
        }

        availablePieces = availablePieces
            .OrderByDescending(p => PieceManager.pieceShapes[p].GetLength(0) * PieceManager.pieceShapes[p].GetLength(1))
            .ToList();

        foreach (PieceManager.PieceType pieceType in availablePieces)
        {
            GameObject testPiece = PieceManager.Instance.CreatePiece(pieceType, aiPlayerIndex);
            testPiece.SetActive(false);

            List<Vector2Int> positionsToTry = GetStrategicPositions(aiPlayerIndex);

            foreach (Vector2Int pos in positionsToTry)
            {
                for (int rotation = 0; rotation < 360; rotation += 90)
                {
                    Vector3 testPosition = BoardManager.Instance.BoardToWorldPosition(pos.x, pos.y);
                    testPiece.transform.position = testPosition;
                    testPiece.transform.rotation = Quaternion.Euler(0, rotation, 0);

                    if (GameManager.Instance.IsValidMove(testPiece))
                    {
                        testPiece.SetActive(true);

                        if (GameManager.Instance.PlacePiece(testPiece, testPosition))
                        {
                            if (TurnUI.Instance != null) TurnUI.Instance.ShowAIThinking(false);
                            yield break;
                        }
                        else
                        {
                            testPiece.SetActive(false);
                        }
                    }
                }
            }

            Destroy(testPiece);
        }

        Debug.Log("AI found no valid moves. Skipping turn.");
        PlayerStatusUI.Instance?.ShowForPlayer(aiPlayerIndex, LocalizationKeys.StatusNoMoves);

        if (TurnUI.Instance != null) TurnUI.Instance.ShowAIThinking(false);
        GameManager.Instance.SwitchPlayer();
    }

    /// <summary>
    /// Generates a list of target coordinates based on local territorial proximity and game rules.
    /// </summary>
    /// <param name="aiPlayerIndex">Index of the AI player executing the turn.</param>
    /// <returns>A distinct list of strategic board coordinates to evaluate.</returns>
    private List<Vector2Int> GetStrategicPositions(int aiPlayerIndex)
    {
        var positions = new List<Vector2Int>();
        int boardSize = BoardManager.BoardSize;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (GameManager.Instance.occupiedSpaces[x, y] == aiPlayerIndex)
                {
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        for (int dy = -3; dy <= 3; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize &&
                                GameManager.Instance.occupiedSpaces[nx, ny] == -1)
                            {
                                positions.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }
        }

        if (GameManager.Instance.IsFirstMove(aiPlayerIndex))
        {
            Vector2Int startPos = GameManager.Instance.startPositions[aiPlayerIndex];
            positions.Insert(0, startPos);
        }

        while (positions.Count < 20)
        {
            int x = Random.Range(0, boardSize);
            int y = Random.Range(0, boardSize);
            positions.Add(new Vector2Int(x, y));
        }

        return positions.Distinct().ToList();
    }

    private string L(string key) =>
        LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key) : key;
}