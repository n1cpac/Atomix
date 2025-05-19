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

        // Verificar collider
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

        // Verificar Rigidbody
        Rigidbody metanoRb = currentMetano.GetComponent<Rigidbody>();
        if (metanoRb == null)
        {
            Debug.LogWarning("MetanoAssembler: El metano inicial no tiene un Rigidbody. Añadiendo uno...");
            metanoRb = currentMetano.AddComponent<Rigidbody>();
            metanoRb.isKinematic = true;
            metanoRb.useGravity = false;
        }

        // Tag
        if (currentMetano.tag != "Metano")
        {
            Debug.LogWarning("MetanoAssembler: El metano inicial no tiene el tag 'Metano'. Cambiando tag...");
            currentMetano.tag = "Metano";
        }

        // Handler y video
        AgregarCollisionHandler();
        SetupVideoPlayer();

        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(false);

        Debug.Log("MetanoAssembler: Inicializado. Etapa actual: " + currentStage);

        if (showDebugGizmos && currentMetano != null)
        {
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
        if (videoPlayerCanvas == null)
        {
            videoPlayerCanvas = GameObject.Find("VideoPlayerCanvas");
            if (videoPlayerCanvas == null)
                Debug.LogWarning("VideoPlayerCanvas no encontrado. Por favor, crea un Canvas en la escena con nombre 'VideoPlayerCanvas'.");
        }

        if (videoRawImage == null && videoPlayerCanvas != null)
        {
            videoRawImage = videoPlayerCanvas.GetComponentInChildren<UnityEngine.UI.RawImage>();
            if (videoRawImage == null)
                Debug.LogWarning("No se encontró un RawImage para mostrar el video. Por favor, añade un RawImage al Canvas.");
        }

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;               // <-- Evita reproducción y sonido al iniciar
            videoPlayer.isLooping = true;

            if (videoRawImage != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
                videoRawImage.texture = videoPlayer.targetTexture;
            }
            else
            {
                videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
                Camera videoCamera = GameObject.Find("VideoCamera")?.GetComponent<Camera>();
                if (videoCamera == null)
                {
                    GameObject videoCameraObj = new GameObject("VideoCamera");
                    videoCamera = videoCameraObj.AddComponent<Camera>();
                    videoCamera.depth = 999;
                    videoCamera.clearFlags = CameraClearFlags.SolidColor;
                    videoCamera.backgroundColor = Color.black;
                    videoCameraObj.SetActive(false);
                }
                videoPlayer.targetCamera = videoCamera;
            }

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

            videoPlayer.Prepare();
            videoPlayer.SetDirectAudioMute(0, true);      // <-- Silencia la pista de audio por defecto
        }
    }

    private void Update()
    {
        if (isPlayingCompletionVideo && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Se presionó ENTER. Omitiendo video y volviendo al menú...");
            StopVideo();
            VolverAlMenu();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            SaltarAMoleculaCompleta();
        }
    }

    private void SaltarAMoleculaCompleta()
    {
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

        Vector3 posicion = Vector3.zero;
        Quaternion rotacion = Quaternion.identity;

        if (currentMetano != null)
        {
            posicion = currentMetano.transform.position;
            rotacion = currentMetano.transform.rotation;
            Destroy(currentMetano);
            currentMetano = null;
        }
        else
        {
            posicion = new Vector3(0, 1, 0);
        }

        GameObject metanoCompleto = Instantiate(
            metanoStages[ultimaEtapa],
            posicion,
            rotacion,
            null
        );
        metanoCompleto.name = "Metano_Completo_Saltado";
        currentMetano = metanoCompleto;
        currentStage = ultimaEtapa;

        if (currentMetano.tag != "Metano")
            currentMetano.tag = "Metano";

        NivelCompletado();
    }

    private void OnDrawGizmos()
    {
        if (showDebugGizmos && currentMetano != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentMetano.transform.position, 0.2f);
        }
    }

    private void AgregarCollisionHandler()
    {
        if (currentMetano != null)
        {
            MetanoCollisionHandler handler = currentMetano.GetComponent<MetanoCollisionHandler>();
            if (handler == null)
            {
                handler = currentMetano.AddComponent<MetanoCollisionHandler>();
                handler.assembler = this;
                Debug.Log("MetanoAssembler: Añadido MetanoCollisionHandler al metano actual: " + currentMetano.name);
            }
        }
    }

    public void EvolucionarMetano(GameObject hidrogenoObj)
{
    if (currentMetano == null)
    {
        Debug.LogError("MetanoAssembler: No hay metano actual para evolucionar");
        return;
    }

    Debug.Log("MetanoAssembler: Hidrógeno consumido: " + hidrogenoObj.name);
    
    // Guarda la posición y rotación EXACTA antes de destruir el objeto
    Vector3 exactPosition = currentMetano.transform.position;
    Quaternion exactRotation = currentMetano.transform.rotation;
    Transform parentTransform = currentMetano.transform.parent;  // También conservamos el padre
    
    // Guarda la escala por si acaso
    Vector3 currentScale = currentMetano.transform.localScale;
    
    // NUEVO: Guarda información sobre si está siendo agarrado actualmente
    bool isCurrentlyHeld = (parentTransform != null && parentTransform.name.Contains("HoldPosition"));
    Transform holdParent = isCurrentlyHeld ? parentTransform : null;
    
    // NUEVO: Guarda todas las referencias y componentes importantes para el sistema de agarre
    Dictionary<string, Component> importantComponents = new Dictionary<string, Component>();
    Dictionary<string, object> importantValues = new Dictionary<string, object>();
    
    // Busca componentes que puedan estar relacionados con el sistema de agarre
    // (esto puede necesitar ajustes según tu sistema específico)
    Component[] allComponents = currentMetano.GetComponents<Component>();
    foreach (Component comp in allComponents)
    {
        if (comp == null) continue;
        
        string typeName = comp.GetType().Name;
        
        // Guarda componentes que podrían estar relacionados con el agarre
        // Ajusta estos nombres según los componentes que estés usando
        if (typeName.Contains("Grab") || typeName.Contains("Pickable") || 
            typeName.Contains("Holdable") || typeName.Contains("Interact") ||
            typeName.Contains("Item") || typeName.Contains("Object"))
        {
            importantComponents[typeName] = comp;
            Debug.Log("Guardando componente importante: " + typeName);
            
            // Intenta guardar campos públicos que podrían ser importantes
            System.Reflection.FieldInfo[] fields = comp.GetType().GetFields();
            foreach (var field in fields)
            {
                string key = typeName + "." + field.Name;
                object value = field.GetValue(comp);
                importantValues[key] = value;
                Debug.Log("Guardando valor: " + key);
            }
        }
    }
    
    Destroy(hidrogenoObj);  // Destruye el hidrógeno

    int nextStage = currentStage + 1;
    if (nextStage >= metanoStages.Length)
    {
        Debug.Log("MetanoAssembler: El metano ya está en su etapa final (Metano Completo)");
        return;
    }
    if (metanoStages[nextStage] == null)
    {
        Debug.LogError("MetanoAssembler: El prefab para la etapa " + nextStage + " no está asignado en el array");
        return;
    }

    GameObject oldMetano = currentMetano;
    currentMetano = null;
    
    // MODIFICADO: Instanciamos primero antes de destruir el viejo para poder copiar componentes
    GameObject newMetano = Instantiate(
        metanoStages[nextStage],
        exactPosition,  // Usa la posición guardada
        exactRotation,  // Usa la rotación guardada
        null  // Temporalmente sin padre para correcta inicialización
    );
    
    // Copiar componentes importantes del objeto original al nuevo
    foreach (var kvp in importantComponents)
    {
        string typeName = kvp.Key;
        Component originalComp = kvp.Value;
        
        // Busca si ya existe un componente del mismo tipo
        Component existingComp = null;
        Component[] newComponents = newMetano.GetComponents<Component>();
        foreach (Component comp in newComponents)
        {
            if (comp != null && comp.GetType().Name == typeName)
            {
                existingComp = comp;
                break;
            }
        }
        
        // Si no existe, intenta añadirlo
        if (existingComp == null)
        {
            try
            {
                System.Type compType = originalComp.GetType();
                existingComp = newMetano.AddComponent(compType);
                Debug.Log("Añadido componente: " + typeName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("No se pudo añadir componente " + typeName + ": " + e.Message);
            }
        }
        
        // Restaura valores de campos
        if (existingComp != null)
        {
            System.Reflection.FieldInfo[] fields = existingComp.GetType().GetFields();
            foreach (var field in fields)
            {
                string key = typeName + "." + field.Name;
                if (importantValues.ContainsKey(key))
                {
                    try
                    {
                        field.SetValue(existingComp, importantValues[key]);
                        Debug.Log("Restaurado valor: " + key);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("No se pudo restaurar valor " + key + ": " + e.Message);
                    }
                }
            }
        }
    }
    
    // Ahora podemos destruir el viejo
    Destroy(oldMetano);
    
    // Aplica la misma escala
    newMetano.transform.localScale = currentScale;
    
    // NUEVO: Si estaba siendo sostenido, mantén la relación padre-hijo
    if (isCurrentlyHeld && holdParent != null)
    {
        newMetano.transform.SetParent(holdParent, true);
        Debug.Log("La molécula estaba siendo sostenida. Restaurando parent: " + holdParent.name);
    }
    else if (parentTransform != null)
    {
        newMetano.transform.SetParent(parentTransform, true);
    }
    
    // Registra información de debug detallada
    Debug.Log("Posición exacta: " + exactPosition + " - Nueva posición: " + newMetano.transform.position);
    
    // Fuerza la posición nuevamente después de la instanciación por si acaso
    newMetano.transform.position = exactPosition;
    newMetano.transform.rotation = exactRotation;
    
    newMetano.SetActive(true);
    newMetano.name = "Metano_Etapa_" + nextStage;
    currentMetano = newMetano;
    currentStage = nextStage;

    if (currentMetano.tag != "Metano")
        currentMetano.tag = "Metano";

    Collider newCollider = currentMetano.GetComponent<Collider>();
    if (newCollider == null)
    {
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
        newRb = currentMetano.AddComponent<Rigidbody>();
    }
    
    // NUEVO: Configura el Rigidbody según si está siendo sostenido o no
    if (isCurrentlyHeld)
    {
        newRb.isKinematic = true;
        newRb.useGravity = false;
    }
    else
    {
        // Si no está siendo sostenido, usa la configuración estándar
        newRb.isKinematic = true;
        newRb.useGravity = false;
    }

    AgregarCollisionHandler();
    Debug.Log("MetanoAssembler: Metano evolucionado exitosamente a etapa " + currentStage +
              " (" + metanoStages[currentStage].name + ") en posición: " + newMetano.transform.position +
              ", Parent: " + (newMetano.transform.parent ? newMetano.transform.parent.name : "ninguno"));

    if (currentStage == metanoStages.Length - 1)
        NivelCompletado();

    if (showDebugGizmos)
    {
        Debug.DrawLine(currentMetano.transform.position,
                       currentMetano.transform.position + Vector3.up * 2.0f,
                       Color.magenta, 10.0f);
    }
}

    private void NivelCompletado()
    {
        Debug.Log("¡NIVEL COMPLETADO! El metano ha alcanzado su etapa final (metanocompleto)");
        PlayCompletionVideo();
    }

    /// <summary>
    /// Reproduce el video de finalización del nivel
    /// </summary>
    private void PlayCompletionVideo()
    {
        if (videoPlayer == null || string.IsNullOrEmpty(videoPlayer.url))
        {
            Debug.LogWarning("Video no configurado correctamente. Volviendo al menú principal.");
            StartCoroutine(VolverAlMenuDespuesDeEspera());
            return;
        }

        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(true);

        if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera != null)
            videoPlayer.targetCamera.gameObject.SetActive(true);

        // Reactivar audio antes de reproducir
        videoPlayer.SetDirectAudioMute(0, false);

        videoPlayer.Play();
        isPlayingCompletionVideo = true;

        Debug.Log("Presiona ENTER para omitir el video y volver al menú principal");
        CreateSkipVideoUI();
    }

    private void CreateSkipVideoUI()
    {
        if (videoPlayerCanvas != null)
        {
            Transform skipTextTrans = videoPlayerCanvas.transform.Find("SkipText");
            if (skipTextTrans == null)
            {
                GameObject skipTextObj = new GameObject("SkipText");
                skipTextObj.transform.SetParent(videoPlayerCanvas.transform, false);
                UnityEngine.UI.Text skipText = skipTextObj.AddComponent<UnityEngine.UI.Text>();
                skipText.text = "Presiona ENTER para omitir";
                skipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                skipText.fontSize = 24;
                skipText.color = Color.white;
                skipText.alignment = TextAnchor.LowerRight;

                RectTransform rectTransform = skipText.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(0.5f, 0);
                rectTransform.offsetMin = new Vector2(10, 10);
                rectTransform.offsetMax = new Vector2(-10, 40);
            }
        }
    }

    private void StopVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        isPlayingCompletionVideo = false;

        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(false);

        if (videoPlayer != null && videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera != null)
            videoPlayer.targetCamera.gameObject.SetActive(false);
    }

    private void VolverAlMenu()
    {
        StopAllCoroutines();
        Debug.Log("Cargando escena: " + menuInicialSceneName);
        SceneManager.LoadScene(menuInicialSceneName);
        StartCoroutine(ActivarGridNivelesDespuesDeCargarEscena());
    }

    private IEnumerator ActivarGridNivelesDespuesDeCargarEscena()
    {
        yield return null;
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform gridNiveles = canvas.transform.Find(gridNivelesName);
            if (gridNiveles != null)
            {
                Debug.Log("Activando grid_niveles en el menú");
                gridNiveles.gameObject.SetActive(true);
                UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem != null)
                    eventSystem.gameObject.SetActive(true);
                else
                    Debug.LogWarning("No se encontró un EventSystem en la escena. La interacción UI puede no funcionar correctamente.");
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

    private IEnumerator VolverAlMenuDespuesDeEspera()
    {
        Debug.Log("Volviendo al menú de niveles en " + tiempoEsperaFinNivel + " segundos...");
        yield return new WaitForSeconds(tiempoEsperaFinNivel);
        VolverAlMenu();
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (assembler == null)
        {
            Debug.LogError("MetanoCollisionHandler: No hay referencia al MetanoAssembler en " + gameObject.name);
            return;
        }

        if (other.CompareTag("Hidrogeno"))
        {
            Debug.Log("MetanoCollisionHandler: " + gameObject.name + " detectó colisión con hidrógeno: " + other.gameObject.name);
            assembler.EvolucionarMetano(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
