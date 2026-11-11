using UnityEngine;
using UnityEngine.InputSystem;

// Hace aparecer una B cada vez que la puntuación cruza un múltiplo de "interval".
public class BonusCoinDirector : MonoBehaviour
{
    private CoinSpawner _spawner;
    [SerializeField, Min(1)] private int interval = 5000;
    [SerializeField, Min(0f)] private float respawnDelay = 0.5f;

    private int _bucket;       // último múltiplo ya procesado
    private int _pending;      // B pendientes de salir
    private Collectible _coin;

    private void OnEnable()
    {
        LevelContext level = LevelContext.Current;
        _spawner = level != null ? level.BonusCoinSpawner : null;

        GameSession session = GameSession.Instance;
        _bucket = session.Score / interval;
        session.ScoreChanged += OnScoreChanged;
        PlayerLifecycle.Respawned += OnJackRespawned;
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (GameSession.HasInstance) GameSession.Instance.ScoreChanged -= OnScoreChanged;
        PlayerLifecycle.Respawned -= OnJackRespawned;
    }

    private void OnScoreChanged(int score)
    {
        int bucket = score / interval;
        // Mientras el juego está parado (bonus, muerte) la puntuación no genera monedas.
        if (bucket > _bucket && !GameClock.IsPaused) _pending += bucket - _bucket;
        _bucket = bucket;
        TrySpawn();
    }

    private void TrySpawn()
    {
        if (_spawner == null || _pending <= 0 || _spawner.HasActive || GameClock.IsPaused) return;

        ScoreSystem score = ScoreSystem.Current;
        if (score != null && score.Multiplier >= score.MaxMultiplier)
        {
            _pending = 0;
            return;
        }

        _coin = _spawner.Spawn();
        if (_coin == null) return;

        _pending--;
        _coin.Collected += OnCoinCollected;
    }

    private void OnCoinCollected(Collectible coin)
    {
        coin.Collected -= OnCoinCollected;
        _coin = null;
        // Con retraso: así el multiplicador ya está actualizado cuando se decide si sale otra.
        Invoke(nameof(TrySpawn), respawnDelay);
    }

    [ContextMenu("Debug/Forzar B")]
    private void DebugForce()
    {
        _pending++;
        TrySpawn();
    }

    // Al reaparecer Jack no queda ninguna B en pantalla ni pendiente.
    private void OnJackRespawned()
    {
        CancelInvoke();
        _pending = 0;
        if (_coin != null)
        {
            _coin.Collected -= OnCoinCollected;
            _coin = null;
        }
        if (_spawner != null) _spawner.Clear();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Pulsa B para sacar una moneda B al momento
        if (Keyboard.current != null && Keyboard.current[Key.B].wasPressedThisFrame)
            DebugForce();
    }
#endif
}