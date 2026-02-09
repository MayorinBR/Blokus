using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentPlayer = 0; // 0-3 para os 4 jogadores
    public int[,] occupiedSpaces = new int[BoardManager.BoardSize, BoardManager.BoardSize]; // -1 = vazio, 0-3 = jogadores

    public List<PieceManager.PieceType>[] usedPieces;

    public Color[] playerColors = new Color[4] {
    new Color(1.0f, 0.0f, 0.0f),      // Vermelho mais escuro (Jogador 1)
    new Color(0.0f, 0.0f, 1.0f),      // Azul mais escuro (Jogador 2)
    new Color(0.0f, 1.0f, 0.0f),      // Verde mais escuro (Jogador 3)
    new Color(1.0f, 1.0f, 0.0f)       // Amarelo mais escuro (Jogador 4)
};

    public Vector2Int[] startPositions = new Vector2Int[4]
    {
    new Vector2Int(4, 4),   // Jogador 0 - Centro Superior Esquerdo
    new Vector2Int(9, 9),   // Jogador 1 - Centro Inferior Direito
    new Vector2Int(4, 9),   // Jogador 2 - Centro Superior Direito
    new Vector2Int(9, 4)    // Jogador 3 - Centro Inferior Esquerdo
    };

    void Awake()
    {
        usedPieces = new List<PieceManager.PieceType>[2];
        for (int i = 0; i < usedPieces.Length; i++)
        {
            usedPieces[i] = new List<PieceManager.PieceType>();
        }

        if (Instance == null)
        {
            Instance = this;
            // Inicializa o tabuleiro como vazio (-1)
            for (int x = 0; x < BoardManager.BoardSize; x++)
            {
                for (int y = 0; y < BoardManager.BoardSize; y++)
                {
                    occupiedSpaces[x, y] = -1;
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeScores();
        }

        if (TurnUI.Instance != null)
        {
            TurnUI.Instance.UpdateTurnUI(currentPlayer);
        }

        if (ScoreUI.Instance != null)
        {
            ScoreUI.Instance.UpdatePiecesRemaining();
        }

        if (PiecePalette.Instance != null)
        {
            PiecePalette.Instance.DisplayAllPieces();
        }

        Debug.Log($"[GameManager] Jogo iniciado - Jogador atual: {currentPlayer}");
    }

    public bool CanUsePiece(PieceManager.PieceType type, int playerIndex)
    {
        return !usedPieces[playerIndex].Contains(type);
    }

    public void MarkPieceAsUsed(PieceManager.PieceType type, int playerIndex)
    {
        if (!usedPieces[playerIndex].Contains(type))
        {
            usedPieces[playerIndex].Add(type);
            Debug.Log($"Peça {type} marcada como usada pelo jogador {playerIndex}");
        }
    }

    public void ResetGame()
    {
        for (int i = 0; i < usedPieces.Length; i++)
        {
            usedPieces[i].Clear();
        }

        for (int x = 0; x < BoardManager.BoardSize; x++)
        {
            for (int y = 0; y < BoardManager.BoardSize; y++)
            {
                occupiedSpaces[x, y] = -1;
            }
        }

        currentPlayer = 0;

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UpdateTurnUI();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeScores();
        }

        ScoreUI.Instance.gameOverPanel.SetActive(false);
        ScoreUI.Instance.postGameOverPanel.SetActive(false);
        PiecePalette.Instance.DisplayAllPieces();
        ScoreUI.Instance.UpdatePiecesRemaining();
    }

    public bool IsValidMove(GameObject piece)
    {
        // VALIDAÇÃO CRÍTICA: Verifica se a peça pertence ao jogador atual
        int pieceOwner = GetPiecePlayer(piece);
        if (pieceOwner != currentPlayer)
        {
            Debug.LogWarning($"Peça do jogador {pieceOwner} não pode ser movida no turno do jogador {currentPlayer}");
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
            {
                return false;
            }

            if (occupiedSpaces[boardPos.x, boardPos.y] != -1)
            {
                return false;
            }
        }

        if (IsFirstMove(currentPlayer))
        {
            Vector2Int firstBlockPos = WorldToBoardPosition(blockWorldPositions[0]);
            if (!IsInStartingCorner(firstBlockPos, currentPlayer))
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

                if (!anyBlockInStartPos)
                {
                    return false;
                }
            }
        }
        else
        {
            foreach (Vector3 blockPos in blockWorldPositions)
            {
                Vector2Int boardPos = WorldToBoardPosition(blockPos);
                CheckAdjacentSpaces(boardPos, ref hasAdjacentCorner, ref hasAdjacentSide);
            }

            if (!hasAdjacentCorner || hasAdjacentSide)
            {
                return false;
            }
        }

        return true;
    }

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

    public bool IsFirstMove(int player)
    {
        return usedPieces[player].Count == 0;
    }

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
                Debug.Log($"Jogador {currentPlayer} não tem movimentos válidos");
                currentPlayer = (currentPlayer + 1) % 2;

                if (!HasValidMoves(currentPlayer))
                {
                    EndGame();
                    return;
                }
            }

            if (currentPlayer == 1 && !isPvP)
            {
                StartCoroutine(AITurnDelay());
            }
        }

        Debug.Log($"[SwitchPlayer] Mudou de jogador {previousPlayer} para jogador {currentPlayer}");

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UpdateTurnUI();
            Debug.Log($"[SwitchPlayer] TurnManager.UpdateTurnUI() chamado");
        }
        else
        {
            Debug.LogWarning("[SwitchPlayer] TurnManager.Instance é null!");
        }

        if (TurnUI.Instance != null)
        {
            TurnUI.Instance.UpdateTurnUI(currentPlayer);
            Debug.Log($"[SwitchPlayer] TurnUI.UpdateTurnUI({currentPlayer}) chamado diretamente");
        }
        else
        {
            Debug.LogWarning("[SwitchPlayer] TurnUI.Instance é null!");
        }
    }

    public bool IsGameOver()
    {
        return !HasValidMoves(0) && !HasValidMoves(1);
    }

    public void EndGame()
    {
        Debug.Log("Jogo encerrado!");

        int[] scores = new int[2];
        scores[0] = ScoreManager.Instance.GetPlayerScore(0);
        scores[1] = ScoreManager.Instance.GetPlayerScore(1);

        if (scores[0] > scores[1])
        {
            Debug.Log("Jogador 1 venceu!");
        }
        else if (scores[1] > scores[0])
        {
            Debug.Log("Jogador 2 venceu!");
        }
        else
        {
            Debug.Log("Empate!");
        }

        ScoreUI.Instance.gameOverPanel.SetActive(true);
        Invoke("ShowPostGameOverScreen", 3f);
    }

    private void ShowPostGameOverScreen()
    {
        ScoreUI.Instance.gameOverPanel.SetActive(false);
        ScoreUI.Instance.postGameOverPanel.SetActive(true);
    }

    private bool IsInStartingCorner(Vector2Int boardPos, int player)
    {
        Vector2Int startPos = startPositions[player];
        return (boardPos.x == startPos.x && boardPos.y == startPos.y);
    }

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
                    // REGRA DO BLOKUS: Só considera adjacência de peças DA MESMA COR
                    if (occupiedSpaces[checkX, checkY] == currentPlayer)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) == 1)
                        {
                            hasAdjacentSide = true;
                        }
                        else if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                        {
                            hasAdjacentCorner = true;
                        }
                    }
                }
            }
        }
    }

    public Vector2Int WorldToBoardPosition(Vector3 worldPosition)
    {
        float tileSize = BoardManager.Instance.tileSize;
        float halfBoard = BoardManager.BoardSize / 2f;

        int x = Mathf.FloorToInt((worldPosition.x + halfBoard * tileSize) / tileSize);
        int y = Mathf.FloorToInt((worldPosition.z + halfBoard * tileSize) / tileSize);

        return new Vector2Int(x, y);
    }

    public bool PlacePiece(GameObject piece, Vector3 position)
    {
        PieceManager.PieceType type = GetPieceType(piece);

        int pieceOwner = GetPiecePlayer(piece);
        if (pieceOwner != currentPlayer)
        {
            Debug.LogWarning($"Não pode colocar peça do jogador {pieceOwner} no turno do jogador {currentPlayer}");
            return false;
        }

        if (!CanUsePiece(type, currentPlayer))
        {
            Debug.LogWarning("Esta peça já foi usada!");
            return false;
        }

        if (!HasValidMoves(currentPlayer))
        {
            Debug.Log("Jogador não tem movimentos válidos - pulando turno");
            SwitchPlayer();
            return false;
        }

        if (!IsValidMove(piece))
        {
            if (PiecePalette.Instance != null)
            {
                PiecePalette.Instance.ResetPieceRotation(piece);
            }
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
        MarkPieceAsUsed(type, currentPlayer);
        PiecePalette.Instance.RemovePiece(type, currentPlayer);

        if (PiecePalette.Instance != null)
        {
            PiecePalette.Instance.RemovePiece(GetPieceType(piece), currentPlayer);
        }

        ScoreManager.Instance.PiecePlaced(currentPlayer, GetPieceType(piece));

        Debug.Log($"[PlacePiece] Peça colocada pelo jogador {currentPlayer}. Chamando SwitchPlayer()...");
        SwitchPlayer();

        ScoreUI.Instance.UpdatePiecesRemaining();

        return true;
    }

    private void ConfigurePlacedPiece(GameObject piece)
    {
        PieceDragger dragger = piece.GetComponent<PieceDragger>();
        if (dragger != null)
        {
            dragger.CleanUp();
            Destroy(dragger);
        }

        PieceFlipper flipper = piece.GetComponent<PieceFlipper>();
        if (flipper != null)
        {
            Destroy(flipper);
        }

        piece.SetActive(true);
        Vector3 pos = piece.transform.position;
        piece.transform.position = new Vector3(pos.x, 0.1f, pos.z);

        int placedPieceLayer = LayerMask.NameToLayer("PlacedPieces");
        if (placedPieceLayer != -1)
        {
            piece.layer = placedPieceLayer;
            foreach (Transform child in piece.transform)
            {
                child.gameObject.layer = placedPieceLayer;
            }
        }
    }

    private PieceManager.PieceType GetPieceType(GameObject piece)
    {
        string pieceName = piece.name.Split('_')[0];
        return (PieceManager.PieceType)System.Enum.Parse(typeof(PieceManager.PieceType), pieceName);
    }

    // FUNÇÃO CRÍTICA: Extrai o índice do jogador do nome da peça
    private int GetPiecePlayer(GameObject piece)
    {
        // Nome da peça é "I5_Player0" ou "T4_Player1"
        string[] parts = piece.name.Split('_');
        if (parts.Length >= 2 && parts[1].StartsWith("Player"))
        {
            string playerStr = parts[1].Replace("Player", "");
            if (int.TryParse(playerStr, out int playerIndex))
            {
                return playerIndex;
            }
        }

        // Fallback: determina pela cor
        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Color pieceColor = renderer.material.color;
            for (int i = 0; i < playerColors.Length; i++)
            {
                if (ColorsAreSimilar(pieceColor, playerColors[i]))
                {
                    return i;
                }
            }
        }

        Debug.LogError($"Impossível determinar jogador da peça: {piece.name}");
        return -1;
    }

    private bool ColorsAreSimilar(Color a, Color b, float threshold = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
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

    public Color[] playerHighlightColors = new Color[4]
    {
        new Color(1f, 0f, 0f, 0.7f),
        new Color(0f, 0f, 1f, 0.7f),
        new Color(0f, 1f, 0f, 0.7f),
        new Color(1f, 1f, 0f, 0.7f)
    };
}