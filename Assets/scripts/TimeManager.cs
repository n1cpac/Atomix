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

    void Start() {
        ResetearTiempo();
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
            
            // Alternativa sin horas: MM:SS
            // timeText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void Pausar(bool pausar) {
        estaPausado = pausar;
    }

    public void ResetearTiempo() {
        tiempoTranscurrido = iniciarEnCero ? 0f : tiempoInicial;
        ActualizarDisplay();
    }

    // Método para obtener el tiempo actual
    public float ObtenerTiempoActual() {
        return tiempoTranscurrido;
    }
}