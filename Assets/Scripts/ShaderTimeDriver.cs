using UnityEngine;

// Publica el tiempo SIN escalar como global de shader (_UnscaledTime): los efectos siguen con el juego pausado.
[DefaultExecutionOrder(-100)]
public class ShaderTimeDriver : MonoBehaviour
{
    private static readonly int UnscaledTimeId = Shader.PropertyToID("_UnscaledTime");

    private void OnEnable() => Publish();
    private void Update() => Publish();

    // El módulo evita perder precisión en sesiones largas (salta una vez por hora, imperceptible).
    private static void Publish() =>
        Shader.SetGlobalFloat(UnscaledTimeId, (float)(Time.unscaledTimeAsDouble % 3600.0));
}