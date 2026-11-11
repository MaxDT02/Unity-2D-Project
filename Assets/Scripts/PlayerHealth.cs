using UnityEngine;

// Puente entre la vida de Jack y el flujo de la partida.
[RequireComponent(typeof(HealthSystem))]
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Segundos sin recibir daño tras aparecer.")]
    [SerializeField, Min(0f)] private float spawnInvulnerability = 2f;

    private HealthSystem _health;

    private void Awake() => _health = GetComponent<HealthSystem>();

    private void OnEnable()
    {
        _health.Died += OnDied;
        _health.SetInvulnerable(spawnInvulnerability);
    }

    private void OnDisable() => _health.Died -= OnDied;

    private void OnDied() => PlayerLifecycle.ReportDeath();
}