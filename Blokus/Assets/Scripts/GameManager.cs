using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// Core game controller. Manages board state, turn order, move validation,
/// piece placement, and end-of-game detection for a two-player Blokus match.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    /// <summary>Zero-based index of the player whose turn it currently is.</summary>
    public int currentPlayer = 0;

    /// <summary>
    /// 2D grid tracking which player occupies each board cell.
    /// -1 = empty, 0 = Player 1, 1 = Player 2.
    /// </summary>
    public int[,] occupiedSpaces = new int[BoardManager.BoardSize, BoardManager.BoardSize];

    /// <summary>Piece types each player has already placed, indexed by player.</summary>
    public List<PieceManager.PieceType>[] usedPieces;

    /// <summary>Primary display colors for each player.</summary>
    public Color[] playerColors = new Color[4]
    {
        new Color(1.0f, 0.0f, 0.0f),
        new Color(0.0f, 0.0f, 1.0f),
        new Color(0.0f, 1.0f, 0.0f),
        new Color(1.0f, 1.0f, 0.0f)
    };

    /// <summary>Starting corner positions on the board for each player.</summary>
    public Vector2Int[] startPositions = new Vector2Int[4]
    {
        new Vector2Int(4, 4),
        new Vector2Int(9, 9),
        new Vector2Int(4, 9),
        new Vector2Int(9, 4)
    };

    /// <summary>Semi-transparent highlight colors used to mark starting positions on the board.</summary>
    public Color[] playerHighlightColors = new Color[4]
    {
        new Color(1f, 0f, 0f, 0.7f),
        new Color(0f, 0f, 1f, 0.7f),
        new Color(0f, 1f, 0f, 0.7f),
        new Color(1f, 1f, 0f, 0.7f)
    };

    private readonly List<GameObject> placedPieces = new List<GameObject>();

    private void Awake()
    {
        usedPieces = new List<PieceManager.PieceType>[2];
        for (int i = 0; i < usedPieces.Length; i++)
            usedPieces[i] = new List<PieceManager.PieceType>();

        if (Instance == null)
        {
            Instance = this;
            for (int x = 0; x < BoardManager.BoardSize; x++)
                for (int y = 0; y < BoardManager.BoardSize; y++)
                    occupiedSpaces[x, y] = -1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SyncColorsFromSettings();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.InitializeScores();

        if (TurnUI.Instance != null)
            TurnUI.Instance.UpdateTurnUI(currentPlayer);

        if (ScoreUI.Instance != null)
            ScoreUI.Instance.UpdatePiecesRemaining();

        if (PiecePalette.Instance != null)
            PiecePalette.Instance.DisplayAllPieces();

        Debug.Log($"[GameManager] Game started — current player: {currentPlayer}");
    }

    /// <summary>
    /// Returns whether the given player is still allowed to place the specified piece type.
    /// </summary>
    /// <param name="type">Piece type to check.</param>
    /// <param name="playerIndex">Zero-based player index.</param>
    /// <returns><c>true</c> if the piece has not yet been placed by that player.</returns>
    public bool CanUsePiece(PieceManager.PieceType type, int playerIndex)
    {
        return !usedPieces[playerIndex].Contains(type);
    }

    /// <summary>
    /// Records a piece as used for the given player. Has no effect if already recorded.
    /// </summary>
    /// <param name="type">Piece type that was placed.</param>
    /// <param name="playerIndex">Zero-based player index.</param>
    public void MarkPieceAsUsed(PieceManager.PieceType type, int playerIndex)
    {
        if (!usedPieces[playerIndex].Contains(type))
        {
            usedPieces[playerIndex].Add(type);
            Debug.Log($"Piece {type} marked as used by player {playerIndex}.");
        }
    }

    /// <summary>
    /// Resets the full game state: destroys placed pieces, clears the board,
    /// rebuilds the palette, resets scores, and returns to Player 1's turn.
    /// </summary>
    public void ResetGame()
    {
        foreach (GameObject piece in placedPieces)
            if (piece != null) Destroy(piece);
        placedPieces.Clear();

        for (int i = 0; i < usedPieces.Length; i++)
            usedPieces[i].Clear();

        for (int x = 0; x < BoardManager.BoardSize; x++)
            for (int y = 0; y < BoardManager.BoardSize; y++)
                occupiedSpaces[x, y] = -1;

        SyncColorsFromSettings();
        currentPlayer = 0;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeScores();
            ScoreUI.Instance.UpdateScores(new int[]
            {
                ScoreManager.Instance.GetPlayerScore(0),
                ScoreManager.Instance.GetPlayerScore(1)
            });
        }

        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.gameObject.SetActive(false);
            PlayerStatusUI.Instance?.ResetPanels();
        }

        BoardManager.Instance.ClearHighlights();
        PiecePalette.Instance.ClearAll();
        PiecePalette.Instance.DisplayAllPieces();
        ScoreUI.Instance.UpdatePiecesRemaining();

        if (TurnUI.Instance != null)
            TurnUI.Instance.UpdateTurnUI(0);
    }

    /// <summary>
    /// Validates whether the given piece can legally be placed at its current position.
    /// Checks board bounds, occupied cells, first-move starting position, and corner adjacency rules.
    /// </summary>
    /// <param name="piece">The piece GameObject to validate.</param>
    /// <returns><c>true</c> if the placement is legal.</returns>
    public bool IsValidMove(GameObject piece)
    {
        int pieceOwner = GetPiecePlayer(piece);
        if (pieceOwner != currentPlayer)
        {
            Debug.LogWarning($"Piece belongs to player {pieceOwner} but it is player {currentPlayer}'s turn.");
            return false;
        }

        List<Vector3> blockWorldPositions = BoardManager.Instance.GetPieceBlocksWorldPositions(piece);
        bool hasAdjacentCorner = false;
        bool hasAdjacentSide = false;

        foreach (Vector3 blockPos in blockWorldPositions)
        {
            Vector2Int boardPos = WorldToBoardPosition(blockPos);

            if (boardPos.x < 0 || boardPos.x >= BoardManager.BoardSize ||
                boardPos.y < 0 || boardPos.y >= BoardManager.BoardSize)
                return false;

            if (occupiedSpaces[boardPos.x, boardPos.y] != -1)
                return false;
        }

        if (IsFirstMove(currentPlayer))
        {
            bool anyBlockInStartPos = false;
            foreach (Vector3 blockPos in blockWorldPositions)
            {
                Vector2Int boardPos = WorldToBoardPosition(blockPos);
                if (boardPos.x == startPositions[currentPlayer].x &&
                    boardPos.y == startPositions[currentPlayer].y)
                {
                    anyBlockInStartPos = true;
                    break;
                }
            }

            if (!anyBlockInStartPos) return false;
        }
        else
        {
            foreach (Vector3 blockPos in blockWorldPositions)
            {
                Vector2Int boardPos = WorldToBoardPosition(blockPos);
                CheckAdjacentSpaces(boardPos, ref hasAdjacentCorner, ref hasAdjacentSide);
            }

            if (!hasAdjacentCorner || hasAdjacentSide) return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether the given player has at least one legal move available.
    /// Temporarily overrides <see cref="currentPlayer"/> for validation purposes.
    /// </summary>
    /// <param name="player">Zero-based player index to check.</param>
    /// <returns><c>true</c> if a valid move exists.</returns>
    public bool HasValidMoves(int player)
    {
        int originalPlayer = currentPlayer;
        currentPlayer = player;

        foreach (PieceManager.PieceType type in PiecePalette.Instance.GetAvailablePiecesForPlayer(player))
        {
            GameObject testPiece = PieceManager.Instance.CreatePiece(type, player);
            testPiece.SetActive(false);

            for (int x = 0; x < BoardManager.BoardSize; x++)
            {
                for (int y = 0; y < BoardManager.BoardSize; y++)
                {
                    Vector3 testPosition = BoardManager.Instance.BoardToWorldPosition(x, y);
                    testPiece.transform.position = testPosition;

                    for (int rotation = 0; rotation < 360; rotation += 90)
                    {
                        testPiece.transform.rotation = Quaternion.Euler(0, rotation, 0);

                        if (IsValidMove(testPiece))
                        {
                            Destroy(testPiece);
                            currentPlayer = originalPlayer;
                            return true;
                        }
                    }
                }
            }

            Destroy(testPiece);
        }

        currentPlayer = originalPlayer;
        return false;
    }

    /// <summary>
    /// Returns whether the given player has not yet placed any pieces.
    /// </summary>
    /// <param name="player">Zero-based player index.</param>
    /// <returns><c>true</c> if this is the player's first turn.</returns>
    public bool IsFirstMove(int player)
    {
        return usedPieces[player].Count == 0;
    }

    /// <summary>
    /// Advances the game to the next player's turn. Skips players with no valid moves
    /// and shows a localized notification. Ends the game if neither player can move.
    /// </summary>
    public void SwitchPlayer()
    {
        int previousPlayer = currentPlayer;
        BoardManager.Instance.ClearHighlights();

        bool isPvP = GameSettings.Instance != null && GameSettings.Instance.isPvP;

        if (isPvP)
        {
            do
            {
                currentPlayer = (currentPlayer + 1) % 2;
            } while (!HasValidMoves(currentPlayer) && !IsGameOver());
        }
        else
        {
            currentPlayer = (currentPlayer + 1) % 2;

            if (!HasValidMoves(currentPlayer))
            {
                Debug.Log($"Player {currentPlayer + 1} has no valid moves. Skipping turn.");
                PlayerStatusUI.Instance?.ShowForPlayer(currentPlayer, LocalizationKeys.StatusNoMoves);
                currentPlayer = (currentPlayer + 1) % 2;

                if (!HasValidMoves(currentPlayer))
                {
                    EndGame();
                    return;
                }
            }

            if (currentPlayer == 1 && !isPvP)
                StartCoroutine(AITurnDelay());
        }

        Debug.Log($"[SwitchPlayer] Switched from player {previousPlayer} to player {currentPlayer}.");

        if (TurnUI.Instance != null)
            TurnUI.Instance.UpdateTurnUI(currentPlayer);
        else
            Debug.LogWarning("[SwitchPlayer] TurnUI.Instance is null.");
    }

    /// <summary>
    /// Returns whether the game is over, i.e. neither player has any valid moves remaining.
    /// </summary>
    /// <returns><c>true</c> if no moves are available for either player.</returns>
    public bool IsGameOver()
    {
        return !HasValidMoves(0) && !HasValidMoves(1);
    }

    /// <summary>
    /// Ends the game, calculates final scores, and activates the game-over overlay.
    /// </summary>
    public void EndGame()
    {
        Debug.Log("Game over!");

        int[] scores = new int[2];
        scores[0] = ScoreManager.Instance.GetPlayerScore(0);
        scores[1] = ScoreManager.Instance.GetPlayerScore(1);

        if (scores[0] > scores[1]) Debug.Log("Player 1 wins!");
        else if (scores[1] > scores[0]) Debug.Log("Player 2 wins!");
        else Debug.Log("It's a tie!");

        GameOverUI.Instance.Show(scores);
    }

    /// <summary>
    /// Inspects the neighbours of the given board cell and sets the out-parameters to indicate
    /// whether the current player's pieces are adjacent by side or by corner.
    /// </summary>
    /// <param name="boardPos">Cell to inspect.</param>
    /// <param name="hasAdjacentCorner">Set to <c>true</c> if a diagonal neighbour belongs to the current player.</param>
    /// <param name="hasAdjacentSide">Set to <c>true</c> if an orthogonal neighbour belongs to the current player.</param>
    public void CheckAdjacentSpaces(Vector2Int boardPos, ref bool hasAdjacentCorner, ref bool hasAdjacentSide)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = boardPos.x + x;
                int checkY = boardPos.y + y;

                if (checkX >= 0 && checkX < BoardManager.BoardSize &&
                    checkY >= 0 && checkY < BoardManager.BoardSize)
                {
                    if (occupiedSpaces[checkX, checkY] == currentPlayer)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) == 1)
                            hasAdjacentSide = true;
                        else if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                            hasAdjacentCorner = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Converts a world-space position to a board grid coordinate.
    /// </summary>
    /// <param name="worldPosition">World-space position to convert.</param>
    /// <returns>Board cell coordinate, which may be out of bounds if the position is off the board.</returns>
    public Vector2Int WorldToBoardPosition(Vector3 worldPosition)
    {
        float tileSize = BoardManager.Instance.tileSize;
        float halfBoard = BoardManager.BoardSize / 2f;

        int x = Mathf.FloorToInt((worldPosition.x + halfBoard * tileSize) / tileSize);
        int y = Mathf.FloorToInt((worldPosition.z + halfBoard * tileSize) / tileSize);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Attempts to place the given piece at the specified position. Validates ownership,
    /// availability, and move legality, then snaps the piece to the nearest board cell.
    /// </summary>
    /// <param name="piece">The piece GameObject to place.</param>
    /// <param name="position">Target world-space position.</param>
    /// <returns><c>true</c> if the piece was successfully placed.</returns>
    public bool PlacePiece(GameObject piece, Vector3 position)
    {
        PieceManager.PieceType type = GetPieceType(piece);

        int pieceOwner = GetPiecePlayer(piece);
        if (pieceOwner != currentPlayer)
        {
            Debug.LogWarning($"Cannot place piece belonging to player {pieceOwner} on player {currentPlayer}'s turn.");
            return false;
        }

        if (!CanUsePiece(type, currentPlayer))
        {
            Debug.LogWarning("This piece has already been used.");
            return false;
        }

        if (!HasValidMoves(currentPlayer))
        {
            Debug.Log("Player has no valid moves — skipping turn.");
            SwitchPlayer();
            return false;
        }

        if (!IsValidMove(piece))
        {
            if (PiecePalette.Instance != null)
                PiecePalette.Instance.ResetPieceRotation(piece);
            return false;
        }

        List<Vector3> blockWorldPositions = BoardManager.Instance.GetPieceBlocksWorldPositions(piece);

        foreach (Vector3 blockPos in blockWorldPositions)
        {
            Vector2Int boardPos = WorldToBoardPosition(blockPos);
            occupiedSpaces[boardPos.x, boardPos.y] = currentPlayer;
        }

        Vector3 referencePosition = FindBestSnapReference(blockWorldPositions);
        Vector2Int snapPos = WorldToBoardPosition(referencePosition);
        Vector3 snappedPosition = BoardManager.Instance.BoardToWorldPosition(snapPos.x, snapPos.y);
        Vector3 offset = referencePosition - piece.transform.position;
        piece.transform.position = snappedPosition - offset;

        ConfigurePlacedPiece(piece);
        placedPieces.Add(piece);
        MarkPieceAsUsed(type, currentPlayer);
        PiecePalette.Instance.RemovePiece(type, currentPlayer);

        ScoreManager.Instance.PiecePlaced(currentPlayer, GetPieceType(piece));

        Debug.Log($"[PlacePiece] Piece placed by player {currentPlayer}. Switching player...");
        SwitchPlayer();

        ScoreUI.Instance.UpdatePiecesRemaining();
        return true;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private void ConfigurePlacedPiece(GameObject piece)
    {
        PieceDragger dragger = piece.GetComponent<PieceDragger>();
        if (dragger != null) { dragger.CleanUp(); Destroy(dragger); }

        PieceFlipper flipper = piece.GetComponent<PieceFlipper>();
        if (flipper != null) Destroy(flipper);

        piece.SetActive(true);
        Vector3 pos = piece.transform.position;
        piece.transform.position = new Vector3(pos.x, 0.1f, pos.z);

        int placedPieceLayer = LayerMask.NameToLayer("PlacedPieces");
        if (placedPieceLayer != -1)
        {
            piece.layer = placedPieceLayer;
            foreach (Transform child in piece.transform)
                child.gameObject.layer = placedPieceLayer;
        }
    }

    private PieceManager.PieceType GetPieceType(GameObject piece)
    {
        string pieceName = piece.name.Split('_')[0];
        return (PieceManager.PieceType)System.Enum.Parse(typeof(PieceManager.PieceType), pieceName);
    }

    private int GetPiecePlayer(GameObject piece)
    {
        string[] parts = piece.name.Split('_');
        if (parts.Length >= 2 && parts[1].StartsWith("Player"))
        {
            string playerStr = parts[1].Replace("Player", "");
            if (int.TryParse(playerStr, out int playerIndex))
                return playerIndex;
        }

        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Color pieceColor = renderer.material.color;
            for (int i = 0; i < playerColors.Length; i++)
            {
                if (ColorsAreSimilar(pieceColor, playerColors[i]))
                    return i;
            }
        }

        Debug.LogError($"Cannot determine player for piece: {piece.name}");
        return -1;
    }

    private bool ColorsAreSimilar(Color a, Color b, float threshold = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    private void SyncColorsFromSettings()
    {
        if (GameSettings.Instance == null) return;

        for (int i = 0; i < 2; i++)
        {
            Color c = GameSettings.Instance.GetPlayerColor(i);
            playerColors[i] = c;
            playerHighlightColors[i] = new Color(c.r, c.g, c.b, 0.7f);
        }

        if (BoardManager.Instance != null)
            BoardManager.Instance.HighlightStartingPositions();
    }

    private Vector3 FindBestSnapReference(List<Vector3> blockPositions)
    {
        Vector3 bestPosition = blockPositions[0];
        float minDistance = float.MaxValue;

        foreach (Vector3 pos in blockPositions)
        {
            Vector2Int boardPos = WorldToBoardPosition(pos);
            Vector3 tileCenter = BoardManager.Instance.BoardToWorldPosition(boardPos.x, boardPos.y);
            float distance = Vector3.Distance(pos, tileCenter);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestPosition = pos;
            }
        }
        return bestPosition;
    }

    private IEnumerator AITurnDelay()
    {
        yield return new WaitForSeconds(1f);
        AIController.Instance.MakeAIMove(currentPlayer);
    }

    private string L(string key) =>
        LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key) : key;
}