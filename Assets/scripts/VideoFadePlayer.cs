using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VideoFadePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public CanvasGroup canvasGroup;

    [Header("Video Settings")]
    public VideoClip videoClip; // Asignar desde el Inspector
    public float fadeDuration = 1f;

    void Start()
    {
        // Asegurarse de que el video esté configurado al inicio
        canvasGroup.alpha = 0f;
        rawImage.enabled = false;

        if (videoPlayer != null && videoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
        }
    }

    public IEnumerator PlayVideoAndWait()
    {
        yield return StartCoroutine(PlayVideoWithFade());
    }

    private IEnumerator PlayVideoWithFade()
    {
        rawImage.enabled = true;
        canvasGroup.alpha = 0f;

        videoPlayer.Play();

        // Fade in
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Esperar hasta que el video termine
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // Fade out
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
