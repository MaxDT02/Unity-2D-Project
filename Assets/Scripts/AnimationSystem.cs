using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AnimationSystem : MonoBehaviour
{
    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int GroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VelYHash = Animator.StringToHash("VelY");

    private Animator _animator;
    private SpriteRenderer _sprite;
    private bool _faceLeft;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    public void SetState(bool moving, bool grounded, float velY)
    {
        _animator.SetBool(MovingHash, moving);
        _animator.SetBool(GroundedHash, grounded);
        _animator.SetFloat(VelYHash, velY);
    }

    // Los sprites base miran a la derecha. Sin input vuelve al sprite sin voltear.
    public void Face(float dirX) => _faceLeft = dirX < 0f;

    // Después del Animator: así los clips que animan flipX (capa Sequences) no pisan la orientación del juego.
    private void LateUpdate() => _sprite.flipX = _faceLeft;
}