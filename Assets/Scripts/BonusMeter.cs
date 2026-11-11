using System;
using UnityEngine;

public class BonusMeter : MonoBehaviour
{
    public static BonusMeter Current { get; private set; }
    private BombSequence _bombs;
    [SerializeField] private float threshold = 10f;
    [SerializeField] private float litBombValue = 1f;
    [SerializeField] private float unlitBombValue = 0.5f;

    public float Value { get; private set; }
    public bool IsFull => Value >= threshold;
    public float Normalized => Mathf.Clamp01(Value / threshold);

    public event Action<float> ValueChanged;   // 0..1
    public event Action Filled;
    public event Action Emptied;

    private void OnEnable()
    {
        Current = this;
        LevelContext level = LevelContext.Current;
        if (level == null || level.Bombs == null) return;
        _bombs = level.Bombs;
        _bombs.BombCollected += OnBombCollected;
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        if (_bombs != null) _bombs.BombCollected -= OnBombCollected;
        _bombs = null;
    }

    private void OnBombCollected(Bomb bomb, bool wasLit) => AddBomb(wasLit);

    public void AddBomb(bool wasLit) => Add(wasLit ? litBombValue : unlitBombValue);

    public void Add(float amount)
    {
        if (IsFull || amount <= 0f) return;   // lleno: no suma hasta resetear
        Value = Mathf.Min(threshold, Value + amount);
        ValueChanged?.Invoke(Normalized);
        if (IsFull) Filled?.Invoke();
    }

    public void ResetMeter()
    {
        if (Value <= 0f) return;
        Value = 0f;
        ValueChanged?.Invoke(0f);
        Emptied?.Invoke();
    }

    [ContextMenu("Debug/+1 bomba encendida")] private void DebugLit() => AddBomb(true);
    [ContextMenu("Debug/+0.5 bomba apagada")] private void DebugUnlit() => AddBomb(false);
    [ContextMenu("Debug/Reset medidor")] private void DebugReset() => ResetMeter();
}