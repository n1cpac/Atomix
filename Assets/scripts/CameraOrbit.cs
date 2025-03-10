using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;  // Objeto al que la cámara seguirá (ej. "Cube")
    public float rotationSpeed = 100f;  // Velocidad de rotación de la cámara
    public float distance = 10f;        // Distancia de la cámara al target
    public float minDistance = 3f;      // Distancia mínima permitida
    public float maxDistance = 20f;     // Distancia máxima permitida
    public float zoomSpeed = 5f;        // Velocidad de zoom
    public float height = 5f;           // Altura de la cámara en vista isométrica
    private float angle = 0f;
    private bool isTopView = false;     // Alternar vista entre isométrica y cenital

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("No se ha asignado un objetivo para la cámara.");
            return;
        }

        // Rotación con teclas Q y E
        if (Input.GetKey(KeyCode.Q))
        {
            RotateCamera(1f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            RotateCamera(-1f);
        }

        // Rotación con el joystick derecho horizontal
        float rightStick = Input.GetAxis("RightStickHorizontal");
        if (!Mathf.Approximately(rightStick, 0f))
        {
            RotateCamera(rightStick);
        }

        // Alternar vista cenital usando R o el botón del joystick derecho (JoystickButton9)
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            ToggleView();
        }

        // Zoom controlado con el joystick derecho vertical
        float rightStickVertical = Input.GetAxis("RightStickVertical");
        if (!Mathf.Approximately(rightStickVertical, 0f))
        {
            AdjustDistance(rightStickVertical * zoomSpeed * Time.deltaTime);
        }

        // Zoom controlado con las teclas O (acercar) y P (alejar)
        if (Input.GetKey(KeyCode.O))
        {
            AdjustDistance(-zoomSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.P))
        {
            AdjustDistance(zoomSpeed * Time.deltaTime);
        }
    }

    public void RotateCamera(float delta)
    {
        if (isTopView) return; // No se rota en vista cenital

        angle += delta * rotationSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(30, angle, 0); // Vista isométrica con inclinación de 30°
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    void ToggleView()
    {
        isTopView = !isTopView; // Alterna entre vista isométrica y cenital
        if (isTopView)
        {
            transform.position = target.position + new Vector3(0, distance, 0); // Vista cenital centrada en el target
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else
        {
            RotateCamera(0); // Vuelve a la vista isométrica
        }
    }

    void AdjustDistance(float delta)
    {
        distance = Mathf.Clamp(distance + delta, minDistance, maxDistance);
        RotateCamera(0); // Aplica la nueva distancia sin cambiar la orientación
    }
}
