using UnityEngine;

[RequireComponent(typeof(CoinMover))]
public abstract class Coin : Collectible
{
    protected override void Finish() => Destroy(gameObject);
}