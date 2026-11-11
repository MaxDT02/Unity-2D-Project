using System;
using UnityEngine;

// Estado de la partida que sobrevive entre escenas. Se crea solo la primera vez que se pide.
public class GameSession : MonoBehaviour
{
    private static GameSession _instance;

    public static bool HasInstance => _instance != null;

    public static GameSession Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameSession>();
                if (_instance == null)
                    _instance = new GameObject("GameSession").AddComponent<GameSession>();
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _instance = null;

    [Tooltip("Opcional. Si está vacío se busca Resources/GameSessionConfig.")]
    [SerializeField] private GameSessionConfig config;

    public int Score { get; private set; }
    public int HiScore { get; private set; }
    public int Lives { get; private set; }     // vidas de reserva (las que se ven abajo a la izquierda)
    public int Round { get; private set; }     // ronda absoluta: sigue subiendo aunque los niveles se repitan
    public int FinalScore { get; private set; }   // puntuación y ronda de la última partida terminada
    public int FinalRound { get; private set; }

    public event Action<int> ScoreChanged;
    public event Action<int> HiScoreChanged;
    public event Action<int> LivesChanged;
    public event Action<int> RoundChanged;
    public event Action RunEnded;

    public string CurrentSceneName => config.SceneNameForRound(Round);
    public GameSessionConfig Config => config;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (config == null) config = Resources.Load<GameSessionConfig>("GameSessionConfig");
        if (config == null) config = ScriptableObject.CreateInstance<GameSessionConfig>();

        StartNewRun();
    }

    public void StartNewRun()
    {
        HiScore = HighScoreTable.Top;
        Score = 0;
        Lives = config.startLives;
        Round = config.startRound;

        ScoreChanged?.Invoke(Score);
        HiScoreChanged?.Invoke(HiScore);
        LivesChanged?.Invoke(Lives);
        RoundChanged?.Invoke(Round);
    }

    public void AddScore(int points)
    {
        if (points <= 0) return;

        Score += points;
        ScoreChanged?.Invoke(Score);

        if (Score > HiScore)
        {
            HiScore = Score;
            HiScoreChanged?.Invoke(HiScore);
        }
    }

    // true = había una vida de reserva y se ha gastado; false = no quedaban (fin de partida).
    public bool TryConsumeLife()
    {
        if (Lives <= 0) return false;
        Lives--;
        LivesChanged?.Invoke(Lives);
        return true;
    }

    public void GainLife()
    {
        if (Lives >= config.maxLives) return;
        Lives++;
        LivesChanged?.Invoke(Lives);
    }

    public void NextRound() => SetRound(Round + 1);

    public void SetRound(int round)
    {
        Round = Mathf.Max(1, round);
        RoundChanged?.Invoke(Round);
    }

    public void EndRun()
    {
        FinalScore = Score;
        FinalRound = Round;
        RunEnded?.Invoke();
    }

    // Pruebas: clic derecho en el componente, con la partida en marcha.
    [ContextMenu("Debug/Add 1000")] private void DebugAdd() => AddScore(1000);
    [ContextMenu("Debug/Lose life")] private void DebugLoseLife() => TryConsumeLife();
    [ContextMenu("Debug/Gain life")] private void DebugGainLife() => GainLife();
    [ContextMenu("Debug/Next round")] private void DebugNextRound() => NextRound();
}