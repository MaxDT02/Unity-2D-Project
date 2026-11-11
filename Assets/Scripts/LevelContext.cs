using UnityEngine;

// Lo propio de cada nivel. Los sistemas del LevelKit lo consultan en vez de llevar referencias a mano.
[DefaultExecutionOrder(-200)]
public class LevelContext : MonoBehaviour
{
    public static LevelContext Current { get; private set; }

    [SerializeField] private BombSequence bombs;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private CoinSpawner powerCoinSpawner;
    [SerializeField] private CoinSpawner bonusCoinSpawner;
    [SerializeField] private EnemySpawner enemySpawner;

    public BombSequence Bombs => bombs;
    public PlayerSpawner PlayerSpawner => playerSpawner;
    public CoinSpawner PowerCoinSpawner => powerCoinSpawner;
    public CoinSpawner BonusCoinSpawner => bonusCoinSpawner;
    public EnemySpawner Enemies => enemySpawner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Current = null;

    private void Awake() => Current = this;

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }
}