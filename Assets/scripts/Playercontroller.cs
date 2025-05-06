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

    // Vibración
    public float vibrationDuration = 0.2f;
    public float vibrationLowFrequency = 0.5f;
    public float vibrationHighFrequency = 0.5f;

    // Interno
    private Rigidbody rb;
    private Vector3 inputDirection;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        if (pasos != null)
        {
            pasos.spatialBlend = 0f;
            pasos.loop = true;
            pasos.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isTopDownActive = !isTopDownActive;
            if (!isTopDownActive)
                DeactivateTopDownView();
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

        if (isGrounded)
            transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && isGrounded)
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
        if (Input.GetButtonDown("Horizontal")) { Hactivo = true; if (!pasos.isPlaying) pasos.Play(); }
        if (Input.GetButtonDown("Vertical")) { Vactivo = true; if (!pasos.isPlaying) pasos.Play(); }
        if (Input.GetButtonUp("Horizontal")) { Hactivo = false; if (!Vactivo) pasos.Pause(); }
        if (Input.GetButtonUp("Vertical")) { Vactivo = false; if (!Hactivo) pasos.Pause(); }
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
