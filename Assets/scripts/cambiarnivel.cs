using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   

public class cambiarnivel : MonoBehaviour
{
    public void CambiarEscena(string nombreEscena)
    {
        // Cargar la escena especificada
        SceneManager.LoadScene(nombreEscena);
    }
}
