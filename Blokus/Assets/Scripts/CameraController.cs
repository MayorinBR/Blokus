using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float boardMargin = 1f;

    void Start()
    {
        float boardSize = BoardManager.BoardSize;

        // Setup orthographic top-down camera
        Camera.main.orthographic = true;
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Calculate required orthographic size based only on board height
        float totalHeight = boardSize + boardMargin * 2;

        Camera.main.orthographicSize = totalHeight / 2;

        // Center the camera exactly over the board
        Camera.main.transform.position = new Vector3(0f, 15f, 0f);
    }
}
