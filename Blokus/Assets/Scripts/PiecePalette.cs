using UnityEngine;
using System.Collections.Generic;
using static PieceManager;

public class PiecePalette : MonoBehaviour
{
    public static PiecePalette Instance;

    public Dictionary<PieceType, GameObject> player1Pieces = new Dictionary<PieceType, GameObject>();
    public Dictionary<PieceType, GameObject> player2Pieces = new Dictionary<PieceType, GameObject>();
    private Dictionary<PieceType, Quaternion> originalRotations = new Dictionary<PieceType, Quaternion>();
    private GameObject selectedPiece;

    [Header("Layout Settings")]
    [SerializeField] private float _pieceScale = 0.6f;
    public float pieceScale => _pieceScale;
    [SerializeField] private float spacing = 0.2f;
    [SerializeField] private int maxRows = 6;

    [Header("Position Settings")]
    [SerializeField] private float horizontalOffset = 2f;
    [SerializeField] private float verticalStart = 6f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        //DisplayAllPieces();
    }

    public void DisplayAllPieces()
    {
        float tileSize = BoardManager.Instance.tileSize;
        float boardWidth = BoardManager.BoardSize * tileSize;

        float leftStartX = -boardWidth / 2 - horizontalOffset;
        float rightStartX = boardWidth / 2 + horizontalOffset;

        int row = 0;
        int col = 0;

        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            // Jogador 1 (esquerda)
            Vector3 leftPos = new Vector3(
                leftStartX - col * (3f * pieceScale + spacing),
                0f,
                verticalStart - row * (5f * pieceScale + spacing)
            );

            GameObject leftPiece = PieceManager.Instance.CreatePiece(type, 0);
            leftPiece.transform.position = leftPos;
            leftPiece.transform.localScale = Vector3.one * pieceScale;
            leftPiece.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            player1Pieces[type] = leftPiece;

            // Jogador 2 (direita)
            Vector3 rightPos = new Vector3(
                rightStartX + col * (3f * pieceScale + spacing),
                0f,
                verticalStart - row * (5f * pieceScale + spacing)
            );

            GameObject rightPiece = PieceManager.Instance.CreatePiece(type, 1);
            rightPiece.transform.position = rightPos;
            rightPiece.transform.localScale = Vector3.one * pieceScale;
            rightPiece.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            player2Pieces[type] = rightPiece;

            row++;
            if (row >= maxRows)
            {
                row = 0;
                col++;
            }
            if (ScoreUI.Instance != null)
            {
                ScoreUI.Instance.UpdatePiecesRemaining();
            }
        }
    }

    public void RemovePiece(PieceType type, int playerIndex)
    {
        bool isPvP = GameSettings.Instance != null && GameSettings.Instance.isPvP;

        if (playerIndex == 0 && player1Pieces.ContainsKey(type))
        {
            player1Pieces.Remove(type);
        }
        else if (playerIndex == 1 && player2Pieces.ContainsKey(type))
        {
            // Se NÃO for PvP (ou seja, for contra a AI), mantém a lógica de destruir
            if (!isPvP)
            {
                Destroy(player2Pieces[type]);
            }

            player2Pieces.Remove(type);
        }
    }

    public void PieceSelected(GameObject piece)
{
    selectedPiece = piece;
    piece.transform.localScale = Vector3.one;
    
    // Garante que a peça está na rotação padrão quando selecionada
    PieceFlipper flipper = piece.GetComponent<PieceFlipper>();
    if (flipper != null)
    {
        flipper.ResetToDefaultRotation();
    }
    else
    {
        piece.transform.rotation = Quaternion.Euler(0, 90, 0);
    }
}

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

    public List<PieceManager.PieceType> GetAvailablePiecesForPlayer(int playerIndex)
    {
        return playerIndex == 0 ?
            new List<PieceManager.PieceType>(player1Pieces.Keys) :
            new List<PieceManager.PieceType>(player2Pieces.Keys);
    }
}