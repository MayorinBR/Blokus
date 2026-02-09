using UnityEngine;
using System.Collections;

public class PieceDragger : MonoBehaviour
{
    private GameObject selectedPiece;
    private Vector3 offset;
    private float zCoord;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool wasDragging = false;
    private bool isBeingDestroyed = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectPiece();
        }

        if (selectedPiece != null)
        {
            // Only allow transformations if the piece is being dragged
            if (wasDragging && selectedPiece.GetComponent<PieceDragger>() != null)
            {
                HandlePieceTransformations();
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 newPos = GetMouseWorldPos() + offset;
                selectedPiece.transform.position = new Vector3(newPos.x, 0, newPos.z);
                wasDragging = true;

                if (BoardManager.Instance.enableHighlight)
                {
                    BoardManager.Instance.HighlightValidPositions(selectedPiece);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (wasDragging)
                {
                    HandlePiecePlacement();
                }
            }
        }
    }

    private void HandlePieceTransformations()
    {
        PieceFlipper flipper = selectedPiece.GetComponent<PieceFlipper>();

        if (Input.GetKeyDown(KeyCode.A)) // Rotação para esquerda
        {
            selectedPiece.transform.Rotate(0, -90, 0);
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.D)) // Rotação para direita
        {
            selectedPiece.transform.Rotate(0, 90, 0);
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.W)) // Flip vertical
        {
            flipper.FlipZ();
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.S)) // Flip horizontal
        {
            flipper.FlipX();
            UpdateVisuals();
        }
        else if (Input.GetMouseButtonDown(1)) // Rotação com botão direito
        {
            selectedPiece.transform.Rotate(0, 90, 0);
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        // Force position update after transformation
        Vector3 newPos = GetMouseWorldPos() + offset;
        selectedPiece.transform.position = new Vector3(newPos.x, 0, newPos.z);

        if (BoardManager.Instance.enableHighlight)
        {
            BoardManager.Instance.HighlightValidPositions(selectedPiece);
        }
    }

    private void TrySelectPiece()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform rootPiece = hit.transform.root;
            PieceDragger dragger = rootPiece.GetComponent<PieceDragger>();

            if (dragger != null && dragger == this)
            {
                // Verifica se a peça está na paleta do jogador atual
                string pieceName = rootPiece.name.Split('_')[0];
                PieceManager.PieceType type = (PieceManager.PieceType)System.Enum.Parse(
                    typeof(PieceManager.PieceType), pieceName);

                if ((GameManager.Instance.currentPlayer == 0 && PiecePalette.Instance.player1Pieces.ContainsKey(type)) ||
                    (GameManager.Instance.currentPlayer == 1 && PiecePalette.Instance.player2Pieces.ContainsKey(type)))
                {
                    selectedPiece = rootPiece.gameObject;
                    zCoord = Camera.main.WorldToScreenPoint(selectedPiece.transform.position).z;
                    offset = selectedPiece.transform.position - GetMouseWorldPos();
                    originalPosition = selectedPiece.transform.position;
                    wasDragging = false;

                    PiecePalette.Instance.PieceSelected(selectedPiece);
                }
            }
        }
    }

    private int GetPiecePlayer(GameObject piece)
    {
        // Verifica a cor da peça para determinar o jogador
        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Color pieceColor = renderer.material.color;
            for (int i = 0; i < GameManager.Instance.playerColors.Length; i++)
            {
                if (ColorsAreSimilar(pieceColor, GameManager.Instance.playerColors[i]))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private bool ColorsAreSimilar(Color a, Color b, float threshold = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    private void HandlePiecePlacement()
    {
        if (isBeingDestroyed) return;

        bool isValid = GameManager.Instance.IsValidMove(selectedPiece);

        if (isValid && GameManager.Instance.PlacePiece(selectedPiece, selectedPiece.transform.position))
        {
            // Limpa a referência sem destruir a peça
            CleanUp();
            return;
        }
        else
        {
            ReturnPieceToOriginalPosition();
        }
    }

    public void CleanUp()
    {
        if (isBeingDestroyed) return;

        if (selectedPiece != null)
        {
            BoardManager.Instance.ClearHighlights();
            selectedPiece = null;
        }
        wasDragging = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    /*
    private IEnumerator ReturnPieceToOriginalPosition()
    {
        // Salva a rotação atual para animação
        Quaternion currentRotation = selectedPiece.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 90, 0); // Rotação original (padrão das peças na paleta)

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPosition = selectedPiece.transform.position;

        while (elapsed < duration)
        {
            // Interpola tanto a posição quanto a rotação
            selectedPiece.transform.position = Vector3.Lerp(
                startPosition,
                originalPosition,
                elapsed / duration
            );

            selectedPiece.transform.rotation = Quaternion.Lerp(
                currentRotation,
                targetRotation,
                elapsed / duration
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Garante os valores exatos no final
        selectedPiece.transform.position = originalPosition;
        selectedPiece.transform.rotation = targetRotation;
        selectedPiece.transform.localScale = Vector3.one * PiecePalette.Instance.pieceScale;

        CleanUp();
    }*/
    private void ReturnPieceToOriginalPosition()
    {
        if (selectedPiece != null)
        {
            // Retorna para a posição original
            selectedPiece.transform.position = originalPosition;

            // Reseta para rotação padrão de 90 graus
            PieceFlipper flipper = selectedPiece.GetComponent<PieceFlipper>();
            if (flipper != null)
            {
                flipper.ResetToDefaultRotation();
            }
            else
            {
                selectedPiece.transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            selectedPiece.transform.localScale = Vector3.one * PiecePalette.Instance.pieceScale;
        }
        CleanUp();
    }
}