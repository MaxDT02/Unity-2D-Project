using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Palette Cycle")]
public class PaletteCycle : ScriptableObject
{
    public enum Kind { Time, Indexed }

    public const int RampSteps = 8;

    public Kind kind = Kind.Time;

    [Header("Kind = Time")]
    [Tooltip("Un ciclo completo. El último color debe igualar al primero.")]
    public Gradient gradient;
    [Tooltip("Ciclos por segundo (0,573 = 1,75 s por ciclo).")]
    public float cyclesPerSecond = 0.5f;

    [Header("Kind = Indexed")]
    public Color[] colors = { Color.white };

    public int Count => colors != null ? colors.Length : 0;

    public Color GetColor(int index)
    {
        int n = Count;
        if (n == 0) return Color.white;
        return colors[((index % n) + n) % n];
    }

    public Color EvaluateTime(float seconds)
    {
        if (gradient == null) return Color.white;
        float t = seconds * cyclesPerSecond;
        return gradient.Evaluate(t - Mathf.Floor(t));
    }

    // shade 0 = color base, shade 7 = casi blanco. Da la vuelta (mod 8).
    public Color EvaluateRamp(int baseIndex, int shade, float light)
    {
        int s = ((shade % RampSteps) + RampSteps) % RampSteps;
        return Color.Lerp(GetColor(baseIndex), Color.white, light * s / (RampSteps - 1));
    }
}