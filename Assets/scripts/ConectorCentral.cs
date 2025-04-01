using UnityEngine;

public class ConectorCentral : MonoBehaviour
{
    public int totalMoleculasNecesarias = 4;
    private int moleculasConectadas = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Molecula"))
        {
            ConectarMolecula(other.gameObject);
        }
    }

    private void ConectarMolecula(GameObject molecula)
    {
        // Desactiva la física de la molécula para fijarla al conector
        Rigidbody rb = molecula.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Fija la molécula como hija del conector central
        molecula.transform.SetParent(transform);
        molecula.transform.localPosition = Vector3.zero; // Ajusta según la posición deseada

        moleculasConectadas++;

        if (moleculasConectadas >= totalMoleculasNecesarias)
        {
            NivelCompletado();
        }
    }

    private void NivelCompletado()
    {
        Debug.Log("¡Nivel completado! Todas las moléculas están conectadas.");
        // Implementa aquí la lógica para finalizar el nivel
    }
}
