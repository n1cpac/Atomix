using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// SceneVideoPlayer - Persistente en el GameManager del menú para reproducir un video de introducción
/// cuando se carga la escena de nivel especificada. Se inicializa en la escena de menú y sobrevíve
/// al cambio de escena gracias a DontDestroyOnLoad.
/// </summary>
public class SceneVideoPlayer : MonoBehaviour
{
    [Header("Configuración del Nivel y Video")]    
    [Tooltip("Nombre de la escena en la que se reproducirá el video de introducción")]    
    public string targetSceneName;
    [Tooltip("Nombre del archivo de video (debe estar en StreamingAssets/Videos)")]
    public string videoFileName;

    [Header("UI y Controles")]    
    [Tooltip("Canvas que contiene el reproductor de video y elementos UI")]    
    public GameObject videoPlayerCanvas;
    [Tooltip("RawImage donde se mostrará el video")]    
    public RawImage videoRawImage;
    [Tooltip("Duración automática del video en segundos antes de saltar" )]
    public float introDuration = 5f;
    [Tooltip("Tecla para omitir el video (por defecto Return / Enter)")]
    public KeyCode skipKey = KeyCode.Return;

    private VideoPlayer videoPlayer;
    private bool isPlaying = false;

    private void Awake()
    {
        // Hacer persistente este GameObject al cambiar de escena
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == targetSceneName)
        {
            InitializeVideoPlayer();
            StartCoroutine(PlayIntroSequence());
        }
    }

    private void InitializeVideoPlayer()
    {
        if (videoPlayerCanvas == null)
        {
            Debug.LogWarning("SceneVideoPlayer: videoPlayerCanvas no asignado, buscando en la escena...");
            videoPlayerCanvas = GameObject.Find("VideoPlayerCanvas");
        }

        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(false);
        
        // Configurar VideoPlayer o añadirlo si no existe
        videoPlayer = GetComponent<VideoPlayer>() ?? gameObject.AddComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        
        if (videoRawImage == null && videoPlayerCanvas != null)
            videoRawImage = videoPlayerCanvas.GetComponentInChildren<RawImage>();

        if (videoRawImage != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
            videoRawImage.texture = videoPlayer.targetTexture;
        }
        else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            if (videoPlayer.targetCamera == null)
            {
                GameObject camObj = new GameObject("VideoCamera");
                var cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.depth = 100;
                camObj.SetActive(false);
                videoPlayer.targetCamera = cam;
            }
        }

        // Asignar y preparar la ruta del video
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
        videoPlayer.Prepare();
    }

    private IEnumerator PlayIntroSequence()
    {
        // Mostrar UI y cámara si aplica
        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(true);
        if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera != null)
            videoPlayer.targetCamera.gameObject.SetActive(true);

        isPlaying = true;
        videoPlayer.Play();

        float timer = 0f;
        while (isPlaying && timer < introDuration)
        {
            if (Input.GetKeyDown(skipKey))
            {
                Debug.Log("SceneVideoPlayer: Video omitido por el usuario.");
                break;
            }
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        StopIntroVideo();
    }

    private void Update()
    {
        // Permite omitir en Update por si Update corre antes que la coroutine
        if (isPlaying && Input.GetKeyDown(skipKey))
        {
            StopIntroVideo();
        }
    }

    private void StopIntroVideo()
    {
        if (!isPlaying) return;
        videoPlayer.Stop();
        isPlaying = false;

        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(false);

        if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera != null)
            videoPlayer.targetCamera.gameObject.SetActive(false);

        Debug.Log("SceneVideoPlayer: Video de introducción detenido, iniciando nivel.");
    }
}
