using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance;
    public enum PieceType
    {
        I1, I2, I3, I4, I5,
        V3, T4, Z4, L4, O4,
        L5, T5, V5, Z5, P5,
        W5, U5, X5, F5, S5
    }

    public static Dictionary<PieceType, bool[,]> pieceShapes = new Dictionary<PieceType, bool[,]>()
{
    // 1x1 (Monomino)
    { PieceType.I1, new bool[1,1] { {true} } },
    
    // 1x2 (Domino)
    { PieceType.I2, new bool[1,2] { {true, true} } },
    
    // 1x3 (I-tromino)
    { PieceType.I3, new bool[1,3] { {true, true, true} } },
    
    // 2x2 (V-tromino)
    { PieceType.V3, new bool[2,2] { {true,false},
                                    {true,true} } },
    
    // 1x4 (I-tetromino)
    { PieceType.I4, new bool[1,4] { {true, true, true, true} } },
    
    // 2x3 (L-tetromino) - Rotated from original
    { PieceType.L4, new bool[2,3] { {true,false,false},
                                     {true,true,true} } },
    
    // 2x3 (T-tetromino) - Rotated from original
    { PieceType.T4, new bool[2,3] { {false,true,false},
                                     {true,true,true} } },
    
    // 2x2 (O-tetromino)
    { PieceType.O4, new bool[2,2] { {true,true},
                                    {true,true} } },
    
    // 2x3 (Z-tetromino) - Rotated from original
    { PieceType.Z4, new bool[2,3] { {true,true,false},
                                     {false,true,true} } },
    
    // 1x5 (I-pentomino)
    { PieceType.I5, new bool[1,5] { {true, true, true, true, true} } },
    
    // 2x4 (L-pentomino) - Rotated from original
    { PieceType.L5, new bool[2,4] { {true,false,false,false},
                                     {true,true,true,true} } },
    
    // 2x4 (S-pentomino) - Rotated from original
    { PieceType.S5, new bool[2,4] { {false,true,true,true},
                                     {true,true,false,false} } },
    
    // 2x3 (P-pentomino) - Rotated from original
    { PieceType.P5, new bool[2,3] { {true,true,true},
                                     {true,true,false} } },
    
    // 2x3 (U-pentomino)
    { PieceType.U5, new bool[2,3] { {true,false,true},
                                     {true,true,true} } },
    
    // 3x3 (T-pentomino)
    { PieceType.T5, new bool[3,3] { {true,true,true},
                                    {false,true,false},
                                    {false,true,false} } },
    
    // 3x3 (V-pentomino)
    { PieceType.V5, new bool[3,3] { {true,false,false},
                                    {true,false,false},
                                    {true,true,true} } },
    
    // 3x3 (W-pentomino)
    { PieceType.W5, new bool[3,3] { {true,false,false},
                                    {true,true,false},
                                    {false,true,true} } },
    
    // 3x3 (F-pentomino)
    { PieceType.F5, new bool[3,3] { {false,true,true},
                                    {true,true,false},
                                    {false,true,false} } },
    
    // 3x3 (X-pentomino)
    { PieceType.X5, new bool[3,3] { {false,true,false},
                                    {true,true,true},
                                    {false,true,false} } },
    
    // 3x3 (Z-pentomino)
    { PieceType.Z5, new bool[3,3] { {true,true,false},
                                    {false,true,false},
                                    {false,true,true} } }
};

    public GameObject piecePrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject CreatePiece(PieceType type, int playerIndex)
    {
        bool[,] shape = pieceShapes[type];

        // Create parent object
        GameObject piece = new GameObject($"{type}_Player{playerIndex}");
        int piecesLayer = LayerMask.NameToLayer("Pieces");
        if (piecesLayer != -1) piece.layer = piecesLayer;

        // Create collider object
        GameObject colliderObj = new GameObject("PieceCollider");
        colliderObj.transform.SetParent(piece.transform);
        colliderObj.transform.localPosition = Vector3.zero;
        if (piecesLayer != -1) colliderObj.layer = piecesLayer;

        BoxCollider collider = colliderObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(shape.GetLength(1), 0.1f, shape.GetLength(0));
        collider.center = new Vector3(
            shape.GetLength(1) / 2f - 0.5f,
            0,
            shape.GetLength(0) / 2f - 0.5f
        );

        // Create individual blocks
        for (int x = 0; x < shape.GetLength(0); x++)
        {
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                if (shape[x, y])
                {
                    GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.transform.SetParent(piece.transform);
                    block.transform.localPosition = new Vector3(y, 0, x);
                    if (piecesLayer != -1) block.layer = piecesLayer;

                    Renderer blockRenderer = block.GetComponent<Renderer>();
                    blockRenderer.material = new Material(Shader.Find("Unlit/Color"))
                    {
                        color = GameManager.Instance.playerColors[playerIndex]
                    };

                    Destroy(block.GetComponent<BoxCollider>());
                }
            }
        }

        // Add PieceDragger component
        PieceDragger dragger = piece.AddComponent<PieceDragger>();
        PieceFlipper flipper = piece.AddComponent<PieceFlipper>();
        flipper.Initialize(piece);

        piece.transform.rotation = Quaternion.Euler(0, 90, 0);

        return piece;
    }
}