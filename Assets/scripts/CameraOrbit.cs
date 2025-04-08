using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 100f;
    public float distance = 10f;
    public float minDistance = 3f;
    public float maxDistance = 20f;
    public float zoomSpeed = 5f;
    public float height = 5f;

    public float smoothTime = 0.2f; // Suavizado
    private Vector3 velocity = Vector3.zero;

    private float angle = 0f;
    private bool isTopView = false;

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("No se ha asignado un objetivo para la cámara.");
            return;
        }

        // Rotación con teclas Q y E
        if (Input.GetKey(KeyCode.Q))
            angle += rotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            angle -= rotationSpeed * Time.deltaTime;

        // Zoom con scroll del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Vista cenital (por ejemplo, con tecla 'T')
        if (Input.GetKeyDown(KeyCode.T))
            isTopView = !isTopView;

        Vector3 desiredPosition;

        if (isTopView)
        {
            desiredPosition = target.position + Vector3.up * distance;
        }
        else
        {
            // Posición deseada con rotación
            float radians = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(radians) * distance;
            float z = Mathf.Cos(radians) * distance;
            desiredPosition = target.position + new Vector3(x, height, z);
        }

        // Suavizado de movimiento
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }
}