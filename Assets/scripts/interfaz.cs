using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    // --- Control de Botones del Menú Principal ---
    public void BotonExperimenta()
    {
        Menu_principal.SetActive(false);
        seleccion_dificultad.SetActive(true);
    }

    public void BotonPersonaliza()
    {
        Debug.Log("Personalización activada");
        // Lógica de personalización aquí
    }

    public void BotonTutorial()
    {
        Debug.Log("Tutorial activado");
        // Cargar escena de tutorial o mostrar panel
    }

    public void BotonSalir()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // --- Navegación entre paneles ---
    public void SeleccionarDificultad(int nivel)
    {
        PlayerPrefs.SetInt("Dificultad", nivel); // 0=fácil, 1=medio, 2=difícil
        
        if (nivel == 0)
        {
            seleccion_dificultad.SetActive(false);
            seleccion_polimeros.SetActive(true);
        }
    }

    public void SeleccionarPolimero(int indice)
    {
        SceneManager.LoadScene(indice);
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
}
