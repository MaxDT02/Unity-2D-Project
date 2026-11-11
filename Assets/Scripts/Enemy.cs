using System.Collections.Generic;
using UnityEngine;

// Lo que comparten todos los enemigos: el registro de los vivos y los ajustes de dificultad.
public class Enemy : MonoBehaviour
{
    private static readonly List<Enemy> _all = new();
    public static IReadOnlyList<Enemy> All => _all;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _all.Clear();

    [SerializeField] private EnemyKind kind;

    private EnemyMotion _motion;

    public EnemyKind Kind => kind;
    public float SpeedScale { get; private set; } = 1f;
    // Null si el prefab no tiene EnemyCoin.
    public EnemyCoin Coin { get; private set; }

    private void Awake()
    {
        _motion = GetComponent<EnemyMotion>();
        Coin = GetComponent<EnemyCoin>();
    }

    // Cada enemigo se ajusta solo al nacer, venga de donde venga (director, transformación...).
    private void Start()
    {
        EnemyDifficulty difficulty = EnemyDifficulty.Active;
        if (difficulty == null) return;

        int round = GameSession.Instance.Round;
        SpeedScale = difficulty.SpeedFor(kind, round);
        if (_motion != null) _motion.Configure(difficulty, round);
    }

    private void OnEnable() => _all.Add(this);
    private void OnDisable() => _all.Remove(this);

    public static void DestroyAll()
    {
        for (int i = _all.Count - 1; i >= 0; i--)
        {
            GameObject go = _all[i].gameObject;
            go.SetActive(false);     // sale del registro al instante
            Destroy(go);
        }
    }
}