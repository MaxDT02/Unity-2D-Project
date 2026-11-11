using System;
using UnityEngine.InputSystem;

// Controles de menús y pantallas con el Input System (teclado y mando), sin depender de un asset.
// El teclado solo usa las flechas, así las letras quedan libres para escribir el nombre.
public sealed class UiInputMap : IDisposable
{
    public InputAction Up { get; }
    public InputAction Down { get; }
    public InputAction Left { get; }
    public InputAction Right { get; }
    public InputAction Submit { get; }
    public InputAction Cancel { get; }

    private readonly InputAction[] _all;

    public UiInputMap()
    {
        Up = Create("Up", "<Keyboard>/upArrow", "<Gamepad>/dpad/up", "<Gamepad>/leftStick/up");
        Down = Create("Down", "<Keyboard>/downArrow", "<Gamepad>/dpad/down", "<Gamepad>/leftStick/down");
        Left = Create("Left", "<Keyboard>/leftArrow", "<Gamepad>/dpad/left", "<Gamepad>/leftStick/left");
        Right = Create("Right", "<Keyboard>/rightArrow", "<Gamepad>/dpad/right", "<Gamepad>/leftStick/right");
        Submit = Create("Submit", "<Keyboard>/enter", "<Keyboard>/numpadEnter", "<Gamepad>/buttonSouth", "<Gamepad>/start");
        Cancel = Create("Cancel", "<Keyboard>/escape", "<Gamepad>/buttonEast");
        _all = new[] { Up, Down, Left, Right, Submit, Cancel };
    }

    public void Enable()
    {
        foreach (InputAction action in _all) action.Enable();
    }

    public void Disable()
    {
        foreach (InputAction action in _all) action.Disable();
    }

    public void Dispose()
    {
        foreach (InputAction action in _all) action.Dispose();
    }

    private static InputAction Create(string name, params string[] paths)
    {
        InputAction action = new(name, InputActionType.Button);
        foreach (string path in paths) action.AddBinding(path);
        return action;
    }
}