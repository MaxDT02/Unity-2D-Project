using UnityEngine;

// Crea una moneda en uno de sus puntos. El "cuándo" lo decide quien lo llame.
public class CoinSpawner : MonoBehaviour
{
    public enum Order { InOrder, Random }

    [SerializeField] private Collectible prefab;
    [SerializeField] private CoinSpawnPoint[] points;
    [SerializeField] private Order order = Order.InOrder;   // InOrder con 2 puntos = alternar

    private int _next;

    public Collectible Current { get; private set; }
    public bool HasActive => Current != null && !Current.IsCollected;

    public Collectible Spawn()
    {
        if (points == null || points.Length == 0) return null;

        int index = order == Order.Random ? Random.Range(0, points.Length) : _next++ % points.Length;
        CoinSpawnPoint point = points[index];

        Current = Instantiate(prefab, point.transform.position, Quaternion.identity);
        if (Current.TryGetComponent(out CoinMover mover)) mover.SetDirection(point.Direction);
        return Current;
    }

    public void Clear()
    {
        if (Current != null) Destroy(Current.gameObject);
        Current = null;
    }
}