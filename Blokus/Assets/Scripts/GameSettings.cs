using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public float aiDelay = 1f; // Valor padrão
    public bool isPvP = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}