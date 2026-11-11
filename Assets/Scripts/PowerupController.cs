using System;
using System.Collections;
using UnityEngine;

// Estados de la moneda P: Idle -> CoinActive (moneda en pantalla) -> Active (efecto 5 s) -> Idle.
// Los enemigos (bloque 5) solo escuchan Started / Ended y piden NextChainPoints().
public class PowerupController : MonoBehaviour
{
    public enum State { Idle, CoinActive, Active }

    public static PowerupController Current { get; private set; }

    [SerializeField] private PowerCoinConfig config;
    [SerializeField] private BonusMeter meter;

    public State CurrentState { get; private set; }
    public bool IsActive => CurrentState == State.Active;

    public event Action Started;
    public event Action Ended;

    private Collectible _coin;
    private SkinSystem _skin;
    private Coroutine _timer;
    private int _chain;
    private float _endTime;

    // Segundos que le quedan al efecto (0 si no está activo).
    public float Remaining => IsActive ? Mathf.Max(0f, _endTime - Time.time) : 0f;

    private void OnEnable()
    {
        Current = this;
        meter.Filled += OnMeterFilled;
        JackActions.Performed += OnJackAction;
        PlayerLifecycle.Respawned += OnJackRespawned;
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        meter.Filled -= OnMeterFilled;
        JackActions.Performed -= OnJackAction;
        PlayerLifecycle.Respawned -= OnJackRespawned;
    }

    // Puntos del siguiente enemigo-moneda recogido: 100, 200, 300, 500, 800, 1200, 2000...
    public int NextChainPoints() => config.ChainPoints(_chain++);

    private void OnMeterFilled()
    {
        if (CurrentState != State.Idle) return;

        PaletteDriver driver = PaletteDriver.Current;
        if (driver != null) driver.SetIndex(config.palette, 0);   // la P siempre empieza en azul

        LevelContext level = LevelContext.Current;
        _coin = level != null && level.PowerCoinSpawner != null ? level.PowerCoinSpawner.Spawn() : null;
        if (_coin == null) return;

        CurrentState = State.CoinActive;
        _coin.Collected += OnCoinCollected;
    }

    // Mientras la moneda está en pantalla, cada acción de Jack cambia el color de la P y del medidor.
    private void OnJackAction()
    {
        if (CurrentState != State.CoinActive) return;
        PaletteDriver driver = PaletteDriver.Current;
        if (driver != null) driver.Advance(config.palette);
    }

    private void OnCoinCollected(Collectible coin)
    {
        coin.Collected -= OnCoinCollected;
        _coin = null;

        CurrentState = State.Active;
        _chain = 0;

        PaletteDriver driver = PaletteDriver.Current;
        if (driver != null) driver.Capture(config.skinChannelId);   // primero congela el color...

        LevelContext level = LevelContext.Current;
        PlayerSpawner spawner = level != null ? level.PlayerSpawner : null;
        _skin = spawner != null && spawner.Current != null
            ? spawner.Current.GetComponentInChildren<SkinSystem>()
            : null;
        if (_skin != null) _skin.Apply(config.skinMaterial);        // ...y después tiñe a Jack

        Started?.Invoke();
        _timer = StartCoroutine(RunTimer());
    }

    private IEnumerator RunTimer()
    {
        _endTime = Time.time + config.duration;
        yield return new WaitForSeconds(config.duration);
        _timer = null;
        EndPowerup();
    }

    private void EndPowerup()
    {
        ReleaseSkin();
        Ended?.Invoke();
        meter.ResetMeter();
        CurrentState = State.Idle;
    }

    private void ReleaseSkin()
    {
        if (_skin != null) _skin.Restore();
        PaletteDriver driver = PaletteDriver.Current;
        if (driver != null) driver.Release(config.skinChannelId);
        _skin = null;
    }
    
    // Para cuando Jack muere en mitad del efecto: los enemigos vuelven y el medidor se vacía.
    public void EndEffect()
    {
        if (CurrentState != State.Active) return;
        if (_timer != null) { StopCoroutine(_timer); _timer = null; }
        EndPowerup();
    }

    // Si la P estaba en pantalla sin recoger, desaparece y el medidor se vacía.
    // Con el medidor a medias no hace nada (el progreso se conserva), y con el efecto activo
    // ya se cortó al morir (EndEffect).
    private void OnJackRespawned()
    {
        if (CurrentState != State.CoinActive) return;

        if (_coin != null) _coin.Collected -= OnCoinCollected;
        _coin = null;

        LevelContext level = LevelContext.Current;
        if (level != null && level.PowerCoinSpawner != null) level.PowerCoinSpawner.Clear();

        CurrentState = State.Idle;
        meter.ResetMeter();
    }
}