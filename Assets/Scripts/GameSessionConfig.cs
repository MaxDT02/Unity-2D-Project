using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Game Session Config")]
public class GameSessionConfig : ScriptableObject
{
    [Min(0)] public int startLives = 3;
    [Min(1)] public int maxLives = 5;
    [Min(1)] public int startRound = 1;
    [Tooltip("Récord inicial si no hay uno guardado. Ajusta al valor que quieras.")]
    public int defaultHiScore = 10000;
    public string hiScoreKey = "BJ_HiScore";

    [Tooltip("Escenas de nivel en orden. Tras la última se vuelve a la primera.")]
    public string[] levelScenes = { "Level1", "Level2", "Level3", "Level4" };

    public string SceneNameForRound(int round)
    {
        if (levelScenes == null || levelScenes.Length == 0) return null;
        int n = levelScenes.Length;
        return levelScenes[(((round - 1) % n) + n) % n];
    }

    [Header("Transición entre escenas")]
    public TransitionStyle transition = TransitionStyle.Cut;
    [Min(0f)] public float fadeDuration = 0.4f;
}

public enum TransitionStyle { Cut, Fade }