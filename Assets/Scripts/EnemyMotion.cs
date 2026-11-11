using UnityEngine;

// Base de los movimientos de enemigo. Cada uno publica hacia dónde va, para que lo use la animación.
public abstract class EnemyMotion : MonoBehaviour
{
    protected Enemy Owner { get; private set; }

    // Dirección en la que se mueve ahora (cero = parado).
    public Vector2 Direction { get; protected set; }

    protected float SpeedScale => Owner != null ? Owner.SpeedScale : 1f;

    protected virtual void Awake() => Owner = GetComponent<Enemy>();

    // Hacia dónde mira al aparecer (lo da el punto de aparición). Por defecto se ignora.
    public virtual void SetStartDirection(Vector2 direction) { }

    // Ajustes por ronda. Lo llama Enemy al empezar.
    public virtual void Configure(EnemyDifficulty difficulty, int round) { }
}