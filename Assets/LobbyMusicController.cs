using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMusicController : MonoBehaviour
{
    public AudioSource audioSource;
    
    [Tooltip("Nombre exacto de la escena de lobby (como aparece en Build Settings)")]
    public string lobbySceneName = "Menuinicial*";

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Suscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Reproducir música al iniciar (si estamos en el lobby)
        if (SceneManager.GetActiveScene().name == lobbySceneName)
        {
            audioSource.Play();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Detener la música si la nueva escena NO es el lobby
        if (scene.name != lobbySceneName && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Opcional: Reproducir si volvemos al lobby
        if (scene.name == lobbySceneName && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void OnDestroy()
    {
        // Importante: Desuscribirse del evento al destruir el objeto
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}