using UnityEngine;
using System.Collections.Generic;

public class MoleculeConnector : MonoBehaviour
{
    public bool isCentralConnector;
    public float connectionRange = 0.5f; // Radio reducido
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
        // Verificación de agarre (si es necesario)
        // if (molecule.GetComponent<Grabbable>() != null && molecule.GetComponent<Grabbable>().IsGrabbed) return;

        // Configuración de posición y parentesco
        molecule.transform.SetParent(transform, true); // Mantiene posición global
        molecule.transform.position = GetClosestConnectionPoint(molecule);
        
        // Configuración física mejorada
        Rigidbody rb = molecule.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Mantenemos física activa
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll; // Congela todas las rotaciones y movimientos
        }

        // Configuración de colisiones
        Collider collider = molecule.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false; // Aseguramos colisiones normales
        }

        connectedMolecules.Add(molecule);
        Debug.Log("Molécula conectada correctamente");
    }

    private Vector3 GetClosestConnectionPoint(GameObject molecule)
    {
        // Calcula punto más cercano en el collider del conector
        Vector3 closestPoint = GetComponent<Collider>().ClosestPoint(molecule.transform.position);
        
        // Ajuste final de posición con offset pequeño
        return closestPoint + (transform.position - closestPoint).normalized * 0.1f;
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