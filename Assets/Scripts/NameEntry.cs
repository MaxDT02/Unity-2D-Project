using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Escritura del nombre. Teclado: escribir, Backspace borra, Enter confirma.
// Mando o flechas: arriba y abajo cambian la letra actual, derecha fija la letra y añade otra,
// izquierda borra, y el botón de aceptar confirma.
public class NameEntry : MonoBehaviour
{
    [SerializeField] private NameFrame frame;
    [SerializeField, Min(1)] private int maxLength = HighScoreTable.MaxNameLength;
    [Tooltip("Caracteres permitidos, en el orden en que se recorren con arriba y abajo.")]
    [SerializeField] private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.- ";
    [Tooltip("Ignora las pulsaciones al empezar (por la pulsación que acaba de traer a esta pantalla).")]
    [SerializeField, Min(0f)] private float ignoreInputSeconds = 0.4f;

    public event Action<string> Confirmed;

    private readonly StringBuilder _text = new();
    private TMP_Text _label;
    private UiInputMap _input;
    private Keyboard _keyboard;
    private float _startTime;
    private bool _active;

    public void Begin(TMP_Text label)
    {
        _label = label;
        _text.Clear();
        _active = true;
        _startTime = Time.unscaledTime;

        _input ??= new UiInputMap();
        _input.Enable();

        _keyboard = Keyboard.current;
        if (_keyboard != null) _keyboard.onTextInput += OnTextInput;

        if (frame != null) frame.AttachTo(label.rectTransform);
        Refresh();
    }

    public void End()
    {
        _active = false;
        if (_keyboard != null) _keyboard.onTextInput -= OnTextInput;
        _keyboard = null;
        _input?.Disable();
        if (frame != null) frame.Hide();
    }

    private void OnDisable() => End();

    private void OnDestroy()
    {
        _input?.Dispose();
        _input = null;
    }

    private void Update()
    {
        if (!_active || Time.unscaledTime - _startTime < ignoreInputSeconds) return;

        if (_input.Up.WasPressedThisFrame()) Cycle(1);
        if (_input.Down.WasPressedThisFrame()) Cycle(-1);
        if (_input.Right.WasPressedThisFrame()) Append(alphabet[0]);
        if (_input.Left.WasPressedThisFrame()) Backspace();
        if (_input.Submit.WasPressedThisFrame()) Confirm();
    }

    private void OnTextInput(char c)
    {
        if (!_active || Time.unscaledTime - _startTime < ignoreInputSeconds) return;

        if (c == '\b')
        {
            Backspace();
            return;
        }

        char upper = char.ToUpperInvariant(c);
        if (c >= ' ' && alphabet.IndexOf(upper) >= 0) Append(upper);
    }

    private void Append(char c)
    {
        if (_text.Length >= maxLength) return;
        _text.Append(c);
        Refresh();
    }

    private void Backspace()
    {
        if (_text.Length == 0) return;
        _text.Length--;
        Refresh();
    }

    // Cambia la última letra por la siguiente o anterior del alfabeto (o añade la primera si aún no hay).
    private void Cycle(int delta)
    {
        if (_text.Length == 0)
        {
            Append(alphabet[0]);
            return;
        }

        int index = alphabet.IndexOf(_text[_text.Length - 1]);
        index = index < 0 ? 0 : ((index + delta) % alphabet.Length + alphabet.Length) % alphabet.Length;
        _text[_text.Length - 1] = alphabet[index];
        Refresh();
    }

    private void Confirm()
    {
        if (_text.Length == 0) return;
        string name = HighScoreTable.Clean(_text.ToString());
        End();
        Confirmed?.Invoke(name);
    }

    // El marco deja siempre un hueco libre para la siguiente letra.
    private void Refresh()
    {
        _label.text = _text.ToString();
        if (frame != null) frame.Fit(Mathf.Min(_text.Length + 1, maxLength));
    }
}