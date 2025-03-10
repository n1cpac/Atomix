using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 70f;              // Velocidad del movimiento horizontal
    public float jumpForce = 50f;              // Fuerza del salto
    public float rotationSpeed = 100f;         // Velocidad de rotación (solo eje Y)
    public Transform holdPosition;             // Posición para sujetar la molécula
    public float pickupRange = 10f;            // Rango para agarrar una molécula
    public float dropDistance = 1.5f;          // Distancia al soltar la molécula
    public Vector3 holdOffset = new Vector3(0, 1.0f, 0); // Offset para la molécula

    // Parámetros de vibración
    public float vibrationDuration = 0.2f;
    public float vibrationLowFrequency = 0.5f;
    public float vibrationHighFrequency = 0.5f;

    private Rigidbody rb;
    private GameObject heldMolecule = null;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Evitar que se incline: congelar rotación en X y Z
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
    }

    void Update()
    {
        // Movimiento: usar Input.GetAxis("Vertical") para combinar teclado y mando (W/S o joystick izquierdo)
        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            if (Input.GetKey(KeyCode.W))
                verticalInput = 1f;
            else if (Input.GetKey(KeyCode.S))
                verticalInput = -1f;
        }
        // (El movimiento horizontal se gestiona en FixedUpdate para preservar la gravedad)

        // Rotación del jugador (solo en el suelo) usando Input.GetAxis("Horizontal") o teclas A/D
        float horizontalInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            if (Input.GetKey(KeyCode.A))
                horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D))
                horizontalInput = 1f;
        }
        if (isGrounded)
        {
            transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);
        }

        // Salto: se activa con Space o botón A del mando (JoystickButton0)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Agarrar/Soltar molécula: se activa con F o botón B del mando (JoystickButton1)
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (heldMolecule == null)
                PickUpMolecule();
            else
                DropMolecule();
        }

        // Actualizar la posición de la molécula recogida para que siga al jugador
        if (heldMolecule != null)
        {
            heldMolecule.transform.position = holdPosition.position + holdOffset;
        }
    }

    void FixedUpdate()
    {
        // Movimiento horizontal: se calcula la velocidad manteniendo la componente vertical
        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            if (Input.GetKey(KeyCode.W))
                verticalInput = 1f;
            else if (Input.GetKey(KeyCode.S))
                verticalInput = -1f;
        }
        Vector3 horizontalVelocity = transform.forward * verticalInput * moveSpeed;
        horizontalVelocity.y = rb.linearVelocity.y; // Preservar la velocidad vertical (gravedad)
        rb.linearVelocity = horizontalVelocity;
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
            // Desactivar el collider para que no interfiera con las colisiones del jugador
            Collider moleculeCollider = heldMolecule.GetComponent<Collider>();
            if(moleculeCollider != null)
                moleculeCollider.enabled = false;
            // Opcional: se puede optar por no poner isKinematic o mantenerlo, según el comportamiento deseado
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
        // Reactivar el collider de la molécula al soltarla
        Collider moleculeCollider = heldMolecule.GetComponent<Collider>();
        if(moleculeCollider != null)
            moleculeCollider.enabled = true;
        // Opcional: si se cambió alguna propiedad del Rigidbody, se puede resetear aquí
        heldMolecule = null;
    }
}

    void OnCollisionEnter(Collision collision)
    {
        // Al colisionar, iniciar vibración del mando
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
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("piso"))
        {
            isGrounded = false;
        }
    }
}
