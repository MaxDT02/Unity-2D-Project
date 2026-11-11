using System.Collections;
using UnityEngine;

// Sacude un RectTransform en saltos de píxeles enteros y lo devuelve a su sitio.
public class ScreenShake : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [Tooltip("Segundos entre un salto y el siguiente.")]
    [SerializeField, Min(0.01f)] private float stepSeconds = 0.04f;

    private Vector2 _home;
    private Coroutine _routine;

    private void Awake() => _home = target.anchoredPosition;

    public void Shake(float seconds, int amplitude)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Run(seconds, amplitude));
    }

    private IEnumerator Run(float seconds, int amplitude)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end)
        {
            int a = Mathf.CeilToInt(amplitude * (end - Time.unscaledTime) / seconds);   // se va apagando
            target.anchoredPosition = _home + new Vector2(Random.Range(-a, a + 1), Random.Range(-a, a + 1));
            yield return new WaitForSecondsRealtime(stepSeconds);
        }
        target.anchoredPosition = _home;
        _routine = null;
    }
}