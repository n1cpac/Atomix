using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Necesario para reproducir videos

/// <summary>
/// MetanoAssembler - Gestiona la evolución del modelo de metano cuando entra en contacto con hidrógeno.
/// Este script maneja las diferentes etapas de evolución del metano, desde metano0 hasta metanocompleto.
/// </summary>
public class MetanoAssembler : MonoBehaviour
{
    [Tooltip("Array con los prefabs de las diferentes etapas del metano (0-4)")]
    public GameObject[] metanoStages;  // Arrastrar los prefabs metano0, metano1, metano2, metano3, metanocompleto
    
    [Tooltip("Referencia a la instancia actual del metano en la escena")]
    public GameObject currentMetano;   // Referencia a la instancia actual del metano
    
    [Tooltip("Etapa actual del metano (0-4)")]
    [SerializeField] private int currentStage = 0;      // La etapa actual del metano (comienza en 0)

    [Tooltip("Si es verdadero, mostrará mensajes de depuración adicionales")]
    public bool showDebugGizmos = true;
    
    [Tooltip("Tiempo de espera en segundos antes de volver al menú tras completar el nivel")]
    public float tiempoEsperaFinNivel = 2.0f;
    
    [Tooltip("Nombre de la escena que contiene el menú de niveles")]
    public string menuInicialSceneName = "MenuInicial";
    
    [Tooltip("Nombre del GameObject que contiene el grid de niveles")]
    public string gridNivelesName = "grid_niveles";
    
    [Tooltip("Ruta del video a reproducir al completar el nivel (desde Assets/Videos/)")]
    public string videoFileName = "completion_video.mp4";
    
    [Tooltip("Canvas UI que contiene el reproductor de video")]
    public GameObject videoPlayerCanvas;
    
    [Tooltip("RawImage donde se mostrará el video")]
    public UnityEngine.UI.RawImage videoRawImage;
    
    // VideoPlayer que manejará la reproducción del video
    private VideoPlayer videoPlayer;
    
    // Indica si se está reproduciendo el video de finalización
    private bool isPlayingCompletionVideo = false;

    /// <summary>
    /// Inicializa el componente y verifica que todas las referencias estén configuradas correctamente.
    /// </summary>
    private void Start()
    {
        // Validar que el array metanoStages tenga elementos
        if (metanoStages == null || metanoStages.Length == 0)
        {
            Debug.LogError("MetanoAssembler: El array de etapas de metano está vacío. Por favor, arrastra los prefabs al inspector.");
            return;
        }

        // Validar que currentMetano esté asignado
        if (currentMetano == null)
        {
            Debug.LogWarning("MetanoAssembler: No se ha asignado un metano inicial. Buscando en la escena...");
            
            // Intentar encontrar un objeto con tag "Metano"
            currentMetano = GameObject.FindWithTag("Metano");
            
            if (currentMetano == null)
            {
                Debug.LogError("MetanoAssembler: No se encontró un metano inicial en la escena. Por favor, asigna uno manualmente.");
                return;
            }
            else
            {
                Debug.Log("MetanoAssembler: Se encontró automáticamente un metano inicial en la escena.");
            }
        }

        // Verificar que el currentMetano tenga un collider con isTrigger=true
        Collider metanoCollider = currentMetano.GetComponent<Collider>();
        if (metanoCollider == null)
        {
            Debug.LogWarning("MetanoAssembler: El metano inicial no tiene un Collider. Añadiendo un SphereCollider...");
            SphereCollider sc = currentMetano.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1.0f;
        }
        else if (!metanoCollider.isTrigger)
        {
            Debug.LogWarning("MetanoAssembler: El Collider del metano inicial no es un trigger. Cambiando a isTrigger=true...");
            metanoCollider.isTrigger = true;
        }

        // Verificar que el currentMetano tenga un Rigidbody
        Rigidbody metanoRb = currentMetano.GetComponent<Rigidbody>();
        if (metanoRb == null)
        {
            Debug.LogWarning("MetanoAssembler: El metano inicial no tiene un Rigidbody. Añadiendo uno...");
            metanoRb = currentMetano.AddComponent<Rigidbody>();
            metanoRb.isKinematic = true;
            metanoRb.useGravity = false;
        }
        
        // Asegurarse que tiene el tag correcto
        if (currentMetano.tag != "Metano")
        {
            Debug.LogWarning("MetanoAssembler: El metano inicial no tiene el tag 'Metano'. Cambiando tag...");
            currentMetano.tag = "Metano";
        }

        // Agregar el componente MetanoCollisionHandler al metano actual
        AgregarCollisionHandler();
        
        // Configurar el reproductor de video si no existe
        SetupVideoPlayer();
        
        // Desactivar el canvas de video al inicio
        if (videoPlayerCanvas != null)
        {
            videoPlayerCanvas.SetActive(false);
        }
        
        Debug.Log("MetanoAssembler: Inicializado. Etapa actual: " + currentStage);
        
        // Mostrar gizmos si está habilitado
        if (showDebugGizmos && currentMetano != null)
        {
            // Dibuja una esfera en la ubicación del metano actual para visualización en el editor
            Debug.DrawLine(currentMetano.transform.position, 
                          currentMetano.transform.position + Vector3.up * 2.0f, 
                          Color.green, 10.0f);
        }
    }
    
    /// <summary>
    /// Configura el reproductor de video
    /// </summary>
    private void SetupVideoPlayer()
    {
        // Si no tenemos un canvas para el video, intentamos buscarlo o crearlo
        if (videoPlayerCanvas == null)
        {
            videoPlayerCanvas = GameObject.Find("VideoPlayerCanvas");
            
            if (videoPlayerCanvas == null)
            {
                Debug.LogWarning("VideoPlayerCanvas no encontrado. Por favor, crea un Canvas en la escena con nombre 'VideoPlayerCanvas'.");
            }
        }
        
        // Si no tenemos un RawImage para el video, intentamos buscarlo
        if (videoRawImage == null && videoPlayerCanvas != null)
        {
            videoRawImage = videoPlayerCanvas.GetComponentInChildren<UnityEngine.UI.RawImage>();
            
            if (videoRawImage == null)
            {
                Debug.LogWarning("No se encontró un RawImage para mostrar el video. Por favor, añade un RawImage al Canvas.");
            }
        }
        
        // Crear o conseguir un VideoPlayer
        if (videoPlayer == null)
        {
            // Verificar si existe un VideoPlayer en este GameObject
            videoPlayer = GetComponent<VideoPlayer>();
            
            if (videoPlayer == null)
            {
                // Añadir VideoPlayer si no existe
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }
        
        // Configurar el VideoPlayer
        if (videoPlayer != null)
        {
            // Configurar para reproducir en bucle
            videoPlayer.isLooping = true;
            
            // Configurar para reproducir en RawImage si existe
            if (videoRawImage != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
                videoRawImage.texture = videoPlayer.targetTexture;
            }
            else
            {
                // Si no hay RawImage, reproducir en pantalla completa
                videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
                // Obtener o crear una cámara para reproducir el video
                Camera videoCamera = GameObject.Find("VideoCamera")?.GetComponent<Camera>();
                if (videoCamera == null)
                {
                    GameObject videoCameraObj = new GameObject("VideoCamera");
                    videoCamera = videoCameraObj.AddComponent<Camera>();
                    videoCamera.depth = 999; // Asegurar que se muestra por encima de todo
                    videoCamera.clearFlags = CameraClearFlags.SolidColor;
                    videoCamera.backgroundColor = Color.black;
                    videoCameraObj.SetActive(false); // Desactivar inicialmente
                }
                videoPlayer.targetCamera = videoCamera;
            }
            
            // Configurar la ruta del video
            try
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
                Debug.Log("Video configurado: " + videoPlayer.url);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al configurar la ruta del video: " + e.Message);
            }
            
            // Preparar el video pero no reproducirlo aún
            videoPlayer.Prepare();
        }
    }

    /// <summary>
    /// Actualiza cada frame y verifica si se presiona ENTER para omitir el video
    /// o si se presiona la tecla 5 para saltar directamente a la molécula completada
    /// </summary>
    private void Update()
    {
        // Si se está reproduciendo el video y se presiona ENTER, omitir el video
        if (isPlayingCompletionVideo && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Se presionó ENTER. Omitiendo video y volviendo al menú...");
            StopVideo();
            VolverAlMenu();
        }
        
        // MÉTODO OCULTO: Si se presiona la tecla 5, saltar directamente a la molécula completa
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            // Acceso directo secreto para saltar a la molécula completada
            SaltarAMoleculaCompleta();
        }
    }
    
    /// <summary>
    /// Método secreto que permite saltar directamente a la molécula completa (última etapa)
    /// Se activa con la tecla 5
    /// </summary>
    private void SaltarAMoleculaCompleta()
    {
        // Verificar que tengamos configurados los prefabs
        if (metanoStages == null || metanoStages.Length == 0)
        {
            Debug.LogError("No se puede saltar a molécula completa: Array de metanoStages no configurado");
            return;
        }
        
        int ultimaEtapa = metanoStages.Length - 1;
        if (metanoStages[ultimaEtapa] == null)
        {
            Debug.LogError("No se puede saltar a molécula completa: Prefab de última etapa no asignado");
            return;
        }
        
        Debug.Log("¡MODO DESARROLLADOR ACTIVADO! Saltando directamente a la molécula completa (etapa " + ultimaEtapa + ")");
        
        // Guardar posición actual si existe un metano
        Vector3 posicion = Vector3.zero;
        Quaternion rotacion = Quaternion.identity;
        
        if (currentMetano != null)
        {
            posicion = currentMetano.transform.position;
            rotacion = currentMetano.transform.rotation;
            
            // Destruir el metano actual
            Destroy(currentMetano);
            currentMetano = null;
        }
        else
        {
            // Si no hay metano, usar una posición predeterminada
            posicion = new Vector3(0, 1, 0);
        }
        
        // Instanciar directamente la molécula completa
        GameObject metanoCompleto = Instantiate(
            metanoStages[ultimaEtapa],
            posicion,
            rotacion,
            null
        );
        
        metanoCompleto.name = "Metano_Completo_Saltado";
        currentMetano = metanoCompleto;
        currentStage = ultimaEtapa;
        
        // Asegurarse que tiene el tag correcto
        if (currentMetano.tag != "Metano")
        {
            currentMetano.tag = "Metano";
        }
        
        // El metano ya está completo, llamar a NivelCompletado
        NivelCompletado();
    }

    /// <summary>
    /// Para visualización en el editor, muestra la posición del metano actual
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showDebugGizmos && currentMetano != null)
        {
            // Dibuja un gizmo en la posición del metano actual
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentMetano.transform.position, 0.2f);
        }
    }

    /// <summary>
    /// Añade el componente MetanoCollisionHandler al metano actual
    /// </summary>
    private void AgregarCollisionHandler()
    {
        if (currentMetano != null)
        {
            // Verificar si ya tiene el componente
            MetanoCollisionHandler handler = currentMetano.GetComponent<MetanoCollisionHandler>();
            if (handler == null)
            {
                // Añadir el componente de manejo de colisiones
                handler = currentMetano.AddComponent<MetanoCollisionHandler>();
                // Asignar este MetanoAssembler como referencia
                handler.assembler = this;
                Debug.Log("MetanoAssembler: Añadido MetanoCollisionHandler al metano actual: " + currentMetano.name);
            }
        }
    }

    /// <summary>
    /// Actualiza el metano a la siguiente etapa cuando colisiona con un hidrógeno.
    /// </summary>
    /// <param name="hidrogenoObj">El objeto hidrógeno que colisionó con el metano</param>
    public void EvolucionarMetano(GameObject hidrogenoObj)
    {
        if (currentMetano == null)
        {
            Debug.LogError("MetanoAssembler: No hay metano actual para evolucionar");
            return;
        }
        
        // Desactivar o destruir el hidrógeno
        Debug.Log("MetanoAssembler: Hidrógeno consumido: " + hidrogenoObj.name);
        Destroy(hidrogenoObj);

        // Calcular la próxima etapa
        int nextStage = currentStage + 1;
        
        // Verificar si hemos alcanzado el límite máximo de etapas
        if (nextStage >= metanoStages.Length)
        {
            Debug.Log("MetanoAssembler: El metano ya está en su etapa final (Metano Completo)");
            return;
        }

        // Verificar que el siguiente prefab existe
        if (metanoStages[nextStage] == null)
        {
            Debug.LogError("MetanoAssembler: El prefab para la etapa " + nextStage + " no está asignado en el array");
            return;
        }

        // Guardar la posición y rotación actuales
        Vector3 currentPosition = currentMetano.transform.position;
        Quaternion currentRotation = currentMetano.transform.rotation;
        
        Debug.Log("MetanoAssembler: Posición del metano actual antes de destruirlo: " + currentPosition);
        
        // Desactivar o destruir el metano actual
        GameObject oldMetano = currentMetano;
        currentMetano = null; // Limpiar referencia antes de destruir
        Debug.Log("MetanoAssembler: Destruyendo metano en etapa " + currentStage + ": " + oldMetano.name);
        Destroy(oldMetano);
        
        // Instanciar la nueva etapa del metano - usar Instantiate con parent = null para asegurar que no herede transformaciones
        GameObject newMetano = Instantiate(
            metanoStages[nextStage], 
            currentPosition, 
            currentRotation,
            null // sin padre
        );
        
        // Asegurarse de que el nuevo metano esté activo
        newMetano.SetActive(true);
        
        // Dar un nombre más descriptivo para facilitar depuración
        newMetano.name = "Metano_Etapa_" + nextStage;
        
        Debug.Log("MetanoAssembler: Instanciado nuevo metano: " + newMetano.name + " en posición: " + newMetano.transform.position);
        
        // Actualizar la referencia al metano actual
        currentMetano = newMetano;
        
        // Actualizar la etapa actual
        currentStage = nextStage;
        
        // Asegurarse que tiene el tag correcto
        if (currentMetano.tag != "Metano")
        {
            currentMetano.tag = "Metano";
        }

        // Verificar o añadir componentes necesarios al nuevo metano
        Collider newCollider = currentMetano.GetComponent<Collider>();
        if (newCollider == null)
        {
            Debug.LogWarning("MetanoAssembler: El nuevo metano no tiene un Collider. Añadiendo uno...");
            SphereCollider sc = currentMetano.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1.0f;
        }
        else if (!newCollider.isTrigger)
        {
            newCollider.isTrigger = true;
        }

        Rigidbody newRb = currentMetano.GetComponent<Rigidbody>();
        if (newRb == null)
        {
            Debug.LogWarning("MetanoAssembler: El nuevo metano no tiene un Rigidbody. Añadiendo uno...");
            newRb = currentMetano.AddComponent<Rigidbody>();
            newRb.isKinematic = true;
            newRb.useGravity = false;
        }
        
        // Añadir el componente de manejo de colisiones al nuevo metano
        AgregarCollisionHandler();
        
        Debug.Log("MetanoAssembler: Metano evolucionado exitosamente a etapa " + currentStage + 
                  " (" + metanoStages[currentStage].name + ")");
        
        // Verificar si hemos alcanzado la etapa final (metanocompleto)
        if (currentStage == metanoStages.Length - 1)
        {
            NivelCompletado();
        }
        
        // Para visualización en fase de depuración
        if (showDebugGizmos)
        {
            Debug.DrawLine(currentMetano.transform.position, 
                          currentMetano.transform.position + Vector3.up * 2.0f, 
                          Color.magenta, 10.0f);
        }
    }

    /// <summary>
    /// Se llama cuando el metano ha alcanzado su etapa final (metanocompleto)
    /// </summary>
    private void NivelCompletado()
    {
        Debug.Log("¡NIVEL COMPLETADO! El metano ha alcanzado su etapa final (metanocompleto)");
        
        // Reproducir el video de finalización
        PlayCompletionVideo();
    }
    
    /// <summary>
    /// Reproduce el video de finalización del nivel
    /// </summary>
    private void PlayCompletionVideo()
    {
        // Si el video no está configurado correctamente, volver directamente al menú
        if (videoPlayer == null || string.IsNullOrEmpty(videoPlayer.url))
        {
            Debug.LogWarning("Video no configurado correctamente. Volviendo al menú principal.");
            StartCoroutine(VolverAlMenuDespuesDeEspera());
            return;
        }
        
        Debug.Log("Reproduciendo video de finalización: " + videoPlayer.url);
        
        // Activar el canvas del video si existe
        if (videoPlayerCanvas != null)
        {
            videoPlayerCanvas.SetActive(true);
        }
        
        // Si estamos usando una cámara para reproducir, activarla
        if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera != null)
        {
            videoPlayer.targetCamera.gameObject.SetActive(true);
        }
        
        // Reproducir el video
        videoPlayer.Play();
        isPlayingCompletionVideo = true;
        
        // Mostrar mensaje para omitir
        Debug.Log("Presiona ENTER para omitir el video y volver al menú principal");
        
        // Crear un GameObject con UI para mostrar el mensaje "Presiona ENTER para omitir"
        CreateSkipVideoUI();
    }
    
    /// <summary>
    /// Crea una UI para mostrar el mensaje "Presiona ENTER para omitir"
    /// </summary>
    private void CreateSkipVideoUI()
    {
        if (videoPlayerCanvas != null)
        {
            // Verificar si ya existe el texto
            Transform skipTextTrans = videoPlayerCanvas.transform.Find("SkipText");
            if (skipTextTrans == null)
            {
                // Crear un GameObject para el texto
                GameObject skipTextObj = new GameObject("SkipText");
                skipTextObj.transform.SetParent(videoPlayerCanvas.transform, false);
                
                // Añadir componente Text
                UnityEngine.UI.Text skipText = skipTextObj.AddComponent<UnityEngine.UI.Text>();
                skipText.text = "Presiona ENTER para omitir";
                skipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                skipText.fontSize = 24;
                skipText.color = Color.white;
                skipText.alignment = TextAnchor.LowerRight;
                
                // Configurar RectTransform
                RectTransform rectTransform = skipText.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(0.5f, 0);
                rectTransform.offsetMin = new Vector2(10, 10);
                rectTransform.offsetMax = new Vector2(-10, 40);
            }
        }
    }
    
    /// <summary>
    /// Detiene la reproducción del video
    /// </summary>
    private void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        isPlayingCompletionVideo = false;
        
        // Desactivar el canvas del video si existe
        if (videoPlayerCanvas != null)
        {
            videoPlayerCanvas.SetActive(false);
        }
        
        // Si estamos usando una cámara para reproducir, desactivarla
        if (videoPlayer != null && videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera != null)
        {
            videoPlayer.targetCamera.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Vuelve al menú principal inmediatamente
    /// </summary>
    private void VolverAlMenu()
    {
        // Detener cualquier corrutina en ejecución
        StopAllCoroutines();
        
        // Cargar la escena del menú principal
        Debug.Log("Cargando escena: " + menuInicialSceneName);
        SceneManager.LoadScene(menuInicialSceneName);
        
        // Iniciar corrutina para activar el grid de niveles después de cargar la escena
        StartCoroutine(ActivarGridNivelesDespuesDeCargarEscena());
    }
    
    /// <summary>
    /// Corrutina para activar el grid de niveles después de cargar la escena
    /// </summary>
    private IEnumerator ActivarGridNivelesDespuesDeCargarEscena()
    {
        // Esperar a que la escena esté completamente cargada
        yield return null;
        
        // Intentar encontrar y activar el grid_niveles
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform gridNiveles = canvas.transform.Find(gridNivelesName);
            if (gridNiveles != null)
            {
                Debug.Log("Activando grid_niveles en el menú");
                gridNiveles.gameObject.SetActive(true);
                
                // Asegurarse de que el EventSystem esté activo para recibir inputs
                UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem != null)
                {
                    eventSystem.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("No se encontró un EventSystem en la escena. La interacción UI puede no funcionar correctamente.");
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el grid_niveles '" + gridNivelesName + "' dentro del Canvas");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el Canvas en la escena " + menuInicialSceneName);
        }
    }
    
    /// <summary>
    /// Coroutine que espera un tiempo y luego vuelve al menú de niveles
    /// </summary>
    private IEnumerator VolverAlMenuDespuesDeEspera()
    {
        Debug.Log("Volviendo al menú de niveles en " + tiempoEsperaFinNivel + " segundos...");
        
        // Esperar el tiempo configurado
        yield return new WaitForSeconds(tiempoEsperaFinNivel);
        
        // Volver al menú
        VolverAlMenu();
    }

    /// <summary>
    /// Método de depuración para mostrar información sobre el estado actual del metano.
    /// </summary>
    public void MostrarEstadoActual()
    {
        if (currentMetano != null)
        {
            Debug.Log("MetanoAssembler - Estado actual:" +
                      "\n- Etapa: " + currentStage +
                      "\n- Prefab: " + metanoStages[currentStage].name +
                      "\n- Instancia: " + currentMetano.name +
                      "\n- Posición: " + currentMetano.transform.position +
                      "\n- Activo: " + currentMetano.activeSelf);
        }
        else
        {
            Debug.LogWarning("MetanoAssembler: No hay metano actual asignado.");
        }
    }
}

/// <summary>
/// Componente auxiliar que se añade a cada instancia de metano para manejar sus colisiones.
/// </summary>
public class MetanoCollisionHandler : MonoBehaviour
{
    [HideInInspector]
    public MetanoAssembler assembler;  // Referencia al MetanoAssembler principal
    
    /// <summary>
    /// Se llama cuando otro collider entra en el trigger del metano.
    /// </summary>
    /// <param name="other">El collider que entró en el trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (assembler == null)
        {
            Debug.LogError("MetanoCollisionHandler: No hay referencia al MetanoAssembler en " + gameObject.name);
            return;
        }
        
        // Verificar si el objeto que entró es un hidrógeno
        if (other.CompareTag("Hidrogeno"))
        {
            Debug.Log("MetanoCollisionHandler: " + gameObject.name + " detectó colisión con hidrógeno: " + other.gameObject.name);
            assembler.EvolucionarMetano(other.gameObject);
        }
    }
    
    /// <summary>
    /// Muestra información visual adicional para depuración
    /// </summary>
    private void OnDrawGizmos()
    {
        // Dibujar una línea para mostrar que este objeto tiene el handler
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}