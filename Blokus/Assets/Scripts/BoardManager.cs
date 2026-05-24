using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and manages the game board. Responsible for creating tile GameObjects,
/// highlighting valid positions during piece placement, and resetting tile colors.
/// </summary>
[DefaultExecutionOrder(-100)]
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    /// <summary>Width and height of the board in tiles.</summary>
    public const int BoardSize = 14;

    [Tooltip("Prefab used to instantiate each board tile.")]
    public GameObject tilePrefab;

    [Tooltip("World-space size of each tile in units.")]
    public float tileSize = 1.0f;

    [Header("Highlight Settings")]
    [SerializeField] private bool _enableHighlight;

    /// <summary>Whether valid-placement highlights are shown while dragging a piece.</summary>
    public bool enableHighlight => _enableHighlight;

    /// <summary>Grid of instantiated tile GameObjects, indexed by [x, y].</summary>
    public GameObject[,] tiles = new GameObject[BoardSize, BoardSize];

    private void Awake()
    {
        Instance = this;
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        int boardLayer = LayerMask.NameToLayer("Board");

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Vector3 position = new Vector3(
                    x * tileSize - (BoardSize * tileSize) / 2 + tileSize / 2,
                    0,
                    y * tileSize - (BoardSize * tileSize) / 2 + tileSize / 2
                );

                GameObject tile = Instantiate(tilePrefab, position, Quaternion.Euler(90, 0, 0), transform);
                tile.name = $"Tile_{x}_{y}";
                tiles[x, y] = tile;

                Renderer renderer = tile.GetComponent<Renderer>();
                renderer.material.color = (x + y) % 2 == 0 ? Color.white : Color.gray;

                if (boardLayer != -1)
                    tile.layer = boardLayer;
            }
        }

        HighlightStartingPositions();
    }

    /// <summary>
    /// Highlights tiles where the given piece can legally be placed.
    /// Has no effect if <see cref="enableHighlight"/> is false.
    /// </summary>
    /// <param name="piece">The piece currently being dragged.</param>
    public void HighlightValidPositions(GameObject piece)
    {
        if (!enableHighlight)
        {
            ClearHighlights();
            return;
        }

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Vector3 boardCenterPos = BoardToWorldPosition(x, y);
                bool canPlace = true;

                List<Vector3> blockWorldPositions = GetPieceBlocksWorldPositions(piece);
                Vector3 offset = boardCenterPos - piece.transform.position;

                foreach (Vector3 blockPos in blockWorldPositions)
                {
                    Vector3 testPos = blockPos + offset;
                    Vector2Int boardPos = GameManager.Instance.WorldToBoardPosition(testPos);

                    if (boardPos.x < 0 || boardPos.x >= BoardSize ||
                        boardPos.y < 0 || boardPos.y >= BoardSize ||
                        GameManager.Instance.occupiedSpaces[boardPos.x, boardPos.y] != 0)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    if (GameManager.Instance.IsFirstMove(GameManager.Instance.currentPlayer))
                    {
                        Vector2Int startPos = GameManager.Instance.startPositions[GameManager.Instance.currentPlayer];
                        if (x == startPos.x && y == startPos.y)
                        {
                            tiles[x, y].GetComponent<Renderer>().material.color =
                                GameManager.Instance.playerColors[GameManager.Instance.currentPlayer];
                        }
                    }
                    else
                    {
                        bool hasAdjacentCorner = false;
                        bool hasAdjacentSide = false;
                        GameManager.Instance.CheckAdjacentSpaces(new Vector2Int(x, y),
                            ref hasAdjacentCorner, ref hasAdjacentSide);

                        if (hasAdjacentCorner && !hasAdjacentSide)
                        {
                            tiles[x, y].GetComponent<Renderer>().material.color =
                                GameManager.Instance.playerColors[GameManager.Instance.currentPlayer];
                        }
                    }
                }
            }
        }
    }

    public void HighlightStartingPositions()
    {
        if (GameManager.Instance == null) return;

        for (int i = 0; i < GameManager.Instance.startPositions.Length; i++)
        {
            Vector2Int pos = GameManager.Instance.startPositions[i];
            if (pos.x >= 0 && pos.x < BoardSize && pos.y >= 0 && pos.y < BoardSize)
            {
                tiles[pos.x, pos.y].GetComponent<Renderer>().material.color =
                    GameManager.Instance.playerHighlightColors[i];
            }
        }
    }

    /// <summary>
    /// Returns the world-space positions of every block in the given piece,
    /// accounting for the piece's current rotation.
    /// </summary>
    /// <param name="piece">The piece GameObject to sample.</param>
    /// <returns>List of world-space block positions.</returns>
    public List<Vector3> GetPieceBlocksWorldPositions(GameObject piece)
    {
        var blockPositions = new List<Vector3>();

        foreach (Transform child in piece.transform)
        {
            if (!child.name.Contains("Collider"))
            {
                Vector3 rotatedPosition = piece.transform.rotation * child.localPosition;
                blockPositions.Add(piece.transform.position + rotatedPosition);
            }
        }

        return blockPositions;
    }

    /// <summary>
    /// Resets all tile colors to their default checkerboard pattern,
    /// preserving the highlight on player starting positions.
    /// </summary>
    public void ClearHighlights()
    {
        var startingPositions = new HashSet<Vector2Int>(GameManager.Instance.startPositions);

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                if (startingPositions.Contains(new Vector2Int(x, y))) continue;

                Renderer rend = tiles[x, y].GetComponent<Renderer>();
                rend.material.color = (x + y) % 2 == 0 ? Color.white : Color.gray;
            }
        }
    }

    /// <summary>
    /// Converts a board grid coordinate to its corresponding world-space center position.
    /// </summary>
    /// <param name="x">Column index (0-based).</param>
    /// <param name="y">Row index (0-based).</param>
    /// <returns>World-space center of the tile at (x, y).</returns>
    public Vector3 BoardToWorldPosition(int x, int y)
    {
        return new Vector3(
            x * tileSize - (BoardSize * tileSize) / 2 + tileSize / 2,
            0,
            y * tileSize - (BoardSize * tileSize) / 2 + tileSize / 2
        );
    }
}