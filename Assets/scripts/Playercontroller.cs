using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 300f;
    public float jumpForce = 120f;
    public float rotationSpeed = 100f;
    public Transform holdPosition;
    public float pickupRange = 10f;
    public float dropDistance = 1.5f;
    public Vector3 holdOffset = new Vector3(0, 1.0f, 0);

    private Rigidbody rb;
    private GameObject heldMolecule = null;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- Movimiento del jugador ---
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * vertical * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        // --- Rotación del jugador (solo si está en el suelo) ---
        if (isGrounded)
        {
            float horizontal = Input.GetAxis("Horizontal");
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
        }

        // --- Salto ---
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // --- Agarrar/Soltar Molécula ---
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (heldMolecule == null)
                PickUpMolecule();
            else
                DropMolecule();
        }

        // --- Actualizar la posición de la molécula recogida ---
        if (heldMolecule != null)
        {
            heldMolecule.transform.position = holdPosition.position + holdOffset;
        }
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
                heldMolecule.GetComponent<Rigidbody>().isKinematic = true;
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
            heldMolecule.GetComponent<Rigidbody>().isKinematic = false;
            heldMolecule = null;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("piso"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("piso"))
        {
            isGrounded = false;
        }
    }
}
