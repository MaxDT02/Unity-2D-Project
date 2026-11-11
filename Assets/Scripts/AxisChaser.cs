using UnityEngine;

// Persigue a Jack en 4 direcciones (nunca en diagonal), despacio y sin atravesar plataformas.
// Si el camino directo está cerrado, rodea la pared en una dirección fija hasta que el camino se abre.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AxisChaser : EnemyMotion
{
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField, Min(0f)] private float speedPixels = 30f;
    [Tooltip("Capas que le cierran el paso (Ground).")]
    [SerializeField] private LayerMask blockMask;

    [Header("Persecución")]
    [Tooltip("Cada cuántos segundos reconsidera hacia dónde ir.")]
    [SerializeField, Min(0.02f)] private float thinkInterval = 0.15f;
    [Tooltip("Cuánto debe superar una distancia a la otra para cambiar de eje (evita vibrar).")]
    [SerializeField, Min(1f)] private float axisSwitchRatio = 1.25f;
    [Tooltip("Por debajo de esta distancia en un eje se considera alineado y usa el otro.")]
    [SerializeField, Min(0f)] private float alignedPixels = 3f;

    [Header("Paredes")]
    [Tooltip("A qué distancia mira si hay plataforma delante. Debe ser mayor que el hueco.")]
    [SerializeField, Min(1f)] private float lookAheadPixels = 4f;
    [Tooltip("Hueco que deja con la pared al frenar.")]
    [SerializeField, Min(1f)] private float gapPixels = 2f;

    private Rigidbody2D _rb;
    private ContactProbe2D _probe;
    private Vector2 _dir;
    private bool _horizontal = true;
    private float _nextThink;

    // Rodeo: dirección que estaba cerrada y dirección en la que se bordea la pared.
    private bool _detouring;
    private Vector2 _blockedDir;
    private Vector2 _detourDir;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        if (blockMask.value == 0)
        {
            Debug.LogWarning("AxisChaser: Block Mask vacío; se usa la capa Ground.", this);
            blockMask = LayerMask.GetMask("Ground");
        }
        _probe = new ContactProbe2D(GetComponent<Collider2D>(), blockMask, gapPixels / pixelsPerUnit);
    }

    private void FixedUpdate()
    {
        if (!JackTarget.TryGetPosition(out Vector2 target))
        {
            Direction = Vector2.zero;
            return;
        }

        float now = Time.time;
        if (now >= _nextThink) Think(target, now);

        if (_dir != Vector2.zero)
        {
            float step = speedPixels / pixelsPerUnit * SpeedScale * Time.fixedDeltaTime;
            float free = _probe.FreeDistance(_dir, step);
            if (free > 0f) _rb.MovePosition(_rb.position + _dir * free);
            if (free < step) _nextThink = 0f;        // chocó: reconsidera en el siguiente paso
        }

        Direction = _dir;
    }

    private void Think(Vector2 target, float now)
    {
        _nextThink = now + thinkInterval;
        Vector2 delta = target - _rb.position;

        if (_detouring && FollowDetour(delta)) return;

        float ax = Mathf.Abs(delta.x);
        float ay = Mathf.Abs(delta.y);
        float aligned = alignedPixels / pixelsPerUnit;

        // Eje preferido, con histéresis para no cambiar de eje con distancias parecidas.
        _horizontal = _horizontal ? !(ay > ax * axisSwitchRatio) : (ax > ay * axisSwitchRatio);
        if ((_horizontal ? ax : ay) < aligned) _horizontal = !_horizontal;   // ya alineado en este eje

        Vector2 primary = Toward(_horizontal, delta, aligned);
        if (!Blocked(primary))
        {
            _dir = primary;
            return;
        }

        // Camino directo cerrado: empieza a bordear la pared por el otro eje.
        Vector2 secondary = Toward(!_horizontal, delta, aligned);
        Vector2 detour = !Blocked(secondary) ? secondary : (!Blocked(-secondary) ? -secondary : Vector2.zero);

        if (detour == Vector2.zero)
        {
            _dir = !Blocked(-primary) ? -primary : Vector2.zero;   // encerrado: retrocede
            return;
        }

        _detouring = true;
        _blockedDir = primary;
        _detourDir = detour;
        _dir = detour;
    }

    // Sigue bordeando hasta que el camino directo se abra. false = el rodeo ya no sirve.
    private bool FollowDetour(Vector2 delta)
    {
        // Jack ya no está en la dirección que estaba cerrada.
        if (Vector2.Dot(_blockedDir, delta) <= 0f)
        {
            _detouring = false;
            return false;
        }

        // El camino directo se abrió (borde de la plataforma): pasa por el hueco.
        if (!Blocked(_blockedDir))
        {
            _detouring = false;
            _dir = _blockedDir;
            return true;
        }

        if (!Blocked(_detourDir))
        {
            _dir = _detourDir;
            return true;
        }

        // Fin de la pared por este lado: da la vuelta y la bordea por el otro.
        _detourDir = -_detourDir;
        if (!Blocked(_detourDir))
        {
            _dir = _detourDir;
            return true;
        }

        _detouring = false;
        return false;
    }

    private static Vector2 Toward(bool horizontal, Vector2 delta, float aligned)
    {
        float value = horizontal ? delta.x : delta.y;
        float sign = Mathf.Abs(value) < aligned ? (Random.value < 0.5f ? -1f : 1f) : Mathf.Sign(value);
        return horizontal ? new Vector2(sign, 0f) : new Vector2(0f, sign);
    }

    private bool Blocked(Vector2 direction) =>
        _probe.IsBlocked(direction, lookAheadPixels / pixelsPerUnit);
}