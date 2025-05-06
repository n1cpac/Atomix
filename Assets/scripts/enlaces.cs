using UnityEngine;

public class Conector : MonoBehaviour
{
    public Transform[] puntosConexion; // posiciones alrededor del conector
    public GameObject[] enlacesPrefabs; // prefabs de enlaces correspondientes
    public float distanciaMaxima = 2f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ConectarMoleculas();
        }
    }

    void ConectarMoleculas()
    {
        GameObject[] moleculas = GameObject.FindGameObjectsWithTag("Molecula1"); // puedes hacer un loop para todas
        for (int i = 0; i < moleculas.Length && i < puntosConexion.Length; i++)
        {
            GameObject molecula = moleculas[i];
            float distancia = Vector3.Distance(transform.position, molecula.transform.position);
            if (distancia <= distanciaMaxima)
            {
                // Posicionar molécula
                molecula.transform.position = puntosConexion[i].position;
                molecula.transform.rotation = Quaternion.identity; // o lo que necesites

                // Instanciar y posicionar enlace
                GameObject enlace = Instantiate(enlacesPrefabs[i], transform.position, Quaternion.identity);
                Vector3 direccion = molecula.transform.position - transform.position;
                enlace.transform.position = transform.position + direccion / 2f; // centro del enlace
                enlace.transform.up = direccion.normalized; // orienta el cilindro
                enlace.transform.localScale = new Vector3(0.1f, direccion.magnitude / 2f, 0.1f); // escala eje Y
            }
        }
    }
}