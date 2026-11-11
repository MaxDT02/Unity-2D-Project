using UnityEngine;

public class BonusCoin : Coin
{
    [SerializeField] private ScoreConfig scoreConfig;

    protected override int GetPoints() => scoreConfig.bonusCoin;

    // Se llama después de sumar los puntos con el multiplicador anterior.
    protected override void OnCollected()
    {
        if (ScoreSystem.Current != null) ScoreSystem.Current.IncreaseMultiplier();
    }
}