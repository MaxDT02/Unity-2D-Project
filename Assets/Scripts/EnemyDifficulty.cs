using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Enemy Difficulty")]
public class EnemyDifficulty : ScriptableObject
{
    // El director la fija al activarse; los enemigos la consultan al nacer.
    public static EnemyDifficulty Active { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Active = null;

    [Serializable]
    public struct RoundValue
    {
        public int fromRound;
        public int value;
    }

    [Serializable]
    public struct KindSpeed
    {
        public EnemyKind kind;
        [Tooltip("Multiplicador de velocidad según la ronda (eje X = ronda).")]
        public AnimationCurve speed;
    }

    [Header("Velocidad por tipo")]
    public KindSpeed[] speeds =
    {
        new KindSpeed { kind = EnemyKind.Bird, speed = AnimationCurve.Linear(1f, 1f, 40f, 1.8f) },
        new KindSpeed { kind = EnemyKind.Mummy, speed = AnimationCurve.Linear(1f, 1f, 40f, 1.5f) },
    };

    [Header("Límite")]
    [Tooltip("Enemigos vivos a la vez (pájaros, momias y perseguidores) desde cada ronda.")]
    public RoundValue[] maxEnemies =
    {
        new RoundValue { fromRound = 1, value = 4 },
        new RoundValue { fromRound = 11, value = 6 },
    };

    [Header("Pájaros")]
    [Tooltip("Cuántos pájaros desde cada ronda (absoluta).")]
    public RoundValue[] birdCounts =
    {
        new RoundValue { fromRound = 1, value = 1 },
        new RoundValue { fromRound = 11, value = 2 },
    };
    [Min(0f)] public float birdSpawnDelay = 1f;

    [Header("Momias")]
    [Tooltip("Segundos desde que salen los pájaros hasta la primera momia.")]
    [Min(0f)] public float mummyFirstDelay = 3f;
    [Tooltip("Segundos entre momias según la ronda (eje X = ronda).")]
    public AnimationCurve mummyInterval = AnimationCurve.Linear(1f, 10f, 40f, 5f);
    [Tooltip("Medias vueltas en los bordes antes de caer, desde cada ronda. 0 = cae sin dudar.")]
    public RoundValue[] mummyEdgeTurns =
    {
        new RoundValue { fromRound = 1, value = 1 },
        new RoundValue { fromRound = 11, value = 0 },
    };
    public SpawnPick mummySpawnPick = SpawnPick.FarthestFromJack;

    public int BirdCount(int round) => ValueFor(birdCounts, round);
    public int MaxEnemies(int round) => ValueFor(maxEnemies, round);
    public int MummyEdgeTurns(int round) => ValueFor(mummyEdgeTurns, round);
    public float MummyInterval(int round) => Mathf.Max(0.5f, mummyInterval.Evaluate(round));

    public float SpeedFor(EnemyKind kind, int round)
    {
        foreach (KindSpeed s in speeds)
            if (s.kind == kind && s.speed != null && s.speed.length > 0)
                return Mathf.Max(0.1f, s.speed.Evaluate(round));
        return 1f;
    }

    public static int ValueFor(RoundValue[] table, int round)
    {
        int value = 0;
        int bestFrom = int.MinValue;
        foreach (RoundValue row in table)
        {
            if (round < row.fromRound || row.fromRound < bestFrom) continue;
            bestFrom = row.fromRound;
            value = row.value;
        }
        return value;
    }
}