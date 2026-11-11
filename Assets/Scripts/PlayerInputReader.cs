using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private float deadZone = 0.5f;

    public float Horizontal { get; private set; }  // -1, 0, 1
    public int Vertical { get; private set; }      // -1, 0, 1
    public bool JumpHeld => jump.action.IsPressed();

    public event Action JumpPressed;
    public event Action UpPressed;                 // solo en el flanco: al empezar a inclinar arriba

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        jump.action.started += OnJump;
    }

    private void OnDisable()
    {
        jump.action.started -= OnJump;
        move.action.Disable();
        jump.action.Disable();
    }

    private void OnJump(InputAction.CallbackContext _) => JumpPressed?.Invoke();

    private void Update()
    {
        Vector2 v = move.action.ReadValue<Vector2>();
        Horizontal = Mathf.Abs(v.x) > deadZone ? Mathf.Sign(v.x) : 0f;

        int previous = Vertical;
        Vertical = Mathf.Abs(v.y) > deadZone ? (int)Mathf.Sign(v.y) : 0;
        if (Vertical > 0 && previous <= 0) UpPressed?.Invoke();
    }
}