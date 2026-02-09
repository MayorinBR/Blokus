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

    // Substitua o array startCorners por:
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
        // Initialize scores first
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeScores();
        }

        // Then update UI
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

        // Limpa o tabuleiro
        for (int x = 0; x < BoardManager.BoardSize; x++)
        {
            for (int y = 0; y < BoardManager.BoardSize; y++)
            {
                occupiedSpaces[x, y] = -1;
            }
        }

        // Reseta os jogadores
        currentPlayer = 0;

        // Atualiza a UI
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UpdateTurnUI();
        }

        // Reseta o score (se necessário)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitializeScores();
        }

        ScoreUI.Instance.gameOverPanel.SetActive(false);
        ScoreUI.Instance.postGameOverPanel.SetActive(false);

        // Reiniciar a paleta de peças
        PiecePalette.Instance.DisplayAllPieces();

        // Atualizar UI
        ScoreUI.Instance.UpdatePiecesRemaining();
    }

    public bool IsValidMove(GameObject piece)
    {
        List<Vector3> blockWorldPositions = BoardManager.Instance.GetPieceBlocksWorldPositions(piece);
        bool hasAdjacentCorner = false;
        bool hasAdjacentSide = false;

        foreach (Vector3 blockPos in blockWorldPositions)
        {
            Vector2Int boardPos = WorldToBoardPosition(blockPos);

            // Verificações básicas (dentro do tabuleiro e espaço não ocupado)
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

        // Verificação especial para o primeiro movimento
        if (IsFirstMove(currentPlayer))
        {
            Vector2Int firstBlockPos = WorldToBoardPosition(blockWorldPositions[0]);
            if (!IsInStartingCorner(firstBlockPos, currentPlayer))
            {
                // Para o primeiro movimento, pelo menos um bloco deve estar na posição inicial
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
            // Para movimentos subsequentes, verifica as regras de adjacência
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
        // Salva o jogador atual para restaurar depois
        int originalPlayer = currentPlayer;
        currentPlayer = player;

        // Verifica todas as peças do jogador
        foreach (PieceManager.PieceType type in PiecePalette.Instance.GetAvailablePiecesForPlayer(player))
        {
            // Cria uma peça temporária para teste
            GameObject testPiece = PieceManager.Instance.CreatePiece(type, player);
            testPiece.SetActive(false); // Esconde a peça de teste

            // Testa todas as posições possíveis no tabuleiro
            for (int x = 0; x < BoardManager.BoardSize; x++)
            {
                for (int y = 0; y < BoardManager.BoardSize; y++)
                {
                    Vector3 testPosition = BoardManager.Instance.BoardToWorldPosition(x, y);
                    testPiece.transform.position = testPosition;

                    // Testa todas as rotações possíveis (0, 90, 180, 270 graus)
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
        // Verifica se é o primeiro movimento do jogador
        for (int x = 0; x < BoardManager.BoardSize; x++)
        {
            for (int y = 0; y < BoardManager.BoardSize; y++)
            {
                if (occupiedSpaces[x, y] == player) // Verifica se há peça do jogador
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void SwitchPlayer()
    {
        int originalPlayer = currentPlayer;
        int nextPlayer = (currentPlayer + 1) % 2;
        int playersChecked = 0;

        while (playersChecked < 2)
        {
            // Verifica se o próximo jogador tem movimentos válidos
            bool hasValidMoves = HasValidMoves(nextPlayer);

            // Verifica se é a AI e se ela tem peças disponíveis
            bool isAI = (nextPlayer == 1 && AIController.Instance != null && AIController.Instance.isActive);
            bool AIHasPieces = isAI ? (AIController.Instance.GetAvailablePieces(1).Count > 0) : true;

            if (hasValidMoves && AIHasPieces)
            {
                currentPlayer = nextPlayer;
                Debug.Log($"Turno do Jogador {currentPlayer + 1}");

                if (TurnUI.Instance != null)
                {
                    TurnUI.Instance.UpdateTurnUI(currentPlayer);
                }

                // Se for o turno da AI
                if (isAI)
                {
                    StartCoroutine(AITurnDelay());
                }

                return;
            }
            else if (isAI && !hasValidMoves)
            {
                Debug.Log($"AI (Jogador {nextPlayer + 1}) não tem movimentos válidos. Pulando turno.");
            }

            nextPlayer = (nextPlayer + 1) % 2;
            playersChecked++;
        }

        // Se chegou aqui, nenhum jogador tem movimentos válidos
        GameOver();
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Nenhum jogador pode colocar mais peças.");

        if (AIController.Instance != null && AIController.Instance.GetAvailablePieces(1).Count == 0)
        {
            Debug.Log("AI ficou sem peças disponíveis!");
        }

        int[] scores = {
        ScoreManager.Instance.GetPlayerScore(0),
        ScoreManager.Instance.GetPlayerScore(1)
    };

        ScoreUI.Instance.UpdateScores(scores);

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

        // Mostrar painel de game over
        ScoreUI.Instance.gameOverPanel.SetActive(true);

        // Agendar para mostrar a tela de pós-game over após 3 segundos
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
        // Verifica os 8 espaços ao redor
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
                        if (Mathf.Abs(x) + Mathf.Abs(y) == 1) // Lado adjacente
                        {
                            hasAdjacentSide = true;
                        }
                        else if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1) // Canto adjacente
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

        if (!CanUsePiece(type, currentPlayer))
        {
            Debug.LogWarning("Esta peça já foi usada!");
            return false;
        }

        // Verifica se o jogador atual ainda tem movimentos válidos
        if (!HasValidMoves(currentPlayer))
        {
            Debug.Log("Jogador não tem movimentos válidos - pulando turno");
            SwitchPlayer();
            return false;
        }

        // 1. Verify the move is valid
        if (!IsValidMove(piece))
        {
            if (PiecePalette.Instance != null)
            {
                PiecePalette.Instance.ResetPieceRotation(piece);
            }
            return false;
        }

        // 2. Get all block positions
        List<Vector3> blockWorldPositions = BoardManager.Instance.GetPieceBlocksWorldPositions(piece);

        // 3. Mark all occupied spaces
        foreach (Vector3 blockPos in blockWorldPositions)
        {
            Vector2Int boardPos = WorldToBoardPosition(blockPos);
            occupiedSpaces[boardPos.x, boardPos.y] = currentPlayer;
        }

        // 4. Find the best block for snapping
        Vector3 referencePosition = FindBestSnapReference(blockWorldPositions);
        Vector2Int snapPos = WorldToBoardPosition(referencePosition);

        // 5. Calculate perfect snap position
        Vector3 snappedPosition = BoardManager.Instance.BoardToWorldPosition(snapPos.x, snapPos.y);
        Vector3 offset = referencePosition - piece.transform.position;
        piece.transform.position = snappedPosition - offset;

        // 6. Configura a peça colocada corretamente
        ConfigurePlacedPiece(piece);
        MarkPieceAsUsed(type, currentPlayer);
        PiecePalette.Instance.RemovePiece(type, currentPlayer);

        // 7. Remove a peça da paleta (mas não destrói a peça colocada)
        if (PiecePalette.Instance != null)
        {
            PiecePalette.Instance.RemovePiece(GetPieceType(piece), currentPlayer);
        }

        ScoreManager.Instance.PiecePlaced(currentPlayer, GetPieceType(piece));

        // Switch player after successful placement
        SwitchPlayer();

        // Safely update turn UI if TurnManager exists
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UpdateTurnUI();
        }

        ScoreUI.Instance.UpdatePiecesRemaining();

        return true;
    }

    private void ConfigurePlacedPiece(GameObject piece)
    {
        // Remove os componentes de interação
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

        // Configurações visuais importantes:
        // 1. Garante que a peça está ativa
        piece.SetActive(true);

        // 2. Ajusta a posição Y para ficar acima do tabuleiro
        Vector3 pos = piece.transform.position;
        piece.transform.position = new Vector3(pos.x, 0.1f, pos.z);

        // 3. Configura a layer correta
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
        // Extrai o nome do tipo da peça do nome do GameObject
        string pieceName = piece.name.Split('_')[0];
        return (PieceManager.PieceType)System.Enum.Parse(typeof(PieceManager.PieceType), pieceName);
    }

    private Vector3 FindBestSnapReference(List<Vector3> blockPositions)
    {
        // Find the block closest to a tile center for perfect alignment
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

    private Vector3 GetFirstBlockLocalPosition(GameObject piece)
    {
        foreach (Transform child in piece.transform)
        {
            if (!child.name.Contains("Collider"))
            {
                return child.localPosition;
            }
        }
        return Vector3.zero;
    }

    private IEnumerator AITurnDelay()
    {
        // Pequeno delay antes da AI jogar
        yield return new WaitForSeconds(1f);
        AIController.Instance.MakeAIMove(currentPlayer);
    }

    public Color[] playerHighlightColors = new Color[4]
{
    new Color(1f, 0f, 0f, 0.7f), // Light red (para posição inicial)
    new Color(0f, 0f, 1f, 0.7f), // Light blue (para posição inicial)
    new Color(0f, 1f, 0f, 0.7f), // Light green (para posição inicial)
    new Color(1f, 1f, 0f, 0.7f)    // Light yellow (para posição inicial)
};
}