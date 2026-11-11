using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D _rb;
    private float _direction;

    public float SpeedMultiplier { get; set; } = 1f;
    public float VerticalVelocity => _rb.linearVelocityY;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void SetDirection(float dirX) => _direction = dirX;
    public void Stop() => _direction = 0f;

    private void FixedUpdate() =>
        _rb.linearVelocityX = _direction * speed * SpeedMultiplier;
}