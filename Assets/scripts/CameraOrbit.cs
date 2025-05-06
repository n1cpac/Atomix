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
    public float smoothTime = 0.2f;

    private float angle = 0f;
    private bool isTopView = false;
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (!target)
            return;

        // ---- Rotación isométrica ----
        // Q/E o joystick derecho horizontal
        float h = Input.GetKey(KeyCode.Q) ? 1f :
                  Input.GetKey(KeyCode.E) ? -1f : 0f;
        h += Input.GetAxis("RightStickHorizontal");
        angle += h * rotationSpeed * Time.deltaTime;

        // ---- Zoom ----
        float z = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        z -= Input.GetAxis("RightStickVertical") * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance + z, minDistance, maxDistance);

        // ---- Alternar vista cenital ----
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.JoystickButton9))
            isTopView = !isTopView;

        // ---- Calcula posición deseada ----
        Vector3 desired;
        if (isTopView)
        {
            desired = target.position + Vector3.up * distance;
        }
        else
        {
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(rad) * distance;
            float zOff = Mathf.Cos(rad) * distance;
            desired = target.position + new Vector3(x, height, zOff);
        }

        // ---- Move & LookAt con suavizado ----
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }
}