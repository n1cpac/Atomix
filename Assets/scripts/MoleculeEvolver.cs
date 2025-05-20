using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Necesario para reproducir videos

/// <summary>
/// MoleculeEvolver - Versión mejorada del MetanoAssembler que garantiza que los objetos
/// evolucionen correctamente y maneja correctamente los pivotes y la interacción con el jugador.
/// </summary>
public class MoleculeEvolver : MonoBehaviour
{
    [Tooltip("Array con los prefabs de las diferentes etapas de la molécula")]
    public GameObject[] moleculeStages;  // Arrastrar los prefabs de las etapas

    [Tooltip("Referencia a la instancia actual de la molécula en la escena")]
    public GameObject currentMolecule;   // Referencia a la instancia actual de la molécula

    [Tooltip("Etapa actual de la molécula")]
    [SerializeField] private int currentStage = 0;

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

    [Tooltip("Vector de desplazamiento para corregir el pivote (personaliza según tu modelo)")]
    public Vector3 pivotOffset = Vector3.zero;

    [Tooltip("Si es verdadero, recentrará automáticamente el pivote")]
    public bool autoCenterPivot = true;

    // VideoPlayer que manejará la reproducción del video
    private VideoPlayer videoPlayer;

    // Indica si se está reproduciendo el video de finalización
    private bool isPlayingCompletionVideo = false;

    // Posición del objeto HoldPosition actual
    private Vector3 lastHoldPosition;
    private Quaternion lastHoldRotation;
    private bool wasHeld = false;

    /// <summary>
    /// Inicializa el componente y verifica que todas las referencias estén configuradas correctamente.
    /// </summary>
    private void Start()
    {
        // Validar que el array moleculeStages tenga elementos
        if (moleculeStages == null || moleculeStages.Length == 0)
        {
            Debug.LogError("MoleculeEvolver: El array de etapas de la molécula está vacío. Por favor, arrastra los prefabs al inspector.");
            return;
        }

        // Validar que currentMolecule esté asignado
        if (currentMolecule == null)
        {
            Debug.LogWarning("MoleculeEvolver: No se ha asignado una molécula inicial. Buscando en la escena...");
            currentMolecule = GameObject.FindWithTag("Molecule");
            if (currentMolecule == null)
            {
                Debug.LogError("MoleculeEvolver: No se encontró una molécula inicial en la escena. Por favor, asigna una manualmente.");
                return;
            }
            else
            {
                Debug.Log("MoleculeEvolver: Se encontró automáticamente una molécula inicial en la escena.");
            }
        }

        if (autoCenterPivot)
        {
            // Intenta calcular un offset de pivote basado en los renderers
            pivotOffset = CalculateCenterOffset(currentMolecule);
            Debug.Log("Offset de pivote calculado: " + pivotOffset);
        }

        // Verificar collider
        Collider moleculeCollider = currentMolecule.GetComponent<Collider>();
        if (moleculeCollider == null)
        {
            Debug.LogWarning("MoleculeEvolver: La molécula inicial no tiene un Collider. Añadiendo un SphereCollider...");
            SphereCollider sc = currentMolecule.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1.0f;
        }
        else if (!moleculeCollider.isTrigger)
        {
            Debug.LogWarning("MoleculeEvolver: El Collider de la molécula inicial no es un trigger. Cambiando a isTrigger=true...");
            moleculeCollider.isTrigger = true;
        }

        // Verificar Rigidbody
        Rigidbody moleculeRb = currentMolecule.GetComponent<Rigidbody>();
        if (moleculeRb == null)
        {
            Debug.LogWarning("MoleculeEvolver: La molécula inicial no tiene un Rigidbody. Añadiendo uno...");
            moleculeRb = currentMolecule.AddComponent<Rigidbody>();
            moleculeRb.isKinematic = true;
            moleculeRb.useGravity = false;
        }

        // Tag
        if (currentMolecule.tag != "Molecule")
        {
            Debug.LogWarning("MoleculeEvolver: La molécula inicial no tiene el tag 'Molecule'. Cambiando tag...");
            currentMolecule.tag = "Molecule";
        }

        // Handler y video
        AgregarCollisionHandler();
        SetupVideoPlayer();

        if (videoPlayerCanvas != null)
            videoPlayerCanvas.SetActive(false);

        Debug.Log("MoleculeEvolver: Inicializado. Etapa actual: " + currentStage);

        if (showDebugGizmos && currentMolecule != null)
        {
            Debug.DrawLine(currentMolecule.transform.position,
                           currentMolecule.transform.position + Vector3.up * 2.0f,
                           Color.green, 10.0f);
        }
    }

    /// <summary>
    /// Calcula el centro real del objeto basado en sus renderers
    /// </summary>
    private Vector3 CalculateCenterOffset(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No se encontraron renderers para calcular el centro del objeto");
            return Vector3.zero;
        }

        // Calcula los límites combinados de todos los renderers
        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // El offset es la diferencia entre el centro del bounds y la posición del objeto
        Vector3 offset = bounds.center - obj.transform.position;
        Debug.Log("Centro calculado basado en renderers: " + bounds.center + 
                  ", Offset desde pivote actual: " + offset);
                  
        return offset;
    }

    /// <summary>
    /// Configura el reproductor de video
    /// </summary>
    private void SetupVideoPlayer()
    {
        // Configuración del reproductor de video - mismo código que antes
        // ...
    }

    private void Update()
    {
        // Registra si el objeto está siendo sostenido
        CheckIfObjectIsHeld();

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

    /// <summary>
    /// Verifica si el objeto está siendo sostenido por el jugador
    /// </summary>
    private void CheckIfObjectIsHeld()
    {
        if (currentMolecule == null) return;

        Transform parent = currentMolecule.transform.parent;
        bool isHeldNow = (parent != null && IsHoldPosition(parent));

        if (isHeldNow && !wasHeld)
        {
            // Acaba de ser agarrado
            wasHeld = true;
            lastHoldPosition = parent.position;
            lastHoldRotation = parent.rotation;
            Debug.Log("Objeto agarrado por: " + parent.name);
        }
        else if (!isHeldNow && wasHeld)
        {
            // Acaba de ser soltado
            wasHeld = false;
            Debug.Log("Objeto soltado");
        }
        else if (isHeldNow)
        {
            // Sigue agarrado, actualiza posición
            lastHoldPosition = parent.position;
            lastHoldRotation = parent.rotation;
        }
    }

    /// <summary>
    /// Determina si una transform es un punto de agarre
    /// </summary>
    private bool IsHoldPosition(Transform transform)
    {
        // Ajusta esta función según como identifiques los puntos de agarre en tu juego
        return transform.name.Contains("Hold") || transform.name.Contains("Grab") || 
               transform.name.Contains("Hand") || transform.name.Contains("Pick");
    }

    private void SaltarAMoleculaCompleta()
    {
        // Código original para saltar a la molécula completa
        // ...
    }

    private void OnDrawGizmos()
    {
        if (showDebugGizmos && currentMolecule != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentMolecule.transform.position, 0.2f);
            
            // Dibuja también el pivote corregido
            Vector3 correctedPivot = currentMolecule.transform.position + pivotOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(correctedPivot, 0.15f);
            Gizmos.DrawLine(currentMolecule.transform.position, correctedPivot);
        }
    }

    private void AgregarCollisionHandler()
    {
        if (currentMolecule != null)
        {
            MoleculeCollisionHandler handler = currentMolecule.GetComponent<MoleculeCollisionHandler>();
            if (handler == null)
            {
                handler = currentMolecule.AddComponent<MoleculeCollisionHandler>();
                handler.evolver = this;
                Debug.Log("MoleculeEvolver: Añadido MoleculeCollisionHandler a la molécula actual: " + currentMolecule.name);
            }
        }
    }

    /// <summary>
    /// Este es el método principal para evolucionar la molécula cuando colisiona con otra molécula
    /// </summary>
    public void EvolucionarMolecula(GameObject otherMolecule)
    {
        if (currentMolecule == null)
        {
            Debug.LogError("MoleculeEvolver: No hay molécula actual para evolucionar");
            return;
        }

        Debug.Log("MoleculeEvolver: Molécula consumida: " + otherMolecule.name);
        
        // Guarda la posición y rotación EXACTA antes de destruir el objeto
        Vector3 exactPosition = currentMolecule.transform.position;
        Quaternion exactRotation = currentMolecule.transform.rotation;
        
        // Guarda la escala
        Vector3 currentScale = currentMolecule.transform.localScale;
        
        // Detecta si está siendo sostenido y por quién
        Transform parentTransform = currentMolecule.transform.parent;
        bool isCurrentlyHeld = wasHeld && (parentTransform != null && IsHoldPosition(parentTransform));
        Transform holdParent = isCurrentlyHeld ? parentTransform : null;
        
        // Guarda la posición local si está siendo sostenido (importante para el punto de agarre)
        Vector3 localPosition = currentMolecule.transform.localPosition;
        Quaternion localRotation = currentMolecule.transform.localRotation;
        
        // Esta es la posición visual central del objeto (considerando el offset del pivote)
        Vector3 centerPosition = exactPosition + pivotOffset;
        
        // Guarda todos los componentes importantes
        Dictionary<System.Type, Component> importantComponents = new Dictionary<System.Type, Component>();
        Component[] allComponents = currentMolecule.GetComponents<Component>();
        foreach (Component comp in allComponents)
        {
            if (comp == null) continue;
            
            // Ignora componentes básicos que serán reconfigurados
            if (comp is Transform || comp is Collider || comp is MoleculeCollisionHandler)
                continue;
                
            importantComponents[comp.GetType()] = comp;
        }
        
        // Guarda el contenido del otherMolecule antes de destruirlo
        GameObject consumedMoleculeType = otherMolecule;
        
        // Destruye la molécula consumida
        Destroy(otherMolecule);

        int nextStage = currentStage + 1;
        if (nextStage >= moleculeStages.Length)
        {
            Debug.Log("MoleculeEvolver: La molécula ya está en su etapa final");
            return;
        }
        
        if (moleculeStages[nextStage] == null)
        {
            Debug.LogError("MoleculeEvolver: El prefab para la etapa " + nextStage + " no está asignado en el array");
            return;
        }

        // Guarda una referencia a la molécula actual antes de destruirla
        GameObject oldMolecule = currentMolecule;
        currentMolecule = null;
        
        // Crea un GameObject vacío temporal en la posición centro EXACTA
        GameObject pivotCorrector = new GameObject("PivotCorrector");
        pivotCorrector.transform.position = centerPosition;
        pivotCorrector.transform.rotation = exactRotation;
        
        if (isCurrentlyHeld && holdParent != null)
        {
            // Si estaba siendo sostenido, coloca el corrector bajo el mismo padre
            pivotCorrector.transform.SetParent(holdParent);
            pivotCorrector.transform.localPosition = localPosition + pivotOffset;
            pivotCorrector.transform.localRotation = localRotation;
        }
        
        // Instancia la nueva molécula como hijo del corrector
        GameObject newMolecule = Instantiate(
            moleculeStages[nextStage],
            pivotCorrector.transform.position,
            pivotCorrector.transform.rotation,
            pivotCorrector.transform
        );
        
        // El nuevo objeto aparecerá en la misma posición pero con su pivote "corregido"
        // Ahora compensamos el offset del nuevo modelo
        if (autoCenterPivot)
        {
            // Recalcula el offset para el nuevo modelo
            Vector3 newOffset = CalculateCenterOffset(newMolecule);
            newMolecule.transform.localPosition = -newOffset;
            Debug.Log("Nuevo offset calculado: " + newOffset + ". Aplicando compensación.");
        }
        else
        {
            // Usa el mismo offset configurado
            newMolecule.transform.localPosition = -pivotOffset;
        }
        
        // Aplica la escala correspondiente
        newMolecule.transform.localScale = currentScale;
        
        // Copia todos los componentes importantes
        foreach (var kvp in importantComponents)
        {
            System.Type componentType = kvp.Key;
            Component originalComponent = kvp.Value;
            
            // Verifica si ya existe este componente
            Component existingComponent = newMolecule.GetComponent(componentType);
            
            if (existingComponent == null)
            {
                try {
                    // Intenta añadir el componente
                    existingComponent = newMolecule.AddComponent(componentType);
                    Debug.Log("Componente copiado: " + componentType.Name);
                }
                catch (System.Exception e) {
                    Debug.LogWarning("No se pudo añadir componente " + componentType.Name + ": " + e.Message);
                }
            }
            
            // Si se creó correctamente, copia los valores
            if (existingComponent != null && originalComponent != null)
            {
                // Copia valores públicos usando reflection
                System.Reflection.FieldInfo[] fields = componentType.GetFields();
                foreach (var field in fields)
                {
                    try {
                        object value = field.GetValue(originalComponent);
                        field.SetValue(existingComponent, value);
                    }
                    catch (System.Exception) { 
                        // Ignora errores al copiar valores
                    }
                }
            }
        }
        
        // Ahora podemos eliminar el objeto antiguo
        Destroy(oldMolecule);
        
        // Prepara la nueva molécula
        newMolecule.name = "Molecule_Stage_" + nextStage;
        
        // Configura el tag
        if (newMolecule.tag != "Molecule")
            newMolecule.tag = "Molecule";
        
        // Desacopla del corrector de pivote
        newMolecule.transform.SetParent(pivotCorrector.transform.parent);
        
        // Guarda la referencia a la nueva molécula
        currentMolecule = newMolecule;
        currentStage = nextStage;
        
        // Elimina el objeto corrector de pivote
        Destroy(pivotCorrector);
        
        // Asegura que tenga los componentes necesarios
        Collider newCollider = currentMolecule.GetComponent<Collider>();
        if (newCollider == null)
        {
            SphereCollider sc = currentMolecule.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1.0f;
        }
        
        Rigidbody newRb = currentMolecule.GetComponent<Rigidbody>();
        if (newRb == null)
        {
            newRb = currentMolecule.AddComponent<Rigidbody>();
        }
        
        // Configura el rigidbody según si está siendo sostenido
        if (isCurrentlyHeld)
        {
            newRb.isKinematic = true;
            newRb.useGravity = false;
        }
        
        // Agrega el handler de colisiones
        AgregarCollisionHandler();
        
        Debug.Log("MoleculeEvolver: Molécula evolucionada exitosamente a etapa " + currentStage);
        
        // Si es la última etapa, completa el nivel
        if (currentStage == moleculeStages.Length - 1)
            NivelCompletado();
        
        if (showDebugGizmos)
        {
            Debug.DrawLine(currentMolecule.transform.position,
                           currentMolecule.transform.position + Vector3.up * 2.0f,
                           Color.magenta, 10.0f);
        }
    }

    private void NivelCompletado()
    {
        Debug.Log("¡NIVEL COMPLETADO! La molécula ha alcanzado su etapa final");
        PlayCompletionVideo();
    }

    private void PlayCompletionVideo()
    {
        // Código para reproducir video - mismo código que antes
        // ...
    }

    private void CreateSkipVideoUI()
    {
        // Código para UI de saltar video - mismo código que antes
        // ...
    }

    private void StopVideo()
    {
        // Código para detener video - mismo código que antes
        // ...
    }

    private void VolverAlMenu()
    {
        // Código para volver al menú - mismo código que antes
        // ...
    }

    private IEnumerator ActivarGridNivelesDespuesDeCargarEscena()
    {
        // Código para activar grid niveles - mismo código que antes
        // ...
        yield return null;
    }

    private IEnumerator VolverAlMenuDespuesDeEspera()
    {
        Debug.Log("Volviendo al menú de niveles en " + tiempoEsperaFinNivel + " segundos...");
        yield return new WaitForSeconds(tiempoEsperaFinNivel);
        VolverAlMenu();
    }

    public void MostrarEstadoActual()
    {
        if (currentMolecule != null)
        {
            Debug.Log("MoleculeEvolver - Estado actual:" +
                      "\n- Etapa: " + currentStage +
                      "\n- Prefab: " + moleculeStages[currentStage].name +
                      "\n- Instancia: " + currentMolecule.name +
                      "\n- Posición: " + currentMolecule.transform.position +
                      "\n- Offset de pivote: " + pivotOffset +
                      "\n- Centro visual: " + (currentMolecule.transform.position + pivotOffset) +
                      "\n- Está siendo sostenido: " + wasHeld);
        }
        else
        {
            Debug.LogWarning("MoleculeEvolver: No hay molécula actual asignada.");
        }
    }
}

/// <summary>
/// Componente auxiliar que se añade a cada instancia de la molécula para manejar sus colisiones.
/// </summary>
public class MoleculeCollisionHandler : MonoBehaviour
{
    [HideInInspector]
    public MoleculeEvolver evolver;  // Referencia al MoleculeEvolver principal

    private void OnTriggerEnter(Collider other)
    {
        if (evolver == null)
        {
            Debug.LogError("MoleculeCollisionHandler: No hay referencia al MoleculeEvolver en " + gameObject.name);
            return;
        }

        // Adaptar esta condición según tu juego
        if (other.CompareTag("Hidrogeno") || other.CompareTag("Reactivo") || other.CompareTag("Atom"))
        {
            Debug.Log("MoleculeCollisionHandler: " + gameObject.name + " detectó colisión con: " + other.gameObject.name);
            evolver.EvolucionarMolecula(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}