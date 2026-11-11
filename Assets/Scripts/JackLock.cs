using System.Collections.Generic;
using UnityEngine;

// Quita el control a Jack sin destruirlo (fin de nivel, muerte...).
[RequireComponent(typeof(Rigidbody2D))]
public class JackLock : MonoBehaviour
{
    [SerializeField] private Behaviour[] controls;

    private Rigidbody2D _rb;

    public bool IsLocked { get; private set; }

    private void Reset()
    {
        List<Behaviour> list = new();
        AddIfPresent<Jack>(list);
        AddIfPresent<PlayerInputReader>(list);
        AddIfPresent<MovementSystem>(list);
        AddIfPresent<FlightSystem>(list);
        AddIfPresent<JackActions>(list);
        controls = list.ToArray();
    }

    private void AddIfPresent<T>(List<Behaviour> list) where T : Behaviour
    {
        if (TryGetComponent(out T component)) list.Add(component);
    }

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void Lock()
    {
        if (IsLocked) return;
        IsLocked = true;
        foreach (Behaviour control in controls)
            if (control != null) control.enabled = false;
        _rb.linearVelocity = Vector2.zero;
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;
        foreach (Behaviour control in controls)
            if (control != null) control.enabled = true;
    }
}