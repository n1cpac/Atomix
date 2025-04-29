using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movimiento
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

    // Sonido
    public AudioSource pasos;
    private bool Hactivo;
    private bool Vactivo;
    public float minDistance = 5f;    // Distancia mínima para volumen máximo
    public float maxDistance = 20f;   // Distancia a la que el volumen será mínimo

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        // Configuración inicial del AudioSource (si no se asigna manualmente)
        if (pasos != null)
        {
            pasos.spatialBlend = 0f;  // Modo 2D para panorama estéreo
            pasos.loop = true;        // Para sonido continuo al caminar
            pasos.playOnAwake = false;
        }
    }

    void Update()
    {
        // Alternar vista con R
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
            // Controles invertidos: Derecha = Avanzar, Izquierda = Retroceder
            float vertical = Input.GetAxis("Horizontal");
            float horizontal = -Input.GetAxis("Vertical");

            inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }
        else
        {
            inputDirection = Vector3.zero;
            mainCamera.transform.position = topDownView.position;
            mainCamera.transform.rotation = topDownView.rotation;
        }

        // Control de sonido de pasos
        HandleFootsteps();

        // Ajustar panorama y volumen basado en la posición relativa a la cámara
        if (pasos != null && pasos.isPlaying)
        {
            Vector3 cameraRelativePos = mainCamera.transform.InverseTransformPoint(transform.position);
            
            // Panorama estéreo (-1 = izquierda, 1 = derecha)
            float stereoPan = Mathf.Clamp(cameraRelativePos.x / 2f, -1f, 1f);
            pasos.panStereo = stereoPan;

            // Volumen basado en distancia
            float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
            pasos.volume = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(maxDistance, minDistance, distanceToCamera));
        }
    }

    void HandleFootsteps()
    {
        if (Input.GetButtonDown("Horizontal"))
        {
            Hactivo = true;
            if (!pasos.isPlaying) pasos.Play();
        }
        if (Input.GetButtonDown("Vertical"))
        {
            Vactivo = true;
            if (!pasos.isPlaying) pasos.Play();
        }

        if (Input.GetButtonUp("Horizontal"))
        {
            Hactivo = false;
            if (Vactivo == false) pasos.Pause();
        }

        if (Input.GetButtonUp("Vertical"))
        {
            Vactivo = false;
            if (Hactivo == false) pasos.Pause();
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
        mainCamera.transform.SetParent(topDownView);
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;
    }

    void DeactivateTopDownView()
    {
        mainCamera.transform.SetParent(null);
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;
    }
}   