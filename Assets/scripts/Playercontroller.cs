using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float rotationSpeed = 200f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public Camera mainCamera;
    public Transform topDownView;

    private Rigidbody rb;
    private Vector3 inputDirection;
    private bool isGrounded;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isTopDownActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isTopDownActive = !isTopDownActive;

            if (!isTopDownActive)
            {
                DeactivateTopDownView();
            }
        }

        if (!isTopDownActive)
        {
            // Intercambiar controles: Derecha = Avanzar, Izquierda = Retroceder
            float vertical = Input.GetAxis("Horizontal");
            float horizontal = -Input.GetAxis("Vertical");

            inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }
        else
        {
            inputDirection = Vector3.zero;

            // MANTENER la posición de cámara top-down en cada frame
            mainCamera.transform.position = topDownView.position;
            mainCamera.transform.rotation = topDownView.rotation;
        }
    }
    void FixedUpdate()
    {
        CheckGround();

        if (inputDirection.magnitude > 0.01f)
        {
            Vector3 targetVelocity = inputDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void CheckGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.1f, groundLayer);
    }

    void ActivateTopDownView()
    {
        mainCamera.transform.SetParent(topDownView); // Se vuelve hija del topDownView
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;
    }

    void DeactivateTopDownView()
    {
        mainCamera.transform.SetParent(null); // La separamos
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;
    }
}


