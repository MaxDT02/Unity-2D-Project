using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(GroundChecker))]
public class FlightSystem : MonoBehaviour
{
    [SerializeField] private FlightConfig config;

    private Rigidbody2D _rb;
    private GroundChecker _ground;

    private float _lastJumpTime = float.NegativeInfinity;
    private bool _lastJumpWasPower;
    private bool _glideHeld;
    private bool _fastFallHeld;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ground = GetComponent<GroundChecker>();
    }

    // Se llama cada frame con el estado de los inputs que afectan al aire.
    public void SetAirInput(bool glideHeld, bool fastFallHeld)
    {
        _glideHeld = glideHeld;
        _fastFallHeld = fastFallHeld;
    }

    // Botón pulsado: salto en suelo, mejora a power jump o frenar subida.
    public void OnJumpButton(bool upHeld)
    {
        if (_ground.IsGrounded) Launch(upHeld);
        else if (!TryUpgradeToPower() && _rb.linearVelocityY > 0f) Brake();
    }

    // Arriba pulsado: salto en suelo, o mejora a power jump si el botón se pulsó justo antes.
    public void OnUpPressed(bool buttonHeld)
    {
        if (_ground.IsGrounded) Launch(buttonHeld);
        else TryUpgradeToPower();
    }

    private void Launch(bool power)
    {
        float height = power ? config.powerJumpHeight : config.jumpHeight;
        _rb.linearVelocityY = config.SpeedForHeight(height);
        _lastJumpTime = Time.time;
        _lastJumpWasPower = power;
    }

    // true = el input se consumió dentro de la ventana (aunque ya fuera power jump,
    // para que un doble pulso accidental no frene el salto).
    private bool TryUpgradeToPower()
    {
        if (Time.time - _lastJumpTime > config.powerJumpWindow) return false;
        if (!_lastJumpWasPower)
        {
            _rb.linearVelocityY = config.SpeedForHeight(config.powerJumpHeight);
            _lastJumpWasPower = true;
        }
        return true;
    }

    private void Brake() => _rb.linearVelocityY = 0f;

    private void FixedUpdate()
    {
        float gravity = config.gravityScale;
        float maxFall = config.maxFallSpeed;

        if (!_ground.IsGrounded)
        {
            if (_fastFallHeld)
            {
                gravity = config.fastFallGravityScale;
                maxFall = config.fastFallMaxSpeed;
            }
            else if (_glideHeld && _rb.linearVelocityY <= 0f)
            {
                gravity = config.glideGravityScale;
                maxFall = config.glideMaxFallSpeed;
            }
        }

        _rb.gravityScale = gravity;
        if (_rb.linearVelocityY < -maxFall) _rb.linearVelocityY = -maxFall;
    }
}