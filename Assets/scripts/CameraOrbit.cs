using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;  // Objeto al que la cámara seguirá
    public float rotationSpeed = 100f;  // Velocidad de rotación
    public float distance = 10f; // Distancia de la cámara al objeto
    public float minDistance = 3f; // Distancia mínima de la cámara
    public float maxDistance = 20f; // Distancia máxima de la cámara
    public float zoomSpeed = 5f; // Velocidad del zoom
    public float height = 5f; // Altura de la cámara en vista isométrica
    private float angle = 0f;
    private bool isTopView = false; // Para alternar vista

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("No se ha asignado un objetivo para la cámara.");
            return;
        }

        if (Input.GetKey(KeyCode.Q)) // Rotar a la izquierda
        {
            RotateCamera(1);
        }
        if (Input.GetKey(KeyCode.E)) // Rotar a la derecha
        {
            RotateCamera(-1);
        }
        if (Input.GetKeyDown(KeyCode.R)) // Cambiar a vista cenital
        {
            ToggleView();
        }

        if (Input.GetKey(KeyCode.UpArrow)) // Acercar la cámara
        {
            AdjustDistance(-zoomSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow)) // Alejar la cámara
        {
            AdjustDistance(zoomSpeed * Time.deltaTime);
        }
    }

    public void RotateCamera(int direction)
    {
        if (isTopView) return; // No rotar si está en vista cenital

        angle += direction * rotationSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(30, angle, 0); // Vista isométrica con inclinación de 30°
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    void ToggleView()
    {
        isTopView = !isTopView; // Cambiar entre isométrica y cenital
        if (isTopView)
        {
            transform.position = target.position + new Vector3(0, distance, 0); // Vista cenital
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else
        {
            RotateCamera(0); // Volver a la vista isométrica
        }
    }

    void AdjustDistance(float delta)
    {
        distance = Mathf.Clamp(distance + delta, minDistance, maxDistance); // Limita la distancia
        RotateCamera(0); // Aplica la nueva distancia sin cambiar la orientación
    }
}
