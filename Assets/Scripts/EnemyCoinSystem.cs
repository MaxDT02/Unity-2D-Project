using System.Collections.Generic;
using UnityEngine;

// Convierte a los enemigos en monedas mientras dura el power-up y las hace parpadear al acabar.
// Va en Systems del LevelKit. Escucha PowerupController.Started / Ended una sola vez.
public class EnemyCoinSystem : MonoBehaviour
{
    public static EnemyCoinSystem Current { get; private set; }
    public static bool CoinsActive => Current != null && Current._active;
    public static bool CoinVisible => Current == null || Current._visible;

    [Tooltip("Aspecto de moneda que se muestra sobre el enemigo (solo SpriteRenderer, sin collider).")]
    [SerializeField] private GameObject coinVisualPrefab;
    [Tooltip("Chispas al recoger una moneda (con OneShotEffect). Vacío = sin efecto.")]
    [SerializeField] private OneShotEffect collectEffect;
    [Tooltip("Segundos finales en los que las monedas parpadean.")]
    [SerializeField, Min(0f)] private float blinkSeconds = 1.5f;
    [Tooltip("Segundos de cada fase (visible u oculta) del parpadeo.")]
    [SerializeField, Min(0.02f)] private float blinkInterval = 0.12f;

    private PowerupController _power;
    private bool _started;
    private bool _active;
    private bool _visible = true;

    public GameObject CoinVisualPrefab => coinVisualPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Current = null;

    private void OnEnable()
    {
        Current = this;
        if (_started) Subscribe();
    }

    // En Start para que el PowerupController ya esté registrado, sea cual sea el orden de OnEnable.
    private void Start()
    {
        _started = true;
        Subscribe();
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        Unsubscribe();
        _active = false;
    }

    private void Subscribe()
    {
        _power = PowerupController.Current;
        if (_power == null)
        {
            Debug.LogWarning("EnemyCoinSystem: no hay PowerupController en la escena.", this);
            return;
        }
        _power.Started += OnStarted;
        _power.Ended += OnEnded;
    }

    private void Unsubscribe()
    {
        if (_power == null) return;
        _power.Started -= OnStarted;
        _power.Ended -= OnEnded;
        _power = null;
    }

    private void OnStarted()
    {
        _active = true;
        _visible = true;
        SetAll(true);
    }

    private void OnEnded()
    {
        _active = false;
        _visible = true;
        SetAll(false);
    }

    private void Update()
    {
        if (!_active || _power == null) return;

        bool visible = true;
        float remaining = _power.Remaining;
        if (remaining <= blinkSeconds)
        {
            int phase = Mathf.FloorToInt((blinkSeconds - remaining) / blinkInterval);
            visible = (phase & 1) == 1;       // empieza oculta: se nota el aviso desde el primer instante
        }

        if (visible == _visible) return;      // solo actúa cuando cambia la fase
        _visible = visible;

        IReadOnlyList<Enemy> all = Enemy.All;
        for (int i = 0; i < all.Count; i++)
            if (all[i].Coin != null) all[i].Coin.SetCoinVisible(visible);
    }

    public void PlayCollectEffect(Vector3 position) => OneShotEffect.Play(collectEffect, position);

    private static void SetAll(bool coin)
    {
        IReadOnlyList<Enemy> all = Enemy.All;
        for (int i = 0; i < all.Count; i++)
            if (all[i].Coin != null) all[i].Coin.SetCoin(coin);
    }
}