using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : MonoBehaviour
{
    public static AIController Instance;
    public bool isActive = true;
    public float moveDelay;

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

    private void Start()
    {
        if (GameSettings.Instance != null)
        {
            moveDelay = GameSettings.Instance.aiDelay;
        }
    }

    public List<PieceManager.PieceType> GetAvailablePieces(int playerIndex)
    {
        List<PieceManager.PieceType> availablePieces = new List<PieceManager.PieceType>();

        foreach (PieceManager.PieceType type in System.Enum.GetValues(typeof(PieceManager.PieceType)))
        {
            if (GameManager.Instance.CanUsePiece(type, playerIndex))
            {
                availablePieces.Add(type);
            }
        }

        return availablePieces;
    }

    public void MakeAIMove(int aiPlayerIndex)
    {
        if (!isActive || GetAvailablePieces(aiPlayerIndex).Count == 0)
        {
            GameManager.Instance.SwitchPlayer();
            return;
        }

        StartCoroutine(AIMoveRoutine(aiPlayerIndex));
    }

    private IEnumerator AIMoveRoutine(int aiPlayerIndex)
    {
        yield return new WaitForSeconds(moveDelay);

        List<PieceManager.PieceType> availablePieces = GetAvailablePieces(aiPlayerIndex);

        if (availablePieces.Count == 0)
        {
            Debug.Log("AI não tem mais peças disponíveis. Passando a vez.");
            GameManager.Instance.SwitchPlayer();
            yield break;
        }

        // Restante do método permanece igual...
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

        Debug.Log("AI não encontrou movimentos válidos. Passando a vez.");
        GameManager.Instance.SwitchPlayer();
    }

    private List<Vector2Int> GetStrategicPositions(int aiPlayerIndex)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int boardSize = BoardManager.BoardSize;

        // 1. Adiciona posições adjacentes às peças já colocadas
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

        // 2. Se for o primeiro movimento, usa a posição inicial
        if (GameManager.Instance.IsFirstMove(aiPlayerIndex))
        {
            Vector2Int startPos = GameManager.Instance.startPositions[aiPlayerIndex];
            positions.Insert(0, startPos);
        }

        // 3. Adiciona posições aleatórias se não houver estratégicas suficientes
        while (positions.Count < 20)
        {
            int x = Random.Range(0, boardSize);
            int y = Random.Range(0, boardSize);
            positions.Add(new Vector2Int(x, y));
        }

        return positions.Distinct().ToList();
    }

    public void ResetAI()
    {
        StopAllCoroutines();
    }
}