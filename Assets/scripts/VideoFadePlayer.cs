using UnityEngine;
using UnityEngine.Video;                    // VideoPlayer API :contentReference[oaicite:0]{index=0}
using UnityEngine.UI;                       // RawImage, CanvasGroup
using UnityEngine.SceneManagement;          // SceneManager.LoadScene :contentReference[oaicite:1]{index=1}
using System.Collections;
using System.Collections.Generic;
using System.Linq;                          // FirstOrDefault 

[System.Serializable]
public class LevelVideo
{
    public string levelName;   // Nombre exacto de la escena (Build Settings)
    public VideoClip clip;     // Clip asignado a ese nivel
}

public class VideoFadePlayer : MonoBehaviour
{
    [Header("Componentes")]
    public VideoPlayer videoPlayer;
    public RawImage    rawImage;
    public CanvasGroup canvasGroup;

    [Header("Videos por nivel")]
    public List<LevelVideo> levelVideos;      // Lista editable en Inspector

    [Header("Ajustes de fade")]
    public float fadeDuration = 1f;

    void Start()
    {
        // Estado inicial: vídeo oculto
        canvasGroup.alpha = 0f;
        rawImage.enabled  = false;
    }

    // -----------------------------
    // 1) Compatibilidad antigua:
    // -----------------------------
    // Permite seguir usando StartCoroutine(videoFadePlayer.PlayVideoAndWait());
    // Solo reproduce el vídeo con fade, NO carga escena.
    public IEnumerator PlayVideoAndWait()
    {
        // Lanza internamente la corrutina de fade + reproducción
        yield return StartCoroutine(PlayVideoWithFade());       // :contentReference[oaicite:3]{index=3}
    }

    // ------------------------------------------------
    // 2) Nuevo flujo: reproduce y luego carga escena
    // ------------------------------------------------
    public void PlayAndLoadLevel(string levelToLoad)
    {
        // Busca el clip asignado a ese nivel
        var entry = levelVideos.FirstOrDefault(lv => lv.levelName == levelToLoad);
        if (entry == null || entry.clip == null)
        {
            Debug.LogError($"[VideoFadePlayer] No hay VideoClip para nivel '{levelToLoad}'");
            // Carga escena sin vídeo para no bloquear al usuario
            SceneManager.LoadScene(levelToLoad);                // :contentReference[oaicite:4]{index=4}
            return;
        }

        // Asigna el clip y lanza la corrutina que al final cargará la escena
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip   = entry.clip;                       // :contentReference[oaicite:5]{index=5}
        StartCoroutine(PlayVideoThenLoad(levelToLoad));        // :contentReference[oaicite:6]{index=6}
    }

    private IEnumerator PlayVideoThenLoad(string levelName)
    {
        yield return StartCoroutine(PlayVideoWithFade());       // :contentReference[oaicite:7]{index=7}
        SceneManager.LoadScene(levelName);                      // :contentReference[oaicite:8]{index=8}
    }

    // ------------------------------------------------
    // Corrutina común: fade‑in, play, fade‑out
    // ------------------------------------------------
    private IEnumerator PlayVideoWithFade()
    {
        rawImage.enabled  = true;
        canvasGroup.alpha = 0f;
        videoPlayer.Play();                                     // :contentReference[oaicite:9]{index=9}

        // Fade‑in
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Esperar hasta que termine el vídeo
        while (videoPlayer.isPlaying)
            yield return null;

        // Fade‑out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        videoPlayer.Stop();
        rawImage.enabled = false;
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
