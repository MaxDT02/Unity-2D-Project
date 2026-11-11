using System;
using UnityEngine;

public enum RankingMode { Entry, View }

// Rutas entre pantallas. Si una escena no está configurada, salta a la siguiente razonable
// (y al final reinicia la partida, que es lo que hacía el juego antes de existir las pantallas).
public static class ScreenFlow
{
    private static ScreenFlowConfig _config;

    // Qué modo debe mostrar la escena Ranking al abrirse.
    public static RankingMode PendingRanking { get; private set; } = RankingMode.View;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _config = null;
        PendingRanking = RankingMode.View;
    }

    private static ScreenFlowConfig Config
    {
        get
        {
            if (_config == null) _config = Resources.Load<ScreenFlowConfig>("ScreenFlowConfig");
            if (_config == null) _config = ScriptableObject.CreateInstance<ScreenFlowConfig>();
            return _config;
        }
    }

    public static void StartGame() => SceneTransition.RestartRun();

    public static void ShowTitle() => Go(Config.titleScene, StartGame);
    public static void ShowInstructions() => Go(Config.instructionsScene, ShowTitle);
    public static void ShowGameOver() => Go(Config.gameOverScene, AfterGameOver);
    public static void ShowOptions() => Go(Config.optionsScene, ShowTitle);

    public static void ShowRanking(RankingMode mode)
    {
        PendingRanking = mode;
        Go(Config.rankingScene, ShowTitle);
    }

    // Tras el Game Over: si la puntuación entra en la tabla, a escribir el nombre; si no, al título.
    public static void AfterGameOver()
    {
        if (HighScoreTable.RankFor(GameSession.Instance.FinalScore) >= 0) ShowRanking(RankingMode.Entry);
        else ShowTitle();
    }

    private static void Go(string scene, Action fallback)
    {
        if (!string.IsNullOrEmpty(scene)) SceneTransition.Load(scene);
        else fallback();
    }
}