using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private int currentStage = 0;      // La etapa actual del metano (comienza en 0)

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

        // Agregar el componente MetanoCollisionHandler al metano actual
        AgregarCollisionHandler();
        
        Debug.Log("MetanoAssembler: Inicializado. Etapa actual: " + currentStage);
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
                Debug.Log("MetanoAssembler: Añadido MetanoCollisionHandler al metano actual");
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
        Destroy(hidrogenoObj);
        Debug.Log("MetanoAssembler: Hidrógeno consumido");

        // Calcular la próxima etapa
        int nextStage = currentStage + 1;
        
        // Verificar si hemos alcanzado el límite máximo de etapas
        if (nextStage >= metanoStages.Length)
        {
            Debug.Log("MetanoAssembler: El metano ya está en su etapa final (Metano Completo)");
            return;
        }

        // Guardar la posición y rotación actuales
        Vector3 currentPosition = currentMetano.transform.position;
        Quaternion currentRotation = currentMetano.transform.rotation;
        
        // Desactivar o destruir el metano actual
        Destroy(currentMetano);
        Debug.Log("MetanoAssembler: Metano en etapa " + currentStage + " desactivado");
        
        // Instanciar la nueva etapa del metano
        GameObject newMetano = Instantiate(
            metanoStages[nextStage], 
            currentPosition, 
            currentRotation
        );
        
        Debug.Log("MetanoAssembler: Instanciado nuevo metano en etapa " + nextStage);
        
        // Actualizar la referencia al metano actual
        currentMetano = newMetano;
        
        // Actualizar la etapa actual
        currentStage = nextStage;
        
        // Añadir el componente de manejo de colisiones al nuevo metano
        AgregarCollisionHandler();
        
        Debug.Log("MetanoAssembler: Metano evolucionado a etapa " + currentStage + 
                  " (" + metanoStages[currentStage].name + ")");
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
                      "\n- Posición: " + currentMetano.transform.position);
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
            Debug.LogError("MetanoCollisionHandler: No hay referencia al MetanoAssembler");
            return;
        }
        
        // Verificar si el objeto que entró es un hidrógeno
        if (other.CompareTag("Hidrogeno"))
        {
            Debug.Log("MetanoCollisionHandler: Detectada colisión con hidrógeno");
            assembler.EvolucionarMetano(other.gameObject);
        }
    }
}