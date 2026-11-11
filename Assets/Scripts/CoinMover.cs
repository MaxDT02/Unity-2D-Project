using UnityEngine;

// Velocidad constante por eje, gravedad opcional y rebote por eje.
// P: gravedad 0, rebota en X e Y.
// B: con gravedad, rebota solo en X, da un saltito al aparecer y solo avanza en horizontal si está apoyada.
[RequireComponent(typeof(Rigidbody2D))]
public class CoinMover : MonoBehaviour
{
    private const int MaxContacts = 8;

    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] private Vector2 speedPixels = new(45f, 70f);   // píxeles de juego por segundo, por eje
    [SerializeField] private float gravityScale = 0f;
    [SerializeField] private bool reflectX = true;
    [SerializeField] private bool reflectY = true;

    [Header("Con gravedad (moneda B)")]
    [Tooltip("Solo avanza en horizontal mientras está apoyada; al salir de la plataforma cae en vertical.")]
    [SerializeField] private bool walkOnlyWhenGrounded;
    [Tooltip("0 = cae con la gravedad. Mayor que 0 = velocidad de caída constante (px/s) al salir de la plataforma.")]
    [SerializeField, Min(0f)] private float fallSpeedPixels;
    [Tooltip("Saltito al aparecer. Requiere gravedad y Reflect Y desmarcado.")]
    [SerializeField, Min(0f)] private float spawnHopPixels;

    [Header("Contactos")]
    [Tooltip("Cuánto debe apuntar el contacto hacia delante para contar como choque.")]
    [SerializeField, Range(0.3f, 1f)] private float frontThreshold = 0.6f;
    [Tooltip("Cuánto debe estar el contacto justo debajo para contar como apoyada.")]
    [SerializeField, Range(0.5f, 1f)] private float groundedThreshold = 0.8f;

    private readonly ContactPoint2D[] _contacts = new ContactPoint2D[MaxContacts];
    private Rigidbody2D _rb;
    private Vector2 _speed;
    private float _fallSpeed;
    private float _dirX = 1f;
    private float _dirY = 1f;
    private bool _moving = true;
    private float _hopStart;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = gravityScale;
        _rb.freezeRotation = true;
        _speed = speedPixels / pixelsPerUnit;
        _fallSpeed = fallSpeedPixels / pixelsPerUnit;
    }

    private void Start()
    {
        if (spawnHopPixels <= 0f || gravityScale <= 0f || reflectY) return;

        float gravity = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
        float hopSpeed = Mathf.Sqrt(2f * gravity * spawnHopPixels / pixelsPerUnit);
        _rb.linearVelocity = new Vector2(0f, hopSpeed);
        _moving = false;               // quieta en horizontal hasta que aterrice
        _hopStart = Time.time;
    }

    public void SetDirection(Vector2 direction)
    {
        _dirX = direction.x < 0f ? -1f : 1f;
        _dirY = direction.y < 0f ? -1f : 1f;
    }

    private void FixedUpdate()
    {
        bool grounded = ScanContacts();

        Vector2 v = _rb.linearVelocity;
        bool walking = _moving && (!walkOnlyWhenGrounded || grounded);
        v.x = walking ? _dirX * _speed.x : 0f;

        if (reflectY) v.y = _dirY * _speed.y;                       // sin reflectY manda la gravedad
        else if (_moving && !grounded && _fallSpeed > 0f) v.y = -_fallSpeed;

        _rb.linearVelocity = v;
    }

    // Mira los contactos respecto al centro de la moneda (no depende del sentido de la normal).
    // Devuelve true si está apoyada. Gira solo si el contacto está delante, en el sentido del movimiento.
    private bool ScanContacts()
    {
        int count = _rb.GetContacts(_contacts);
        Vector2 center = _rb.position;
        bool grounded = false;

        for (int i = 0; i < count; i++)
        {
            Vector2 toContact = _contacts[i].point - center;
            float length = toContact.magnitude;
            if (length < 0.0001f) continue;

            Vector2 d = toContact / length;
            if (d.y < -groundedThreshold) grounded = true;

            if (reflectX && Mathf.Abs(d.x) > frontThreshold && Mathf.Sign(d.x) == _dirX) _dirX = -_dirX;
            if (reflectY && Mathf.Abs(d.y) > frontThreshold && Mathf.Sign(d.y) == _dirY) _dirY = -_dirY;
        }

        // Aterrizaje tras el saltito: a partir de aquí camina.
        if (!_moving && grounded && Time.time - _hopStart > 0.05f) _moving = true;

        return grounded;
    }
}