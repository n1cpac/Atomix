using UnityEngine;

public class Molecula : MonoBehaviour
{
    private bool esSostenida = false;
    private Transform manoDelJugador;

    private void Update()
    {
        if (esSostenida && Input.GetKeyDown(KeyCode.F))
        {
            Soltar();
        }
    }

    public void Recoger(Transform mano)
    {
        esSostenida = true;
        manoDelJugador = mano;
        transform.SetParent(manoDelJugador);
        transform.localPosition = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void Soltar()
    {
        esSostenida = false;
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
