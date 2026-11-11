using UnityEngine;

// Caminante de plataformas (momia): camina, da media vuelta en los bordes, cae en seco y repite.
// Al caer en el suelo del marco se transforma, con chispa, en otro enemigo.
// Su "suelo" es el punto de los pies: la parte baja del collider debe coincidir con los pies del sprite.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlatformWalker : EnemyMotion
{
    private enum Phase { Pausing, Walking, Falling }
    public enum LandingDirection { Keep, Random, TowardJack }

    [SerializeField] private float pixelsPerUnit = 100f;
    [Tooltip("Capas que le cierran el paso y sirven de suelo (Ground).")]
    [SerializeField] private LayerMask groundMask;

    [Header("Velocidad (px/s)")]
    [SerializeField, Min(0f)] private float walkSpeedPixels = 40f;
    [SerializeField, Min(0f)] private float fallSpeedPixels = 120f;

    [Header("Recorrido")]
    [Tooltip("Medias vueltas que da en los bordes antes de caer por el siguiente. Lo sustituye EnemyDifficulty.")]
    [SerializeField, Min(0)] private int edgeTurns = 1;
    [Tooltip("Hacia dónde camina al caer en otra plataforma.")]
    [SerializeField] private LandingDirection landingDirection = LandingDirection.Random;
    [Tooltip("Parada inicial tras aparecer (se ve el primer sprite).")]
    [SerializeField, Min(0f)] private float spawnPause = 0.3f;

    [Header("Detección")]
    [Tooltip("Cuánto mira por delante del centro para decidir si hay suelo. 0 = cae al pasar el centro; positivo cae antes; negativo, después.")]
    [SerializeField] private float edgeLookPixels = 0f;
    [Tooltip("Hasta dónde busca suelo bajo los pies.")]
    [SerializeField, Min(1f)] private float groundProbePixels = 4f;
    [SerializeField, Min(0f)] private float wallGapPixels = 1f;
    [Tooltip("Recorte inferior de la caja que detecta paredes (para no tomar el suelo por pared).")]
    [SerializeField, Min(0f)] private float feetInsetPixels = 3f;
    [Tooltip("Si aterriza a menos de esta altura sobre la línea del suelo, se considera suelo del marco.")]
    [SerializeField, Min(0f)] private float floorTolerancePixels = 4f;

    [Header("Transformación")]
    [Tooltip("En qué se convierte al caer en el suelo, con pesos. Vacío = solo desaparece.")]
    [SerializeField] private WeightedEnemy[] transformInto;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private ContactProbe2D _wallProbe;
    private Phase _phase = Phase.Pausing;
    private float _phaseUntil;
    private float _dirX = 1f;
    private int _turnsLeft;

    private float Pixel => 1f / pixelsPerUnit;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _col = GetComponent<Collider2D>();
        _wallProbe = new ContactProbe2D(_col, groundMask, wallGapPixels * Pixel, feetInsetPixels * Pixel);
    }

    private void Start() => _phaseUntil = Time.time + spawnPause;

    public override void SetStartDirection(Vector2 direction)
    {
        if (direction.x != 0f) _dirX = Mathf.Sign(direction.x);
    }

    public override void Configure(EnemyDifficulty difficulty, int round) =>
        edgeTurns = difficulty.MummyEdgeTurns(round);

    private void FixedUpdate()
    {
        switch (_phase)
        {
            case Phase.Pausing:
                Direction = Vector2.zero;
                if (Time.time >= _phaseUntil) BeginWalking();
                break;
            case Phase.Walking:
                Walk();
                break;
            case Phase.Falling:
                Fall();
                break;
        }
    }

    private void BeginWalking()
    {
        _turnsLeft = edgeTurns;
        _phase = Phase.Walking;
    }

    private void Walk()
    {
        float step = walkSpeedPixels * Pixel * SpeedScale * Time.fixedDeltaTime;
        Vector2 dir = new(_dirX, 0f);
        Direction = dir;

        // Pared delante: media vuelta.
        if (_wallProbe.FreeDistance(dir, step) < step)
        {
            TurnAround();
            return;
        }

        // Sin suelo delante: media vuelta si quedan, si no, cae.
        if (!GroundAt(FeetCenter().x + _dirX * (step + edgeLookPixels * Pixel)))
        {
            if (_turnsLeft > 0) TurnAround();
            else StartFalling(dir * step);
            return;
        }

        _rb.MovePosition(_rb.position + dir * step);
    }

    private void TurnAround()
    {
        _dirX = -_dirX;
        if (_turnsLeft > 0) _turnsLeft--;
    }

    // Avanza el último paso para que el centro quede fuera de la plataforma y la caída sea limpia.
    private void StartFalling(Vector2 lastStep)
    {
        _rb.MovePosition(_rb.position + lastStep);
        _phase = Phase.Falling;
        Direction = Vector2.down;
    }

    private void Fall()
    {
        float step = fallSpeedPixels * Pixel * SpeedScale * Time.fixedDeltaTime;
        Direction = Vector2.down;

        Vector2 feet = FeetCenter();
        Vector2 origin = feet + Vector2.up * Pixel;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, step + Pixel, groundMask);

        if (hit.collider != null)
        {
            float down = Mathf.Max(0f, hit.distance - Pixel);
            _rb.MovePosition(_rb.position + Vector2.down * down);
            Land(feet.y - down);
            return;
        }

        _rb.MovePosition(_rb.position + Vector2.down * step);
    }

    private void Land(float feetY)
    {
        LevelContext level = LevelContext.Current;
        EnemySpawner spawner = level != null ? level.Enemies : null;

        if (spawner != null && feetY <= spawner.FloorY + floorTolerancePixels * Pixel)
        {
            TransformIntoChaser(spawner);
            return;
        }

        // Otra plataforma: repite el recorrido.
        _dirX = ChooseDirection();
        _turnsLeft = edgeTurns;
        _phase = Phase.Walking;
    }

    private float ChooseDirection()
    {
        switch (landingDirection)
        {
            case LandingDirection.Random:
                return Random.value < 0.5f ? -1f : 1f;
            case LandingDirection.TowardJack:
                if (JackTarget.TryGetPosition(out Vector2 jack))
                    return jack.x < _col.bounds.center.x ? -1f : 1f;
                return _dirX;
            default:
                return _dirX;
        }
    }

    private void TransformIntoChaser(EnemySpawner spawner)
    {
        Enemy prefab = WeightedEnemy.Pick(transformInto);
        if (prefab != null) spawner.SpawnWithSpark(prefab, _col.bounds.center);
        else Debug.LogWarning("PlatformWalker: 'Transform Into' vacío; la momia solo desaparece.", this);

        gameObject.SetActive(false);   // sale del registro al instante
        Destroy(gameObject);
    }

    private Vector2 FeetCenter()
    {
        Bounds b = _col.bounds;
        return new Vector2(b.center.x, b.min.y);
    }

    private bool GroundAt(float x)
    {
        Vector2 origin = new(x, FeetCenter().y + Pixel);
        return Physics2D.Raycast(origin, Vector2.down, (groundProbePixels + 1f) * Pixel, groundMask).collider != null;
    }
}