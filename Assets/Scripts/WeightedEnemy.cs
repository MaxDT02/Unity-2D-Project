using System;
using UnityEngine;

// Una entrada de una tabla de enemigos con peso (más peso = más probable).
[Serializable]
public class WeightedEnemy
{
    public Enemy prefab;
    [Min(0f)] public float weight = 1f;

    public static Enemy Pick(WeightedEnemy[] table)
    {
        if (table == null) return null;

        float total = 0f;
        foreach (WeightedEnemy e in table)
            if (e != null && e.prefab != null) total += e.weight;
        if (total <= 0f) return null;

        float roll = UnityEngine.Random.value * total;
        foreach (WeightedEnemy e in table)
        {
            if (e == null || e.prefab == null) continue;
            roll -= e.weight;
            if (roll <= 0f) return e.prefab;
        }
        return null;
    }
}