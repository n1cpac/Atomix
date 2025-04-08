using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Referencias a los paneles principales
    public GameObject Pantalla_carga_inicial;
    public GameObject Menu_principal;
    public GameObject seleccion_dificultad;
    public GameObject seleccion_polimeros;

    // Botones del menú principal
    public Button btnExperimenta;
    public Button btnPersonaliza;
    public Button btnBye;
    public Button btnTutorial;
    
    // Botones de dificultad
    public Button btnFacil;
    public Button btnMedio;
    public Button btnDificil;
    
    // Botones de selección de polímero
    public Button btnSeleccionarNivel1;
    public Button btnSeleccionarNivel2;
    public Button btnSeleccionarNivel3;
    public Button btnSeleccionarNivel4;
    public Button btnSeleccionarNivel5;
    
    [Header("Configuración")]
    public float tiempoBienvenida = 4f;
    public string[] escenasPolimeros = new string[5];

    void Start()
    {
        // Asegurar que los botones sean interactivos
        btnExperimenta.interactable = true;
        btnPersonaliza.interactable = true;
        btnBye.interactable = true;
        btnTutorial.interactable = true;
        btnFacil.interactable = true;
        btnMedio.interactable = true;
        btnDificil.interactable = true;
        btnSeleccionarNivel1.interactable = true;
        btnSeleccionarNivel2.interactable = true;
        btnSeleccionarNivel3.interactable = true;
        btnSeleccionarNivel4.interactable = true;
        btnSeleccionarNivel5.interactable = true;
        
        // Configuración inicial de visibilidad
        Pantalla_carga_inicial.SetActive(true);
        Menu_principal.SetActive(false);
        seleccion_dificultad.SetActive(false);
        seleccion_polimeros.SetActive(false);
        
        // Configurar listeners de botones
        btnExperimenta.onClick.AddListener(BotonExperimenta);
        btnPersonaliza.onClick.AddListener(BotonPersonaliza);
        btnBye.onClick.AddListener(BotonSalir);
        btnTutorial.onClick.AddListener(BotonTutorial);

        // Listeners para botones de dificultad
        btnFacil.onClick.AddListener(() => SeleccionarDificultad(0));
        btnMedio.onClick.AddListener(() => SeleccionarDificultad(1));
        btnDificil.onClick.AddListener(() => SeleccionarDificultad(2));
        
        // Listeners para selección de polímero
        btnSeleccionarNivel1.onClick.AddListener(() => SeleccionarPolimero(0));
        btnSeleccionarNivel2.onClick.AddListener(() => SeleccionarPolimero(1));
        btnSeleccionarNivel3.onClick.AddListener(() => SeleccionarPolimero(2));
        btnSeleccionarNivel4.onClick.AddListener(() => SeleccionarPolimero(3));
        btnSeleccionarNivel5.onClick.AddListener(() => SeleccionarPolimero(4));

        Invoke("MostrarMenuPrincipal", tiempoBienvenida);
    }

    void MostrarMenuPrincipal()
    {
        Pantalla_carga_inicial.SetActive(false);
        Menu_principal.SetActive(true);
    }

    public void BotonExperimenta()
    {
        Menu_principal.SetActive(false);
        seleccion_dificultad.SetActive(true);
    }

    public void BotonPersonaliza()
    {
        Debug.Log("Personalización activada");
    }

    public void BotonTutorial()
    {
        Debug.Log("Tutorial activado");
    }

    public void BotonSalir()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SeleccionarDificultad(int nivel)
    {
        PlayerPrefs.SetInt("Dificultad", nivel);
        
        if (nivel == 0)
        {
            seleccion_dificultad.SetActive(false);
            seleccion_polimeros.SetActive(true);
        }
    }

    public void SeleccionarPolimero(int indice)
    {
        if (indice >= 0 && indice < escenasPolimeros.Length)
        {
            string nombreEscena = escenasPolimeros[indice];
            if (!string.IsNullOrEmpty(nombreEscena))
            {
                StartCoroutine(ReproducirVideoYEntrar(nombreEscena));
            }
        }
    }

    private IEnumerator ReproducirVideoYEntrar(string nombreEscena)
    {
        // Buscar el VideoFadePlayer en escena
        VideoFadePlayer videoPlayer = FindObjectOfType<VideoFadePlayer>();
        if (videoPlayer != null)
        {
            yield return StartCoroutine(videoPlayer.PlayVideoAndWait());
        }

        SceneManager.LoadScene(nombreEscena);
    }

    public void VolverAPanelAnterior()
    {
        if(seleccion_polimeros.activeSelf)
        {
            seleccion_polimeros.SetActive(false);
            seleccion_dificultad.SetActive(true);
        }
        else if(seleccion_dificultad.activeSelf)
        {
            seleccion_dificultad.SetActive(false);
            Menu_principal.SetActive(true);
        }
  
    }

    public void ReiniciarMenu()
{
    Pantalla_carga_inicial.SetActive(true);
    Menu_principal.SetActive(false);
    seleccion_dificultad.SetActive(false);
    seleccion_polimeros.SetActive(false);
    Invoke("MostrarMenuPrincipal", tiempoBienvenida);
}

}
