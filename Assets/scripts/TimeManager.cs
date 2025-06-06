using UnityEngine;
using TMPro;

public class Cronometro : MonoBehaviour {
    [Header("Configuración")]
    public TMP_Text timeText; // Referencia al texto UI (TextMeshPro)
    
    [Header("Tiempo Inicial")]
    public bool iniciarEnCero = true;
    public float tiempoInicial = 0f; // Tiempo inicial en segundos

    private float tiempoTranscurrido;
    private bool estaPausado = false;

    public GameObject Overlaypausa;
    public GameObject segurodesalir;

    void Start() {
        ResetearTiempo();
        Overlaypausa.SetActive(false); // Asegúrate de que el overlay de pausa esté oculto al inicio
        segurodesalir.SetActive(false); // Asegúrate de que el mensaje de confirmación esté oculto al inicio
    }

    void Update() {
        if (!estaPausado) {
            tiempoTranscurrido += Time.deltaTime;
            ActualizarDisplay();
        }
    }

    void ActualizarDisplay() {
        if (timeText != null) {
            int horas = Mathf.FloorToInt(tiempoTranscurrido / 3600);
            int minutos = Mathf.FloorToInt((tiempoTranscurrido % 3600) / 60);
            int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60);
            
            // Formato HH:MM:SS
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", horas, minutos, segundos);
        }
    }

    public void ResetearTiempo() {
        tiempoTranscurrido = iniciarEnCero ? 0f : tiempoInicial;
        ActualizarDisplay();
    }

    // Método para obtener el tiempo actual
    public float ObtenerTiempoActual() {
        return tiempoTranscurrido;
    }

    public void PausarJuego() {
        Time.timeScale = 0f; // Pausa el juego
        estaPausado = true;
        Overlaypausa.SetActive(true); // Muestra el overlay de pausa
    }

    public void ReanudarJuego() {
        Time.timeScale = 1f; // Reanuda el juego
        estaPausado = false;
        Overlaypausa.SetActive(false); // Oculta el overlay de pausa
        segurodesalir.SetActive(false); // Oculta el mensaje de confirmación
    }

    public void MostrarSeguroSalir() {
        Overlaypausa.SetActive(false); // Oculta el overlay de pausa
        segurodesalir.SetActive(true); // Muestra el mensaje de confirmación
    }

    public void iralmenuprincipal() {
        Time.timeScale = 1f; // Asegúrate de reanudar el tiempo al volver al menú
        estaPausado = false;

        // Si tenés objetos con DontDestroyOnLoad y querés eliminarlos, lo podés hacer acá.
        // Por ahora no se necesita.

        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuInicial"); // Cambia a la escena del menú principal
    }
}
