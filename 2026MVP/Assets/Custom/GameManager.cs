using UnityEngine;

/// <summary>
/// Singleton GameManager that persists across scenes.
/// Stores the expediente number and other session data.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// The expediente number entered by the user in the main menu.
    /// </summary>
    public string Expediente { get; set; }

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
