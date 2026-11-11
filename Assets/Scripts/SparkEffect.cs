using System;
using System.Collections.Generic;
using UnityEngine;

// Explosión de chispas. Cuando va por "revealAt" de su animación avisa para que aparezca el enemigo,
// y se destruye al terminar. Va con el tiempo de juego: se queda quieta durante las pausas.
[RequireComponent(typeof(Animator))]
public class SparkEffect : MonoBehaviour
{
    private static readonly List<SparkEffect> _all = new();
    public static int ActiveCount => _all.Count;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _all.Clear();

    [Tooltip("Momento de la animación (0 a 1) en el que aparece el enemigo. Las últimas chispas se ven ya con él delante.")]
    [SerializeField, Range(0f, 1f)] private float revealAt = 0.6f;

    private Animator _animator;
    private Action _onReveal;
    private bool _revealed;

    public static SparkEffect Play(SparkEffect prefab, Vector3 position, Action onReveal)
    {
        SparkEffect spark = Instantiate(prefab, position, Quaternion.identity);
        spark._onReveal = onReveal;
        return spark;
    }

    // Al reaparecer Jack: se cancelan las chispas en curso (y con ellas los enemigos que iban a salir).
    public static void DestroyAll()
    {
        for (int i = _all.Count - 1; i >= 0; i--)
        {
            GameObject go = _all[i].gameObject;
            go.SetActive(false);
            Destroy(go);
        }
    }

    private void Awake() => _animator = GetComponent<Animator>();
    private void OnEnable() => _all.Add(this);
    private void OnDisable() => _all.Remove(this);

    private void Update()
    {
        float t = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (!_revealed && t >= revealAt)
        {
            _revealed = true;
            Action reveal = _onReveal;
            _onReveal = null;
            reveal?.Invoke();
        }

        if (t >= 1f) Destroy(gameObject);
    }
}