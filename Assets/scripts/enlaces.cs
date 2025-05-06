using UnityEngine;

public class EnlaceAutoConector : MonoBehaviour
{
    [Header("Configuración de detección")]
    public string tagDeLaMolecula = "molecule";
    public float distanciaMaxima = 2.0f;

    void Start()
    {
        ConectarAlObjetoConTag();
    }

    void ConectarAlObjetoConTag()
    {
        GameObject[] moleculas = GameObject.FindGameObjectsWithTag(tagDeLaMolecula);

        GameObject objetivoMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (GameObject molecula in moleculas)
        {
            float distancia = Vector3.Distance(transform.position, molecula.transform.position);
            if (distancia < distanciaMinima && distancia <= distanciaMaxima)
            {
                distanciaMinima = distancia;
                objetivoMasCercano = molecula;
            }
        }

        if (objetivoMasCercano != null)
        {
            transform.LookAt(objetivoMasCercano.transform);

            float distancia = Vector3.Distance(transform.position, objetivoMasCercano.transform.position);
            transform.localScale = new Vector3(0.1f, distancia / 2f, 0.1f);
            transform.position = (transform.position + objetivoMasCercano.transform.position) / 2f;
        }
        else
        {
            Debug.LogWarning("No se encontró ninguna molécula cercana con el tag: " + tagDeLaMolecula);
        }
    }
}