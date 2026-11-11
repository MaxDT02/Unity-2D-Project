using UnityEngine;

// Perseguidor rebotante. Cada eje (X, Y) se configura por separado:
//  - Steer: se acerca a Jack en ese eje y se queda quieto en él al alinearse.
//  - Drift: avanza y solo cambia al chocar; con Random Drift además cambia solo, al azar, de sentido y velocidad.
// Esfera = X Steer + Y Drift. Orbe = X Drift + Y Steer. Maza = X Steer + Y Steer.
// "Think Seconds" hace la persecución menos directa (mira a Jack cada cierto tiempo, no a cada fotograma).
// Si choca yendo hacia Jack y el otro eje es Steer, puede rodear el obstáculo (Detour Enabled).
// El origen del prefab debe estar en el centro del sprite.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BouncingChaser : EnemyMotion
{
    public enum AxisMode { Steer, Drift }
    public enum DriftStart { Random, Positive, Negative }

    private struct Axis
    {
        public AxisMode mode;
        public float drift;          // sentido de la deriva (+1 / -1)
        public float speedFactor;    // multiplicador aleatorio de la deriva
        public float nextChange;     // cuándo vuelve a cambiar al azar
        public float reboundSign;    // sentido forzado tras un rebote en Steer
        public float reboundUntil;   // hasta cuándo dura ese rebote
    }

    [SerializeField] private float pixelsPerUnit = 100f;
    [Tooltip("Capas que le cierran el paso (Ground).")]
    [SerializeField] private LayerMask blockMask;
    [Tooltip("Velocidad base por eje (px/s), antes de los factores y de la velocidad por ronda.")]
    [SerializeField, Min(0f)] private float speedPixels = 40f;
    [Tooltip("Multiplica la velocidad del eje que persigue a Jack.")]
    [SerializeField, Min(0f)] private float steerSpeedFactor = 1f;
    [Tooltip("Multiplica la velocidad del eje que deriva.")]
    [SerializeField, Min(0f)] private float driftSpeedFactor = 1f;

    [Header("Eje horizontal")]
    [SerializeField] private AxisMode horizontal = AxisMode.Steer;
    [Tooltip("Solo Drift: sentido inicial (Positive = derecha).")]
    [SerializeField] private DriftStart horizontalStart = DriftStart.Random;

    [Header("Eje vertical")]
    [SerializeField] private AxisMode vertical = AxisMode.Drift;
    [Tooltip("Solo Drift: sentido inicial (Positive = arriba).")]
    [SerializeField] private DriftStart verticalStart = DriftStart.Positive;

    [Header("Persecución y rebote")]
    [Tooltip("Cada cuántos segundos mira dónde está Jack. 0 = persecución directa; más = menos directa.")]
    [SerializeField, Min(0f)] private float thinkSeconds = 0f;
    [Tooltip("Margen aleatorio (px) alrededor de Jack que se sortea en cada 'pensada' (necesita Think Seconds > 0). Evita rebotar en línea recta cuando está alineado. 0 = exacto.")]
    [SerializeField, Min(0f)] private float steerWanderPixels = 0f;
    [Tooltip("Margen (px) a partir del cual se considera alineado con Jack en un eje Steer.")]
    [SerializeField, Min(0f)] private float alignedPixels = 3f;
    [Tooltip("Tras chocar en un eje Steer, tiempo que se aleja antes de volver a buscar a Jack.")]
    [SerializeField, Min(0f)] private float reboundSeconds = 0.6f;
    [SerializeField, Min(0f)] private float gapPixels = 1f;

    [Header("Deriva aleatoria (ejes Drift)")]
    [SerializeField] private bool randomDrift;
    [Tooltip("Tiempo (mín, máx) entre cambios al azar de sentido y velocidad.")]
    [SerializeField] private Vector2 driftChangeSeconds = new Vector2(1f, 3f);
    [Tooltip("Multiplicador (mín, máx) de velocidad que se sortea en cada cambio.")]
    [SerializeField] private Vector2 driftSpeedRange = new Vector2(0.4f, 1.2f);

    [Header("Rodeo de obstáculos")]
    [Tooltip("Si choca yendo hacia Jack, rodea el obstáculo. Solo para perseguidores directos (maza).")]
    [SerializeField] private bool detourEnabled = true;
    [Tooltip("Hasta dónde busca (px) un carril libre a cada lado.")]
    [SerializeField, Min(1f)] private float scanPixels = 160f;
    [Tooltip("Largo de carril libre que exige y distancia que avanza tras el obstáculo. Debe superar el grosor de la plataforma más el tamaño del enemigo.")]
    [SerializeField, Min(1f)] private float clearPixels = 32f;
    [Tooltip("Prefiere el lado de Jack salvo que el otro carril esté más cerca por más de estos píxeles.")]
    [SerializeField, Min(0f)] private float sideBiasPixels = 24f;
    [Tooltip("Tiempo máximo de un rodeo; si se pasa, rebota como siempre.")]
    [SerializeField, Min(0.1f)] private float detourMaxSeconds = 3f;

    private readonly Axis[] _axes = new Axis[2];
    private Rigidbody2D _rb;
    private Collider2D _col;
    private ContactProbe2D _probe;

    // Última posición de Jack "pensada" (con retraso si thinkSeconds > 0).
    private Vector2 _knownJack;
    private Vector2 _wander;
    private bool _hasKnown;
    private float _nextThink;

    // Rodeo activo: se desplaza por _sideAxis y sigue empujando por _pushAxis hacia Jack.
    private int _sideAxis = -1;
    private float _sideSign;
    private int _pushAxis;
    private float _pushSign;
    private float _progress;
    private float _detourEnd;
    private bool _sideFlipped;

    private float Pixel => 1f / pixelsPerUnit;
    private static Vector2 AxisVector(int axis) => axis == 0 ? Vector2.right : Vector2.up;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _col = GetComponent<Collider2D>();

        if (blockMask.value == 0)
        {
            blockMask = LayerMask.GetMask("Ground");
            Debug.LogWarning("BouncingChaser: 'Block Mask' vacío; uso la capa Ground.", this);
        }
        _probe = new ContactProbe2D(_col, blockMask, gapPixels * Pixel);

        _axes[0] = NewAxis(horizontal, horizontalStart);
        _axes[1] = NewAxis(vertical, verticalStart);
    }

    private Axis NewAxis(AxisMode mode, DriftStart start) => new Axis
    {
        mode = mode,
        drift = start switch
        {
            DriftStart.Positive => 1f,
            DriftStart.Negative => -1f,
            _ => RandomSign(),
        },
        speedFactor = 1f,
        nextChange = Time.time + NextChangeDelay(),
    };

    private static float RandomSign() => Random.value < 0.5f ? -1f : 1f;

    private float NextChangeDelay() =>
        Random.Range(Mathf.Min(driftChangeSeconds.x, driftChangeSeconds.y), Mathf.Max(driftChangeSeconds.x, driftChangeSeconds.y));

    private void FixedUpdate()
    {
        float baseStep = speedPixels * Pixel * SpeedScale * Time.fixedDeltaTime;
        Vector2 toJack = ThinkJack();

        Vector2 dir = Vector2.zero;
        Vector2 move = Vector2.zero;
        bool bounced = false;

        if (_sideAxis >= 0)
        {
            RunDetour(baseStep * steerSpeedFactor, ref dir, ref move);
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                float sign = DesiredSign(i, toJack[i]);
                dir[i] = sign;
                if (sign == 0f) continue;

                float step = AxisStep(i, baseStep);
                float free = _probe.FreeDistance(AxisVector(i) * sign, step);
                move[i] = sign * free;

                if (free < step)
                {
                    if (!TryStartDetour(i, sign, toJack)) Bounce(i, sign);
                    bounced = true;
                }
            }
        }

        Direction = dir;

        // Esquina: cada eje por separado está libre pero la diagonal roza. Se desliza por un solo eje.
        if (!bounced && move.x != 0f && move.y != 0f && _probe.IsBlocked(move.normalized, move.magnitude))
            move[1 - SlideAxis(toJack)] = 0f;

        if (move != Vector2.zero) _rb.MovePosition(_rb.position + move);
    }

    // Vector hacia donde cree que está Jack. Con thinkSeconds > 0 se refresca solo de vez en cuando,
    // y con steerWanderPixels > 0 se desvía un margen aleatorio para no quedarse exactamente alineado.
    private Vector2 ThinkJack()
    {
        if (JackTarget.TryGetPosition(out Vector2 jack))
        {
            if (!_hasKnown || thinkSeconds <= 0f || Time.time >= _nextThink)
            {
                _knownJack = jack;
                _hasKnown = true;
                _nextThink = Time.time + thinkSeconds;

                if (steerWanderPixels > 0f && thinkSeconds > 0f)
                {
                    float w = steerWanderPixels * Pixel;
                    _wander = new Vector2(Random.Range(-w, w), Random.Range(-w, w));
                }
            }
        }
        return _hasKnown ? _knownJack + _wander - (Vector2)_col.bounds.center : Vector2.zero;
    }

    private float AxisStep(int i, float baseStep) =>
        _axes[i].mode == AxisMode.Steer
            ? baseStep * steerSpeedFactor
            : baseStep * driftSpeedFactor * _axes[i].speedFactor;

    // En una esquina sigue el eje que persigue a Jack; si persiguen los dos, el que tiene más camino por recorrer.
    private int SlideAxis(Vector2 toJack)
    {
        bool steerX = _axes[0].mode == AxisMode.Steer;
        bool steerY = _axes[1].mode == AxisMode.Steer;
        if (steerX != steerY) return steerX ? 0 : 1;
        return Mathf.Abs(toJack.y) > Mathf.Abs(toJack.x) ? 1 : 0;
    }

    private float DesiredSign(int i, float delta)
    {
        ref Axis axis = ref _axes[i];

        if (axis.mode == AxisMode.Drift)
        {
            if (randomDrift && Time.time >= axis.nextChange) RerollDrift(ref axis);
            return axis.drift;
        }

        if (Time.time < axis.reboundUntil) return axis.reboundSign;
        if (!_hasKnown) return 0f;
        return Mathf.Abs(delta) <= alignedPixels * Pixel ? 0f : Mathf.Sign(delta);
    }

    private void RerollDrift(ref Axis axis)
    {
        axis.drift = RandomSign();
        axis.speedFactor = Random.Range(Mathf.Min(driftSpeedRange.x, driftSpeedRange.y), Mathf.Max(driftSpeedRange.x, driftSpeedRange.y));
        axis.nextChange = Time.time + NextChangeDelay();
    }

    private void Bounce(int i, float sign)
    {
        ref Axis axis = ref _axes[i];
        if (axis.mode == AxisMode.Drift)
        {
            axis.drift = -sign;
        }
        else
        {
            axis.reboundSign = -sign;
            axis.reboundUntil = Time.time + reboundSeconds;
        }
    }

    // ---- Rodeo ----

    // Solo si choca yendo hacia Jack y el otro eje lo controla el propio enemigo (Steer).
    private bool TryStartDetour(int pushAxis, float pushSign, Vector2 toJack)
    {
        int side = 1 - pushAxis;
        if (!detourEnabled || !_hasKnown || _axes[side].mode != AxisMode.Steer) return false;

        float delta = toJack[pushAxis];
        if (Mathf.Abs(delta) <= alignedPixels * Pixel || Mathf.Sign(delta) != pushSign) return false;

        float sideSign = ChooseSide(pushAxis, pushSign, side, toJack[side]);
        if (sideSign == 0f) return false;

        _sideAxis = side;
        _sideSign = sideSign;
        _pushAxis = pushAxis;
        _pushSign = pushSign;
        _progress = 0f;
        _detourEnd = Time.time + detourMaxSeconds;
        _sideFlipped = false;
        return true;
    }

    // Lado (-1/+1) cuyo carril libre está más cerca; prefiere el de Jack. 0 = no hay carril.
    private float ChooseSide(int pushAxis, float pushSign, int side, float sideDeltaToJack)
    {
        float toward = sideDeltaToJack < 0f ? -1f : 1f;
        int kToward = ScanLane(pushAxis, pushSign, side, toward);
        int kAway = ScanLane(pushAxis, pushSign, side, -toward);

        if (kToward >= 0 && (kAway < 0 || kToward <= kAway + sideBiasPixels)) return toward;
        if (kAway >= 0) return -toward;
        return 0f;
    }

    // Píxeles que hay que desplazarse hacia ese lado hasta que se pueda avanzar clearPixels. -1 = ninguno.
    private int ScanLane(int pushAxis, float pushSign, int side, float sideSign)
    {
        Vector2 sideDir = AxisVector(side) * sideSign;
        Vector2 pushDir = AxisVector(pushAxis) * pushSign;
        int reach = Mathf.FloorToInt(_probe.FreeDistance(sideDir, scanPixels * Pixel) / Pixel);

        for (int k = 1; k <= reach; k++)
            if (!_probe.IsBlocked(pushDir, clearPixels * Pixel, sideDir * (k * Pixel))) return k;
        return -1;
    }

    private void RunDetour(float step, ref Vector2 dir, ref Vector2 move)
    {
        // Lateral: hacia el carril. Si topa con una pared prueba el otro lado una vez.
        dir[_sideAxis] = _sideSign;
        float sideFree = _probe.FreeDistance(AxisVector(_sideAxis) * _sideSign, step);
        move[_sideAxis] = _sideSign * sideFree;
        if (sideFree < step)
        {
            if (_sideFlipped)
            {
                EndDetour();
                return;
            }
            _sideSign = -_sideSign;
            _sideFlipped = true;
        }

        // Empuje: sigue intentando avanzar hacia Jack; en cuanto el camino se abre, avanza.
        dir[_pushAxis] = _pushSign;
        float pushFree = _probe.FreeDistance(AxisVector(_pushAxis) * _pushSign, step);
        move[_pushAxis] = _pushSign * pushFree;
        _progress += pushFree;

        if (_progress >= clearPixels * Pixel || Time.time >= _detourEnd) EndDetour();
    }

    private void EndDetour() => _sideAxis = -1;
}