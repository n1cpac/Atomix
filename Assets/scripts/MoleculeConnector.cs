using UnityEngine;
using System.Collections.Generic;

public class MoleculeConnector : MonoBehaviour
{
    [Header("Connection Settings")]
    public bool isCentralConnector;
    public float connectionRange = 0.5f;
    [SerializeField] private GameObject enlaceVisualPrefab;
    [SerializeField] private float enlaceOffset = 0.5f; // Distancia entre conector y molécula

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip connectionSound;
    [Range(0.1f, 1f)] public float soundVolume = 0.7f;
    public bool randomizePitch = true;
    [Range(0.1f, 0.3f)] public float pitchVariation = 0.15f;

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
        if (isCentralConnector && otherObject.CompareTag("molecule") && !connectedMolecules.Contains(otherObject))
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
        if (connectedMolecules.Contains(molecule)) return;

        // Reproducir sonido
        PlayConnectionSound();

        // Calcular posición final con espacio entre los objetos
        Vector3 direction = (molecule.transform.position - transform.position).normalized;
        Vector3 finalPosition = transform.position + direction * enlaceOffset;

        molecule.transform.position = finalPosition;

        // Congelar física para que no se muevan
        Rigidbody rb = molecule.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        Collider collider = molecule.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }

        connectedMolecules.Add(molecule);

        CreateVisualBond(transform.position, finalPosition);

        Debug.Log("Molécula conectada: " + molecule.name);
    }

    private void PlayConnectionSound()
    {
        if (audioSource == null || connectionSound == null)
        {
            Debug.LogWarning("Componente de audio no configurado");
            return;
        }

        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(connectionSound, soundVolume);
    }

    private void CreateVisualBond(Vector3 start, Vector3 end)
    {
        if (enlaceVisualPrefab == null) return;

        GameObject enlace = Instantiate(enlaceVisualPrefab);
        enlace.transform.position = (start + end) / 2f;
        enlace.transform.up = (end - start).normalized;

        float distancia = Vector3.Distance(start, end);
        enlace.transform.localScale = new Vector3(0.1f, distancia / 2f, 0.1f);
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
