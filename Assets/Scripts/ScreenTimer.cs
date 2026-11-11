using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Espera unos segundos (o cualquier pulsación) y avisa. Sirve para pantallas que se pasan solas.
// Usa tiempo sin escalar, así funciona aunque el juego haya quedado pausado.
public class ScreenTimer : MonoBehaviour
{
    [Tooltip("Segundos hasta pasar solo. 0 = solo pasa al pulsar.")]
    [SerializeField, Min(0f)] private float seconds = 10f;
    [SerializeField] private bool skipOnInput = true;
    [Tooltip("Al empezar ignora las pulsaciones durante este tiempo (evita saltarse la pantalla con la pulsación anterior).")]
    [SerializeField, Min(0f)] private float ignoreInputSeconds = 0.5f;
    [SerializeField] private UnityEvent finished;
    [Tooltip("Se lanza al pulsar una tecla. Si está vacío, se lanza 'finished' como antes.")]
    [SerializeField] private UnityEvent skipped;

    private float _elapsed;
    private bool _running;

    private void OnEnable() => Restart();
    private void OnDisable() => _running = false;

    public void Restart()
    {
        _elapsed = 0f;
        _running = true;
    }

    private void Update()
    {
        if (!_running) return;
        _elapsed += Time.unscaledDeltaTime;

        bool skip = skipOnInput && _elapsed >= ignoreInputSeconds && AnyInput();
        bool timeUp = seconds > 0f && _elapsed >= seconds;
        if (!skip && !timeUp) return;

        _running = false;
        if (skip && skipped.GetPersistentEventCount() > 0) skipped.Invoke();
        else finished.Invoke();
    }

    public static bool AnyInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) return true;

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

        foreach (Gamepad pad in Gamepad.all)
            if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)
                return true;
        return false;
    }
}