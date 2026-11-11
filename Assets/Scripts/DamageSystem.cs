using UnityEngine;

// Daña a todo lo que tenga HealthSystem y esté tocando este trigger.
// Se desactiva el componente para que deje de dañar (Unity no manda eventos de física a scripts desactivados).
public class DamageSystem : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled) return;
        TryHurt(other);
    }

    // Stay: si el objetivo era invulnerable, recibe el golpe en cuanto termina la invulnerabilidad.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActiveAndEnabled) return;
        TryHurt(other);
    }

    private void TryHurt(Collider2D other)
    {
        if (other.TryGetComponent(out HealthSystem health)) health.Hurt(damage);
    }
}