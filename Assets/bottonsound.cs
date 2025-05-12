using UnityEngine;
using UnityEngine.SceneManagement;

public class bottonsound : MonoBehaviour
{
    public AudioSource sonidoBoton;
    public string nombreEscena; // Nombre de la escena a cargar

    public void OnButtonClick()
    {
        if(sonidoBoton != null)
        {
            sonidoBoton.Play();
            // Cargar escena después que termine el sonido
            Invoke("LoadScene", sonidoBoton.clip.length);
        }
        else
        {
            LoadScene();
        }
    }

    void LoadScene()
    {
        SceneManager.LoadScene(nombreEscena);
    }
}