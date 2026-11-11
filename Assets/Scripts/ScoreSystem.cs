using System;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Current { get; private set; }

    [SerializeField] private int score;   // solo lectura: copia de GameSession.Score para verla en el Inspector
    [SerializeField] private int maxMultiplier = 5;
    public int MaxMultiplier => maxMultiplier;
    public int Score => GameSession.Instance.Score;
    public int Multiplier { get; private set; } = 1;
    public event Action<int> ScoreChanged;
    public event Action<int> MultiplierChanged;

    private void OnEnable()
    {
        Current = this;
        GameSession.Instance.ScoreChanged += OnSessionScoreChanged;
        score = GameSession.Instance.Score;
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        if (GameSession.HasInstance) GameSession.Instance.ScoreChanged -= OnSessionScoreChanged;
    }

    private void OnSessionScoreChanged(int value)
    {
        score = value;
        ScoreChanged?.Invoke(value);
    }

    // Suma los puntos con el multiplicador actual y devuelve el multiplicador aplicado.
    public int Award(int basePoints)
    {
        int multiplier = Multiplier;
        GameSession.Instance.AddScore(basePoints * multiplier);
        return multiplier;
    }

    public void SetMultiplier(int value)
    {
        value = Mathf.Clamp(value, 1, maxMultiplier);
        if (value == Multiplier) return;
        Multiplier = value;
        MultiplierChanged?.Invoke(Multiplier);
    }

    // Sube el multiplicador en 1 (lo usa la moneda B). false = ya estaba en el máximo.
    public bool IncreaseMultiplier()
    {
        if (Multiplier >= maxMultiplier) return false;
        SetMultiplier(Multiplier + 1);
        return true;
    }

    [ContextMenu("Debug/Multiplicador +1")] private void DebugMult() => SetMultiplier(Multiplier + 1);
}