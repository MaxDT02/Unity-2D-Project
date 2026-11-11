using System;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private Transform checkPoint;
    [SerializeField] private Vector2 size = new(0.4f, 0.06f);
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public event Action Landed;
    public event Action LeftGround;

    private void FixedUpdate()
    {
        bool grounded = Physics2D.OverlapBox(checkPoint.position, size, 0f, groundLayer);
        if (grounded == IsGrounded) return;

        IsGrounded = grounded;
        if (grounded) Landed?.Invoke();
        else LeftGround?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (checkPoint == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(checkPoint.position, size);
    }
}