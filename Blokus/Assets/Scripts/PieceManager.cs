using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all Blokus piece shapes and provides the factory method for creating
/// piece GameObjects at runtime. Each piece is built from primitive cubes parented
/// to a single root object with a collider sized to the shape's bounding box.
/// </summary>
public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance;

    /// <summary>All available Blokus piece types, from monominoes to pentominoes.</summary>
    public enum PieceType
    {
        I1, I2, I3, I4, I5,
        V3, T4, Z4, L4, O4,
        L5, T5, V5, Z5, P5,
        W5, U5, X5, F5, S5,
        P4
    }

    /// <summary>
    /// Boolean grid shapes for every piece type.
    /// <c>true</c> cells represent filled squares; indices are [row, column].
    /// </summary>
    public static Dictionary<PieceType, bool[,]> pieceShapes = new Dictionary<PieceType, bool[,]>()
    {
        { PieceType.I1, new bool[1,1] { {true} } },
        { PieceType.I2, new bool[1,2] { {true, true} } },
        { PieceType.I3, new bool[1,3] { {true, true, true} } },
        { PieceType.V3, new bool[2,2] { {true,false}, {true,true} } },
        { PieceType.I4, new bool[1,4] { {true, true, true, true} } },
        { PieceType.L4, new bool[2,3] { {true,false,false}, {true,true,true} } },
        { PieceType.T4, new bool[2,3] { {false,true,false}, {true,true,true} } },
        { PieceType.O4, new bool[2,2] { {true,true}, {true,true} } },
        { PieceType.Z4, new bool[2,3] { {true,true,false}, {false,true,true} } },
        { PieceType.I5, new bool[1,5] { {true, true, true, true, true} } },
        { PieceType.L5, new bool[2,4] { {true,false,false,false}, {true,true,true,true} } },
        { PieceType.S5, new bool[2,4] { {false,true,true,true}, {true,true,false,false} } },
        { PieceType.P5, new bool[2,3] { {true,true,true}, {true,true,false} } },
        { PieceType.U5, new bool[2,3] { {true,false,true}, {true,true,true} } },
        { PieceType.T5, new bool[3,3] { {true,true,true}, {false,true,false}, {false,true,false} } },
        { PieceType.V5, new bool[3,3] { {true,false,false}, {true,false,false}, {true,true,true} } },
        { PieceType.W5, new bool[3,3] { {true,false,false}, {true,true,false}, {false,true,true} } },
        { PieceType.F5, new bool[3,3] { {false,true,true}, {true,true,false}, {false,true,false} } },
        { PieceType.X5, new bool[3,3] { {false,true,false}, {true,true,true}, {false,true,false} } },
        { PieceType.Z5, new bool[3,3] { {true,true,false}, {false,true,false}, {false,true,true} } },
        { PieceType.P4, new bool[2,4] { {false,true,false,false}, {true,true,true,true} } }
    };

    [Tooltip("Unused prefab reference kept for future use. Pieces are built procedurally via CreatePiece.")]
    public GameObject piecePrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Creates a piece GameObject for the given type and player.
    /// The piece is built from primitive cubes with a single bounding-box collider on the root.
    /// <see cref="PieceDragger"/> and <see cref="PieceFlipper"/> are attached automatically.
    /// </summary>
    /// <param name="type">The piece type to instantiate.</param>
    /// <param name="playerIndex">Zero-based index of the owning player. Determines the piece color.</param>
    /// <returns>The fully configured piece GameObject.</returns>
    public GameObject CreatePiece(PieceType type, int playerIndex)
    {
        bool[,] shape = pieceShapes[type];

        GameObject piece = new GameObject($"{type}_Player{playerIndex}");
        int piecesLayer = LayerMask.NameToLayer("Pieces");
        if (piecesLayer != -1) piece.layer = piecesLayer;

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

        for (int x = 0; x < shape.GetLength(0); x++)
        {
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                if (!shape[x, y]) continue;

                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.SetParent(piece.transform);
                block.transform.localPosition = new Vector3(y, 0, x);
                block.transform.localScale = Vector3.one * 0.92f;
                if (piecesLayer != -1) block.layer = piecesLayer;

                block.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"))
                {
                    color = GameSettings.Instance.GetPlayerColor(playerIndex)
                };

                Destroy(block.GetComponent<BoxCollider>());
            }
        }

        PieceFlipper flipper = piece.AddComponent<PieceFlipper>();
        piece.AddComponent<PieceDragger>();
        flipper.Initialize(piece);

        piece.transform.rotation = Quaternion.Euler(0, 90, 0);

        return piece;
    }
}