using UnityEngine;
using System.Collections.Generic;

public class MoleculeConnector : MonoBehaviour
{
    public bool isCentralConnector;
    public float connectionRange = 0.5f; // Radio reducido
    [SerializeField] private GameObject enlaceVisualPrefab; // Prefab del enlace visual

    private List<GameObject> connectedMolecules = new List<GameObject>();

    void Update()
    {
        if (isCentralConnector)
        {
            DetectMoleculesByDistance();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessConnection(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcessConnection(collision.gameObject);
    }

    void ProcessConnection(GameObject otherObject)
    {
        if (isCentralConnector && otherObject.CompareTag("molecule"))
        {
            ConnectMolecule(otherObject);
        }
        else if (!isCentralConnector && otherObject.CompareTag("connector"))
        {
            MoleculeConnector connectorScript = otherObject.GetComponent<MoleculeConnector>();
            if (connectorScript != null && connectorScript.isCentralConnector)
            {
                connectorScript.ConnectMolecule(gameObject);
            }
        }
    }

    void DetectMoleculesByDistance()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, connectionRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("molecule") && !connectedMolecules.Contains(hitCollider.gameObject))
            {
                ConnectMolecule(hitCollider.gameObject);
            }
        }
    }

    private void ConnectMolecule(GameObject molecule)
    {
        // Evita reconexiones
        if (connectedMolecules.Contains(molecule)) return;

        // Configura la posición y el parentesco
        molecule.transform.SetParent(transform, true);
        molecule.transform.position = GetClosestConnectionPoint(molecule);

        // Fijar física
        Rigidbody rb = molecule.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Colisiones normales
        Collider collider = molecule.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }

        // Agrega a la lista
        connectedMolecules.Add(molecule);

        // Crear el enlace visual entre ambos
        CreateVisualBond(gameObject, molecule);

        Debug.Log("Molécula conectada correctamente");
    }

    private Vector3 GetClosestConnectionPoint(GameObject molecule)
    {
        Vector3 closestPoint = GetComponent<Collider>().ClosestPoint(molecule.transform.position);
        return closestPoint + (transform.position - closestPoint).normalized * 0.2f; // Aumenta separación visual
    }

    private void CreateVisualBond(GameObject from, GameObject to)
    {
        if (enlaceVisualPrefab == null) return;

        GameObject enlace = Instantiate(enlaceVisualPrefab);
        Vector3 start = from.transform.position;
        Vector3 end = to.transform.position;

        enlace.transform.position = (start + end) / 2f;
        enlace.transform.up = (end - start).normalized;
        float distancia = Vector3.Distance(start, end);
        enlace.transform.localScale = new Vector3(0.1f, distancia / 2f + 0.05f, 0.1f); // Más ancho y largo
    }

    void OnDrawGizmosSelected()
    {
        if (isCentralConnector)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, connectionRange);
        }
    }
}
