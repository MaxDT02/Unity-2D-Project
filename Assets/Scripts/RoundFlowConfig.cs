using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Round Flow Config")]
public class RoundFlowConfig : ScriptableObject
{
    [Serializable]
    public struct BonusTier
    {
        public int minLitBombs;
        public int points;
    }

    [Header("Bonus de fin de ronda")]
    public BonusTier[] bonusTiers =
    {
        new BonusTier { minLitBombs = 20, points = 10000 },
        new BonusTier { minLitBombs = 21, points = 20000 },
        new BonusTier { minLitBombs = 22, points = 30000 },
        new BonusTier { minLitBombs = 23, points = 50000 },
    };

    [Header("Tiempos (segundos, sin escalar)")]
    [Min(0f)] public float clearHoldDelay = 0.6f;       // tras la última bomba, para ver su explosión y popup
    [Min(0f)] public float bonusPreDelay = 1.5f;        // bonus: espera antes de la cuenta atrás
    [Min(1f)] public float countdownPointsPerSecond = 20000f;   // 50000 = 2,5 s
    [Min(0f)] public float bonusPostDelay = 1.5f;       // bonus: espera tras la cuenta atrás
    [Min(0f)] public float respawnDelay = 1.5f;         // tras desaparecer Jack, antes de reaparecer o acabar

    public int BonusFor(int litBombs)
    {
        int points = 0;
        int bestMin = -1;
        foreach (BonusTier tier in bonusTiers)
        {
            if (litBombs < tier.minLitBombs || tier.minLitBombs <= bestMin) continue;
            bestMin = tier.minLitBombs;
            points = tier.points;
        }
        return points;
    }
}