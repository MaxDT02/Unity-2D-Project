using System;
using UnityEngine;

// Emite un único evento por cada "acción notable" de Jack. Lo escuchan la P (color), los 10 puntos,
// y más adelante lo que haga falta (sonidos, efectos...).
[RequireComponent(typeof(PlayerInputReader), typeof(GroundChecker))]
public class JackActions : MonoBehaviour
{
    public static event Action Performed;

    [SerializeField] private LayerMask solidLayers;
    [SerializeField, Range(0.1f, 1f)] private float normalThreshold = 0.5f;
    [SerializeField, Min(0f)] private float contactCooldown = 0.15f;

    private PlayerInputReader _input;
    private GroundChecker _ground;
    private bool _jumpedSinceGrounded;
    private int _lastActionFrame = -1;
    private float _lastContactTime = float.NegativeInfinity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Performed = null;

    private void Awake()
    {
        _input = GetComponent<PlayerInputReader>();
        _ground = GetComponent<GroundChecker>();
    }

    private void OnEnable()
    {
        _input.JumpPressed += OnJump;
        _input.UpPressed += OnUp;
        _ground.LeftGround += OnLeftGround;
        _ground.Landed += OnLanded;
    }

    private void OnDisable()
    {
        _input.JumpPressed -= OnJump;
        _input.UpPressed -= OnUp;
        _ground.LeftGround -= OnLeftGround;
        _ground.Landed -= OnLanded;
    }

    // Cada pulsación cuenta: despegue y cada aleteo en el aire.
    private void OnJump() => Raise(true);

    // Arriba solo despega si está en suelo; en el aire no es una acción.
    private void OnUp()
    {
        if (_ground.IsGrounded) Raise(true);
    }

    // Caída por el borde: solo si no ha saltado (si saltó, ya se contó).
    private void OnLeftGround()
    {
        if (!_jumpedSinceGrounded) Raise(false);
    }

    private void OnLanded() => _jumpedSinceGrounded = false;

    // Techo o lado, solo en el aire.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_ground.IsGrounded) return;
        if ((solidLayers.value & (1 << collision.gameObject.layer)) == 0) return;
        if (Time.time - _lastContactTime < contactCooldown) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 n = collision.GetContact(i).normal;
            if (n.y < -normalThreshold || Mathf.Abs(n.x) > normalThreshold)
            {
                _lastContactTime = Time.time;
                Raise(false);
                return;
            }
        }
    }

    private void Raise(bool isJump)
    {
        if (isJump) _jumpedSinceGrounded = true;
        if (Time.frameCount == _lastActionFrame) return;   // salto + arriba en el mismo frame = una acción
        _lastActionFrame = Time.frameCount;
        Performed?.Invoke();
    }
}