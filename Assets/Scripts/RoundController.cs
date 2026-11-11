using System.Collections;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    public enum State { Playing, Clearing, Dying, Loading }

    public static RoundController Current { get; private set; }

    [SerializeField] private RoundFlowConfig config;
    [Tooltip("Al completar el nivel, en orden: animación de Jack, pantalla de bonus...")]
    [SerializeField] private RoundStep[] clearSteps;
    [Tooltip("Al morir Jack, en orden: animación de muerte...")]
    [SerializeField] private RoundStep[] deathSteps;

    public State CurrentState { get; private set; } = State.Playing;
    public RoundStats Stats { get; } = new RoundStats();

    private LevelContext _level;

    private void OnEnable()
    {
        Current = this;
        _level = LevelContext.Current;
        if (_level == null || _level.Bombs == null)
        {
            Debug.LogError("RoundController: falta el LevelContext (o su BombSequence) en la escena.", this);
            return;
        }

        _level.Bombs.BombCollected += OnBombCollected;
        _level.Bombs.AllCollected += OnAllCollected;
        PlayerLifecycle.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        PlayerLifecycle.Died -= OnPlayerDied;
        if (_level != null && _level.Bombs != null)
        {
            _level.Bombs.BombCollected -= OnBombCollected;
            _level.Bombs.AllCollected -= OnAllCollected;
        }
    }

    private void OnBombCollected(Bomb bomb, bool wasLit)
    {
        Stats.BombsCollected++;
        if (wasLit) Stats.LitBombs++;
    }

    private void OnAllCollected()
    {
        if (CurrentState != State.Playing) return;
        StartCoroutine(ClearRoutine());
    }

    private void OnPlayerDied()
    {
        if (CurrentState != State.Playing) return;   // si ya se está completando el nivel, no cuenta
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        CurrentState = State.Clearing;
        Freeze();
        yield return new WaitForSecondsRealtime(config.clearHoldDelay);
        yield return RunSteps(clearSteps);

        CurrentState = State.Loading;
        SceneTransition.LoadNextRound();
    }

    private IEnumerator DeathRoutine()
    {
        CurrentState = State.Dying;
        if (PowerupController.Current != null) PowerupController.Current.EndEffect();
        Freeze();

        yield return RunSteps(deathSteps);
        yield return new WaitForSecondsRealtime(config.respawnDelay);

        if (GameSession.Instance.TryConsumeLife())
        {
            GameClock.Resume();
            PlayerLifecycle.ReportRespawn();       // los sistemas limpian lo suyo (monedas, medidor...)
            _level.PlayerSpawner.Spawn();
            CurrentState = State.Playing;
            yield break;
        }

        // Sin vidas de reserva: fin de partida.
        CurrentState = State.Loading;
        GameSession.Instance.EndRun();
        ScreenFlow.ShowGameOver();
    }

    private IEnumerator RunSteps(RoundStep[] steps)
    {
        if (steps == null) yield break;
        foreach (RoundStep step in steps)
            if (step != null && step.ShouldRun(Stats))
                yield return step.Run(Stats);
    }

    // Jack se queda donde está y el mundo se para.
    private void Freeze()
    {
        GameObject jack = _level.PlayerSpawner != null ? _level.PlayerSpawner.Current : null;
        if (jack != null && jack.TryGetComponent(out JackLock jackLock)) jackLock.Lock();
        GameClock.Pause();
    }

    // Para pruebas (las usa DebugRoundKeys). Sin menús contextuales: meten tirones de frame en el editor.
    public void DebugForceClear() => OnAllCollected();
}