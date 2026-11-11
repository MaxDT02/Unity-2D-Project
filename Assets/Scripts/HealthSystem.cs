using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    public UnityEvent OnDie;
    public UnityEvent<float> OnHurt;

    // Para código: más cómodos que los UnityEvent cuando no se conecta desde el Inspector.
    public event Action Died;
    public event Action<float> Damaged;

    [SerializeField] private float _maxHealth = 1f;
    [SerializeField] private float _health;

    private float _invulnerableUntil;

    public bool IsDead { get; private set; }
    public bool IsInvulnerable => Time.time < _invulnerableUntil;

    private void Awake() => _health = _maxHealth;

    public float GetMaxHealth() => _maxHealth;
    public float GetCurrenthealth() => _health;

    // Ignora los golpes durante estos segundos (tiempo de juego: no avanza con la pausa).
    public void SetInvulnerable(float seconds) => _invulnerableUntil = Mathf.Max(_invulnerableUntil, Time.time + seconds);

    public void Hurt(float damage)
    {
        if (IsDead || IsInvulnerable || damage <= 0f) return;

        _health -= damage;
        OnHurt?.Invoke(_health);
        Damaged?.Invoke(_health);
        if (_health > 0f) return;

        _health = 0f;
        IsDead = true;           // la muerte se notifica una sola vez
        OnDie?.Invoke();
        Died?.Invoke();
    }

    public void Heal(float heal)
    {
        if (IsDead) return;
        _health = Mathf.Min(_maxHealth, _health + heal);
    }
}