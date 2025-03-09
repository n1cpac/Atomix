using UnityEngine;

public class SphereSelector : MonoBehaviour
{
    public GameObject selectionIndicatorPrefab;
    private GameObject currentIndicator;
    private GameObject selectedSphere;
    private Rigidbody selectedRigidbody;
    public float moveSpeed = 5f;
    private bool isMoving = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectSphere();
        }

        if (selectedSphere != null && !isMoving)
        {
            HandleWASDInput();
        }
    }

    void TrySelectSphere()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Sphere"))
        {
            SelectSphere(hit.collider.gameObject);
        }
    }

    void SelectSphere(GameObject sphere)
    {
        if (selectedSphere != null)
        {
            Destroy(currentIndicator);
            Destroy(selectedSphere.GetComponent<CollisionNotifier>());
        }

        selectedSphere = sphere;
        selectedRigidbody = selectedSphere.GetComponent<Rigidbody>();

        if (selectedRigidbody == null)
        {
            selectedRigidbody = selectedSphere.AddComponent<Rigidbody>();
        }

        selectedRigidbody.useGravity = false;
        selectedRigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Añadir detector de colisiones
        CollisionNotifier notifier = selectedSphere.AddComponent<CollisionNotifier>();
        notifier.OnCollision += HandleCollision;

        ShowSelectionIndicator();
    }

    void ShowSelectionIndicator()
    {
        currentIndicator = Instantiate(
            selectionIndicatorPrefab,
            selectedSphere.transform.position + Vector3.up * 1.2f,
            Quaternion.identity
        );
        currentIndicator.transform.SetParent(selectedSphere.transform);
    }

void HandleCollision(Collision collision)
{
    if (isMoving)
    {
        Vector3 collisionNormal = collision.contacts[0].normal;

        // Si la normal es más horizontal (pared)
        if (Mathf.Abs(collisionNormal.y) < 0.5f) 
        {
            selectedRigidbody.linearVelocity = Vector3.zero;
            isMoving = false;  // Detiene el movimiento en el momento de la colisión
        }
    }
}

   void HandleWASDInput()
{
    Vector3 direction = Vector3.zero;
    Transform cameraTransform = Camera.main.transform;

    if (Input.GetKey(KeyCode.W)) direction += cameraTransform.forward;
    if (Input.GetKey(KeyCode.S)) direction += -cameraTransform.forward;
    if (Input.GetKey(KeyCode.A)) direction += -cameraTransform.right;
    if (Input.GetKey(KeyCode.D)) direction += cameraTransform.right;

    direction.y = 0; // Anular movimiento vertical

    if (direction != Vector3.zero)
    {
        selectedRigidbody.linearVelocity = direction.normalized * moveSpeed;
        isMoving = true; // Aquí lo habilitamos otra vez
    }
}   

    void OnDestroy()
    {
        if (selectedSphere != null)
        {
            var notifier = selectedSphere.GetComponent<CollisionNotifier>();
            if (notifier != null) notifier.OnCollision -= HandleCollision;
        }
    }
}

// Script auxiliar para detectar colisiones
public class CollisionNotifier : MonoBehaviour
{
    public System.Action<Collision> OnCollision;

    void OnCollisionEnter(Collision collision)
    {
        OnCollision?.Invoke(collision);
    }
}
