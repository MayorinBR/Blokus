using System.Collections.Generic;
using UnityEngine;
using static PieceManager;

/// <summary>
/// Manages the visual piece palettes displayed on either side of the board.
/// Pieces are laid out in a predictable grid where every piece is centered
/// within a fixed-size cell, regardless of its shape or block count.
/// </summary>
public class PiecePalette : MonoBehaviour
{
    public static PiecePalette Instance;

    /// <summary>Remaining unplaced pieces for Player 1, keyed by piece type.</summary>
    public Dictionary<PieceType, GameObject> player1Pieces = new Dictionary<PieceType, GameObject>();

    /// <summary>Remaining unplaced pieces for Player 2, keyed by piece type.</summary>
    public Dictionary<PieceType, GameObject> player2Pieces = new Dictionary<PieceType, GameObject>();

    private readonly Dictionary<PieceType, Quaternion> originalRotations = new Dictionary<PieceType, Quaternion>();
    private GameObject selectedPiece;

    [Header("Scale")]
    [Tooltip("Uniform scale applied to palette pieces.")]
    [SerializeField] public float pieceScale = 0.6f;

    /// <summary>Uniform scale applied to all palette pieces.</summary>
    public float PieceScale => pieceScale;

    [Header("Grid Layout")]
    [Tooltip("Maximum pieces per column before wrapping to a new column.")]
    [SerializeField] private int maxRows = 7;

    [Tooltip(
        "World-space size of each grid cell. " +
        "Rule of thumb: blockSize * 5 + a small gap. " +
        "blockSize ≈ tileSize * 0.82 * pieceScale, so with tileSize=1 and pieceScale=0.6 this is ~2.7.")]
    [SerializeField] private float cellSize = 2.8f;

    [Header("Position")]
    [Tooltip("Extra world-space gap between the board edge and the first palette column.")]
    [SerializeField] private float horizontalOffset = 1.5f;

    [Tooltip("World-space Z coordinate of the top row.")]
    [SerializeField] private float verticalStart = 6f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    /// <summary>
    /// Instantiates and positions all pieces for both players in a predictable
    /// column/row grid on either side of the board.
    /// Each piece is centered inside its cell regardless of shape.
    /// Call at game start and after <see cref="ClearAll"/> on restart.
    /// </summary>
    public void DisplayAllPieces()
    {
        float boardHalfWidth = BoardManager.BoardSize * BoardManager.Instance.tileSize / 2f;
        float leftEdgeX = -boardHalfWidth - horizontalOffset;
        float rightEdgeX = boardHalfWidth + horizontalOffset;

        int row = 0;
        int col = 0;

        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            float cellCenterZ = verticalStart - row * cellSize;

            Vector3 leftTarget = new Vector3(leftEdgeX - col * cellSize, 0f, cellCenterZ);
            Vector3 rightTarget = new Vector3(rightEdgeX + col * cellSize, 0f, cellCenterZ);

            GameObject leftPiece = CreateAndScalePiece(type, 0);
            CenterPieceAt(leftPiece, leftTarget);
            player1Pieces[type] = leftPiece;

            GameObject rightPiece = CreateAndScalePiece(type, 1);
            CenterPieceAt(rightPiece, rightTarget);
            player2Pieces[type] = rightPiece;

            row++;
            if (row >= maxRows) { row = 0; col++; }
        }

        ScoreUI.Instance?.UpdatePiecesRemaining();
    }

    /// <summary>
    /// Removes a piece from the palette after it has been placed on the board.
    /// Destroys the AI player's copy in single-player mode.
    /// </summary>
    /// <param name="type">Type of the placed piece.</param>
    /// <param name="playerIndex">Zero-based index of the owning player.</param>
    public void RemovePiece(PieceType type, int playerIndex)
    {
        bool isPvP = GameSettings.Instance != null && GameSettings.Instance.isPvP;

        if (playerIndex == 0 && player1Pieces.ContainsKey(type))
        {
            player1Pieces.Remove(type);
        }
        else if (playerIndex == 1 && player2Pieces.ContainsKey(type))
        {
            if (!isPvP)
                Destroy(player2Pieces[type]);

            player2Pieces.Remove(type);
        }
    }

    /// <summary>
    /// Marks a piece as selected: scales it to full size and resets its rotation.
    /// </summary>
    /// <param name="piece">The piece GameObject picked up.</param>
    public void PieceSelected(GameObject piece)
    {
        selectedPiece = piece;
        piece.transform.localScale = Vector3.one;

        PieceFlipper flipper = piece.GetComponent<PieceFlipper>();
        if (flipper != null)
            flipper.ResetToDefaultRotation();
        else
            piece.transform.rotation = Quaternion.Euler(0, 90, 0);
    }

    /// <summary>
    /// Restores a piece's rotation to its original palette orientation.
    /// Used when placement is cancelled and the piece snaps back.
    /// </summary>
    /// <param name="piece">The piece to reset.</param>
    public void ResetPieceRotation(GameObject piece)
    {
        foreach (PieceType type in originalRotations.Keys)
        {
            if (piece.name.StartsWith(type.ToString()))
            {
                piece.transform.rotation = originalRotations[type];
                break;
            }
        }
    }

    /// <summary>
    /// Destroys all palette piece GameObjects and clears both piece dictionaries.
    /// Must be called before <see cref="DisplayAllPieces"/> on game restart.
    /// </summary>
    public void ClearAll()
    {
        foreach (GameObject piece in player1Pieces.Values)
            if (piece != null) Destroy(piece);

        foreach (GameObject piece in player2Pieces.Values)
            if (piece != null) Destroy(piece);

        player1Pieces.Clear();
        player2Pieces.Clear();
    }

    /// <summary>
    /// Returns the piece types still available for the given player.
    /// </summary>
    /// <param name="playerIndex">Zero-based player index.</param>
    /// <returns>List of remaining piece types.</returns>
    public List<PieceManager.PieceType> GetAvailablePiecesForPlayer(int playerIndex)
    {
        return playerIndex == 0
            ? new List<PieceManager.PieceType>(player1Pieces.Keys)
            : new List<PieceManager.PieceType>(player2Pieces.Keys);
    }

    // ── Private Helpers ────────────────────────────────────────────────────────

    private GameObject CreateAndScalePiece(PieceType type, int playerIndex)
    {
        GameObject piece = PieceManager.Instance.CreatePiece(type, playerIndex);
        piece.transform.localScale = Vector3.one * pieceScale;
        piece.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        return piece;
    }

    /// <summary>
    /// Repositions a piece so that the world-space centroid of its blocks
    /// lands exactly on <paramref name="targetCenter"/>.
    /// Works for any piece shape and rotation because it samples actual
    /// world positions rather than relying on the shape definition.
    /// </summary>
    /// <param name="piece">The piece to reposition.</param>
    /// <param name="targetCenter">Desired world-space center.</param>
    private void CenterPieceAt(GameObject piece, Vector3 targetCenter)
    {
        piece.transform.position = targetCenter;

        Vector3 centroid = GetBlockCentroid(piece);
        if (centroid == Vector3.zero) return;

        piece.transform.position += targetCenter - centroid;
    }

    /// <summary>
    /// Computes the average world-space position of all non-collider block children.
    /// Returns <see cref="Vector3.zero"/> if the piece has no visible blocks.
    /// </summary>
    /// <param name="piece">The piece to sample.</param>
    /// <returns>World-space centroid of the piece's blocks.</returns>
    private Vector3 GetBlockCentroid(GameObject piece)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Transform child in piece.transform)
        {
            if (child.name.Contains("Collider")) continue;
            sum += child.position;
            count++;
        }

        return count > 0 ? sum / count : Vector3.zero;
    }
}