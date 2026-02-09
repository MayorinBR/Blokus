using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public const int BoardSize = 14;
    public GameObject tilePrefab;
    public float tileSize = 1.0f;

    [Header("Highlight Settings")]
    [SerializeField] private bool _enableHighlight;
    public bool enableHighlight => _enableHighlight;

    public GameObject[,] tiles = new GameObject[BoardSize, BoardSize];

    void Awake()
    {
        Instance = this;
        GenerateBoard();
    }

    void GenerateBoard()
    {
        int boardLayer = LayerMask.NameToLayer("Board");

        // Primeiro, gere todas as tiles normalmente
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

                // Assign the tile to the array before trying to access it
                tiles[x, y] = tile;

                // Now it's safe to access the tile's renderer
                Renderer renderer = tile.GetComponent<Renderer>();
                renderer.material.color = (x + y) % 2 == 0 ? Color.white : Color.gray;

                if (boardLayer != -1)
                    tile.layer = boardLayer;
            }
        }

        // Depois, destaque as posições iniciais dos jogadores
        HighlightStartingPositions();
    }

    public void HighlightValidPositions(GameObject piece)
    {
        if (!enableHighlight) // Verifica se o highlight está habilitado
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

                // Obtém as posições dos blocos considerando rotação
                List<Vector3> blockWorldPositions = GetPieceBlocksWorldPositions(piece);

                // Ajusta para a posição do tabuleiro que estamos testando
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
                    // Verificação especial para primeiro movimento
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
                        // Verificação de adjacência para movimentos subsequentes
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

    private void HighlightStartingPositions()
    {
        for (int i = 0; i < GameManager.Instance.startPositions.Length; i++)
        {
            Vector2Int pos = GameManager.Instance.startPositions[i];
            if (pos.x >= 0 && pos.x < BoardSize && pos.y >= 0 && pos.y < BoardSize)
            {
                // Usa a cor de highlight para as posições iniciais
                Color highlightColor = GameManager.Instance.playerHighlightColors[i];
                tiles[pos.x, pos.y].GetComponent<Renderer>().material.color = highlightColor;
            }
        }
    }

    public List<Vector3> GetPieceBlocksWorldPositions(GameObject piece)
    {
        List<Vector3> blockPositions = new List<Vector3>();

        foreach (Transform child in piece.transform)
        {
            if (!child.name.Contains("Collider"))
            {
                // Considera a rotação da peça
                Vector3 rotatedPosition = piece.transform.rotation * child.localPosition;
                blockPositions.Add(piece.transform.position + rotatedPosition);
            }
        }

        return blockPositions;
    }

    public void ClearHighlights()
    {
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                // Pula as posições iniciais
                bool isStartingPos = false;
                foreach (Vector2Int pos in GameManager.Instance.startPositions)
                {
                    if (pos.x == x && pos.y == y)
                    {
                        isStartingPos = true;
                        break;
                    }
                }

                if (!isStartingPos)
                {
                    Renderer rend = tiles[x, y].GetComponent<Renderer>();
                    rend.material.color = (x + y) % 2 == 0 ? Color.white : Color.gray;
                }
            }
        }
    }

    public Vector3 BoardToWorldPosition(int x, int y)
    {
        return new Vector3(
            x * tileSize - (BoardSize * tileSize) / 2 + tileSize / 2,
            0,
            y * tileSize - (BoardSize * tileSize) / 2 + tileSize / 2
        );
    }
}
