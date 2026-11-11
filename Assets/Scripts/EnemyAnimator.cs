using UnityEngine;

// Anima un enemigo según su movimiento. Parámetros opcionales del Animator:
//  - Float "DirX" y "DirY": la dirección actual,
//  - Bool "Vertical": si se mueve en vertical,
//  - Float "Spin": +1 / -1 según el último sentido horizontal (para animaciones que giran y se invierten),
//  - estado "Turn": se reproduce una vez al cambiar de dirección.
// Va en el objeto que tiene el Animator y el SpriteRenderer.
[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class EnemyAnimator : MonoBehaviour
{
    [Tooltip("Voltear el sprite según vaya a la izquierda o a la derecha. Apágalo si hay sprites distintos para cada lado.")]
    [SerializeField] private bool flipByDirection = true;
    [Tooltip("Los sprites horizontales miran a la izquierda.")]
    [SerializeField] private bool spritesFaceLeft = true;
    [Tooltip("Mira según el signo de X aunque se mueva más en vertical, y conserva el último lado. Sin giro ni 'Vertical'.")]
    [SerializeField] private bool faceByHorizontalOnly;
    [Tooltip("Estado que se reproduce una vez al cambiar de dirección. Vacío = sin giro.")]
    [SerializeField] private string turnState = "Turn";
    [SerializeField] private string verticalParameter = "Vertical";
    [SerializeField] private string dirXParameter = "DirX";
    [SerializeField] private string dirYParameter = "DirY";
    [SerializeField] private string spinParameter = "Spin";
    [Tooltip("Invierte el sentido de giro (si la animación gira al revés de lo esperado).")]
    [SerializeField] private bool invertSpin;

    private EnemyMotion _motion;
    private Animator _animator;
    private SpriteRenderer _sprite;
    private int _verticalHash, _dirXHash, _dirYHash, _spinHash, _turnHash;
    private bool _hasVertical, _hasDirX, _hasDirY, _hasSpin, _hasTurn;
    private Vector2Int _facing;
    private bool _faceRight;

    private void Awake()
    {
        _motion = GetComponentInParent<EnemyMotion>();
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;   // siguen animados con el juego pausado

        _verticalHash = Animator.StringToHash(verticalParameter);
        _dirXHash = Animator.StringToHash(dirXParameter);
        _dirYHash = Animator.StringToHash(dirYParameter);
        _spinHash = Animator.StringToHash(spinParameter);
        _turnHash = Animator.StringToHash(turnState);

        _hasTurn = !string.IsNullOrEmpty(turnState) && !faceByHorizontalOnly;
        _hasVertical = HasParameter(_verticalHash, AnimatorControllerParameterType.Bool);
        _hasDirX = HasParameter(_dirXHash, AnimatorControllerParameterType.Float);
        _hasDirY = HasParameter(_dirYHash, AnimatorControllerParameterType.Float);
        _hasSpin = HasParameter(_spinHash, AnimatorControllerParameterType.Float);
        _faceRight = !spritesFaceLeft;   // empieza con el sprite sin voltear

        if (_hasSpin) _animator.SetFloat(_spinHash, invertSpin ? -1f : 1f);   // gira desde el primer instante
    }

    private bool HasParameter(int hash, AnimatorControllerParameterType type)
    {
        if (_animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter p in _animator.parameters)
            if (p.nameHash == hash && p.type == type) return true;
        return false;
    }

    private void Update()
    {
        if (_motion == null) return;

        Vector2 dir = _motion.Direction;
        if (_hasDirX) _animator.SetFloat(_dirXHash, dir.x);
        if (_hasDirY) _animator.SetFloat(_dirYHash, dir.y);

        // Último sentido horizontal: solo cambia cuando X cambia de signo (rebote lateral).
        if (dir.x != 0f)
        {
            int sx = dir.x > 0f ? 1 : -1;
            if (faceByHorizontalOnly) _faceRight = sx > 0;
            if (_hasSpin) _animator.SetFloat(_spinHash, invertSpin ? -sx : sx);
        }

        if (faceByHorizontalOnly) return;

        Vector2Int facing = Cardinal(dir);
        if (facing == Vector2Int.zero || facing == _facing) return;   // parado o misma dirección

        _facing = facing;
        if (facing.x != 0) _faceRight = facing.x > 0;
        if (_hasVertical) _animator.SetBool(_verticalHash, facing.y != 0);
        if (_hasTurn) _animator.Play(_turnHash, 0, 0f);
    }

    // Después del Animator, para que ninguna animación pise el giro del sprite.
    private void LateUpdate()
    {
        if (flipByDirection) _sprite.flipX = spritesFaceLeft ? _faceRight : !_faceRight;
    }

    private static Vector2Int Cardinal(Vector2 d)
    {
        if (d == Vector2.zero) return Vector2Int.zero;
        return Mathf.Abs(d.x) >= Mathf.Abs(d.y)
            ? new Vector2Int(d.x > 0f ? 1 : -1, 0)
            : new Vector2Int(0, d.y > 0f ? 1 : -1);
    }
}