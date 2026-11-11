using UnityEngine;

[RequireComponent(typeof(MovementSystem), typeof(FlightSystem))]
[RequireComponent(typeof(GroundChecker), typeof(PlayerInputReader))]
public class Jack : MonoBehaviour
{
    private MovementSystem _movement;
    private FlightSystem _flight;
    private GroundChecker _ground;
    private PlayerInputReader _input;
    private AnimationSystem _anim;

    private void Awake()
    {
        _movement = GetComponent<MovementSystem>();
        _flight = GetComponent<FlightSystem>();
        _ground = GetComponent<GroundChecker>();
        _input = GetComponent<PlayerInputReader>();
        _anim = GetComponentInChildren<AnimationSystem>();
    }

    private void OnEnable()
    {
        _input.JumpPressed += HandleJumpButton;
        _input.UpPressed += HandleUp;
    }

    private void OnDisable()
    {
        _input.JumpPressed -= HandleJumpButton;
        _input.UpPressed -= HandleUp;
    }

    private void HandleJumpButton() => _flight.OnJumpButton(_input.Vertical > 0);
    private void HandleUp() => _flight.OnUpPressed(_input.JumpHeld);

    private void Update()
    {
        float h = _input.Horizontal;

        _movement.SetDirection(h);
        _flight.SetAirInput(_input.JumpHeld, _input.Vertical < 0);

        _anim.Face(h);
        _anim.SetState(h != 0f, _ground.IsGrounded, _movement.VerticalVelocity);
    }
}