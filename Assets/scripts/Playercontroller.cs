using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    // Movimiento
    public float moveSpeed = 70f;
    public float jumpForce = 50f;
    public float rotationSpeed = 100f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    // Cámara
    public Camera mainCamera;
    public Transform topDownView;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isTopDownActive = false;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    private float currentZoom;

    // Interacción
    public Transform holdPosition;
    public float pickupRange = 10f;
    public float dropDistance = 1.5f;
    public Vector3 holdOffset = new Vector3(0, 1.0f, 0);
    private GameObject heldMolecule = null;

    // Audio
    public AudioSource pasos;
    public float minDistance = 5f;
    public float maxDistance = 20f;
    private bool Hactivo;
    private bool Vactivo;
    private bool rightStickHactivo;
    private bool rightStickVactivo;

    // Vibración
    public float vibrationDuration = 0.2f;
    public float vibrationLowFrequency = 0.5f;
    public float vibrationHighFrequency = 0.5f;

    // Interno
    private Rigidbody rb;
    private Vector3 inputDirection;
    private bool isGrounded;
    private float rightStickX;
    private float rightStickY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;
        currentZoom = Vector3.Distance(mainCamera.transform.position, transform.position);

        if (pasos != null)
        {
            pasos.spatialBlend = 0f;
            pasos.loop = true;
            pasos.playOnAwake = false;
        }
    }

    void Update()
    {
        // Cambio de vista cenital/ortográfica
        if (Input.GetKeyDown(KeyCode.R) || IsRightStickPressed())
        {
            isTopDownActive = !isTopDownActive;
            if (!isTopDownActive)
                DeactivateTopDownView();
            else
                ActivateTopDownView();
        }

        if (!isTopDownActive)
        {
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

        HandleFootsteps();

        if (pasos != null && pasos.isPlaying)
        {
            Vector3 cameraRelativePos = mainCamera.transform.InverseTransformPoint(transform.position);
            pasos.panStereo = Mathf.Clamp(cameraRelativePos.x / 2f, -1f, 1f);
            float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
            pasos.volume = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(maxDistance, minDistance, distanceToCamera));
        }

        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
            else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;
        }

        // Controlamos la rotación con teclado y con joystick derecho
if (isGrounded)
{
    transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);
}

        // Zoom con joystick derecho (vertical)
        rightStickY = Input.GetAxis("RightStickVertical");
        if (Mathf.Abs(rightStickY) > 0.1f && !isTopDownActive)
        {
            currentZoom -= rightStickY * zoomSpeed * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            
            // Aplicar zoom a la cámara
            Vector3 direction = (mainCamera.transform.position - transform.position).normalized;
            mainCamera.transform.position = transform.position + direction * currentZoom;
        }

        // Salto con espacio o botón A del mando
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (heldMolecule == null) PickUpMolecule();
            else DropMolecule();
        }

        if (heldMolecule != null)
            heldMolecule.transform.position = holdPosition.position + holdOffset;
    }

    void FixedUpdate()
    {
        CheckGround();

        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
            else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;
        }

        Vector3 horizontalVelocity = transform.forward * verticalInput * moveSpeed;
        horizontalVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = horizontalVelocity;
    }

    void HandleFootsteps()
    {
        // Detección de entradas por teclado
        if (Input.GetButtonDown("Horizontal")) { Hactivo = true; if (!pasos.isPlaying) pasos.Play(); }
        if (Input.GetButtonDown("Vertical")) { Vactivo = true; if (!pasos.isPlaying) pasos.Play(); }
        if (Input.GetButtonUp("Horizontal")) { Hactivo = false; if (!Vactivo && !rightStickHactivo && !rightStickVactivo) pasos.Pause(); }
        if (Input.GetButtonUp("Vertical")) { Vactivo = false; if (!Hactivo && !rightStickHactivo && !rightStickVactivo) pasos.Pause(); }

        // Detección de entradas por mando - usando los mismos ejes que ya se están usando para el movimiento
        float leftStickX = Input.GetAxis("Horizontal");
        float leftStickY = Input.GetAxis("Vertical");

        // Si hay movimiento significativo en el mando
        if (Mathf.Abs(leftStickX) > 0.1f)
        {
            if (!rightStickHactivo)
            {
                rightStickHactivo = true;
                if (!pasos.isPlaying) pasos.Play();
            }
        }
        else
        {
            if (rightStickHactivo)
            {
                rightStickHactivo = false;
                if (!Hactivo && !Vactivo && !rightStickVactivo) pasos.Pause();
            }
        }

        if (Mathf.Abs(leftStickY) > 0.1f)
        {
            if (!rightStickVactivo)
            {
                rightStickVactivo = true;
                if (!pasos.isPlaying) pasos.Play();
            }
        }
        else
        {
            if (rightStickVactivo)
            {
                rightStickVactivo = false;
                if (!Hactivo && !Vactivo && !rightStickHactivo) pasos.Pause();
            }
        }
    }

    // Verifica si el joystick derecho está siendo presionado
    bool IsRightStickPressed()
    {
    #if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
        {
            return Gamepad.current.rightStickButton.wasPressedThisFrame;
        }
    #endif
        return Input.GetKeyDown(KeyCode.JoystickButton8) || Input.GetButtonDown("RightStickClick"); // Joystick derecho suele ser botón 8 o 9 dependiendo del mando
    }

    void CheckGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.1f, groundLayer);
    }

    private void PickUpMolecule()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("molecule"))
            {
                heldMolecule = col.gameObject;
                heldMolecule.transform.SetParent(holdPosition);
                Collider moleculeCollider = heldMolecule.GetComponent<Collider>();
                if (moleculeCollider != null) moleculeCollider.enabled = false;
                break;
            }
        }
    }

    private void DropMolecule()
    {
        if (heldMolecule != null)
        {
            heldMolecule.transform.SetParent(null);
            heldMolecule.transform.position = transform.position + transform.forward * dropDistance;
            Collider moleculeCollider = heldMolecule.GetComponent<Collider>();
            if (moleculeCollider != null) moleculeCollider.enabled = true;
            heldMolecule = null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(VibrateController(vibrationDuration, vibrationLowFrequency, vibrationHighFrequency));
    }

    private IEnumerator VibrateController(float duration, float lowFrequency, float highFrequency)
    {
    #if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
            yield return new WaitForSeconds(duration);
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    #else
        // Para la API antigua de Input
        if (SystemInfo.supportsVibration)
        {
            GamePad.SetVibration(0, lowFrequency, highFrequency);
            yield return new WaitForSeconds(duration);
            GamePad.SetVibration(0, 0, 0);
        }
    #endif
        yield return null;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("piso"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("piso"))
            isGrounded = false;
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