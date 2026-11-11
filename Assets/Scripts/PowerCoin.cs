using UnityEngine;

public class PowerCoin : Coin
{
    [SerializeField] private PowerCoinConfig config;

    // Los puntos dependen del color vivo en el instante de recogerla.
    protected override int GetPoints()
    {
        PaletteDriver driver = PaletteDriver.Current;
        int index = driver != null ? driver.GetIndex(config.palette) : 0;
        return config.PointsFor(index);
    }
}