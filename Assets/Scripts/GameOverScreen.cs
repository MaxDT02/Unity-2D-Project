using System.Collections;
using UnityEngine;

// Pantalla de Game Over: caen las letras, destello, pausa (saltable) y cierre en iris hacia la siguiente pantalla.
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private LetterDrop letters;
    [SerializeField] private ScreenShake shake;
    [SerializeField] private ScreenFlash flash;
    [SerializeField] private IrisTransition iris;

    [Header("Tiempos (segundos)")]
    [SerializeField, Min(0f)] private float startDelay = 0.6f;
    [SerializeField, Min(0.05f)] private float flashSeconds = 0.25f;
    [SerializeField, Min(0f)] private float holdSeconds = 2.5f;
    [SerializeField, Min(0.1f)] private float irisSeconds = 0.9f;

    [Header("Sacudida al caer cada letra")]
    [SerializeField, Min(0.05f)] private float shakeSeconds = 0.15f;
    [SerializeField, Min(1)] private int shakeAmplitude = 2;

    [Tooltip("Centro del iris al cerrar (0,5 / 0,5 = centro de pantalla).")]
    [SerializeField] private Vector2 irisCenter = new Vector2(0.5f, 0.5f);

    private void OnEnable() => letters.Landed += OnLetterLanded;
    private void OnDisable() => letters.Landed -= OnLetterLanded;

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(startDelay);
        yield return letters.Play();
        yield return flash.Play(flashSeconds);
        yield return Hold(holdSeconds);
        yield return iris.Close(irisSeconds, irisCenter);
        ScreenFlow.AfterGameOver();
    }

    private void OnLetterLanded(int index) => shake.Shake(shakeSeconds, shakeAmplitude);

    // Espera, o hasta que se pulse algo (con un margen para no saltar por la pulsación anterior).
    private static IEnumerator Hold(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed > 0.4f && ScreenTimer.AnyInput()) yield break;
            yield return null;
        }
    }
}