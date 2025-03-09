using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float jumpForce = 50f;
    public float rotationSpeed = 100f; // Velocidad de rotación con A y D
    public Transform holdPosition; // Posición donde se sujetará la molécula
    public float pickupRange = 10f; // Distancia para agarrar una molécula
    public float dropDistance = 1.5f; // Distancia donde se soltará la molécula
    public Vector3 holdOffset = new Vector3(0, 0.9f, 0); // Offset para que la molécula se vea encima

    private Rigidbody rb;
    private GameObject heldMolecule = null; // Referencia a la molécula que se sostiene
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Rotación con A y D (Input horizontal)
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

        // Movimiento hacia adelante y atrás con W y S (Input vertical)
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * vertical * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        // Salto con la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Agarrar/Soltar molécula con la tecla "F"
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (heldMolecule == null)
                PickUpMolecule();
            else
                DropMolecule();
        }

        // Actualizar la posición de la molécula si se tiene una
        if (heldMolecule != null)
        {
            heldMolecule.transform.position = holdPosition.position + holdOffset;
        }
    }

    private void PickUpMolecule()
    {
        // Buscar la molécula más cercana dentro del rango
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("molecule"))
            {
                heldMolecule = col.gameObject;
                heldMolecule.transform.SetParent(holdPosition);
                heldMolecule.GetComponent<Rigidbody>().isKinematic = true; // Desactivar físicas mientras se sostiene
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
            heldMolecule.GetComponent<Rigidbody>().isKinematic = false; // Reactivar físicas
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
