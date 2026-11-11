using System.Collections.Generic;
using UnityEngine;

public enum SpawnPick { Random, FarthestFromJack }

// Crea enemigos en los puntos de este nivel. El "cuándo" lo decide el EnemyDirector.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemySpawnPoint[] points;
    [Tooltip("Un punto con un enemigo a menos de esta distancia (unidades) se considera ocupado.")]
    [SerializeField, Min(0f)] private float freeRadius = 0.3f;

    [Header("Chispa y suelo")]
    [Tooltip("Efecto con el que aparecen y se transforman los enemigos. Vacío = aparecen al instante.")]
    [SerializeField] private SparkEffect sparkPrefab;
    [Tooltip("Objeto vacío colocado en la superficie del suelo del marco. Lo usan las momias para saber cuándo se transforman.")]
    [SerializeField] private Transform floorLine;

    private readonly List<EnemySpawnPoint> _candidates = new();

    public float FloorY => floorLine != null ? floorLine.position.y : float.NegativeInfinity;

    private void Reset() => points = GetComponentsInChildren<EnemySpawnPoint>();

    // Aparece al instante en un punto.
    public Enemy Spawn(Enemy prefab, EnemySpawnPoint.PointKind kind, SpawnPick pick = SpawnPick.Random)
    {
        EnemySpawnPoint point = Choose(kind, pick);
        return point == null ? null : Create(prefab, point.transform.position, point.Direction);
    }

    // Aparece con chispa en un punto.
    public bool SpawnWithSpark(Enemy prefab, EnemySpawnPoint.PointKind kind, SpawnPick pick = SpawnPick.Random)
    {
        EnemySpawnPoint point = Choose(kind, pick);
        if (point == null) return false;
        SpawnWithSpark(prefab, point.transform.position, point.Direction);
        return true;
    }

    // Aparece con chispa en cualquier posición (transformaciones).
    public void SpawnWithSpark(Enemy prefab, Vector3 position, Vector2 direction = default)
    {
        if (sparkPrefab == null)
        {
            Create(prefab, position, direction);
            return;
        }
        SparkEffect.Play(sparkPrefab, position, () => Create(prefab, position, direction));
    }

    private static Enemy Create(Enemy prefab, Vector3 position, Vector2 direction)
    {
        Enemy enemy = Instantiate(prefab, position, Quaternion.identity);
        if (enemy.TryGetComponent(out EnemyMotion motion)) motion.SetStartDirection(direction);
        return enemy;
    }

    private EnemySpawnPoint Choose(EnemySpawnPoint.PointKind kind, SpawnPick pick)
    {
        _candidates.Clear();
        if (points == null) return null;

        foreach (EnemySpawnPoint p in points)
            if (p != null && p.Kind == kind && IsFree(p.transform.position)) _candidates.Add(p);

        if (_candidates.Count == 0)      // todos ocupados: vale cualquiera del tipo
            foreach (EnemySpawnPoint p in points)
                if (p != null && p.Kind == kind) _candidates.Add(p);

        if (_candidates.Count == 0) return null;

        if (pick == SpawnPick.FarthestFromJack && JackTarget.TryGetPosition(out Vector2 jack))
        {
            EnemySpawnPoint best = _candidates[0];
            float bestDistance = -1f;
            foreach (EnemySpawnPoint p in _candidates)
            {
                float d = ((Vector2)p.transform.position - jack).sqrMagnitude;
                if (d <= bestDistance) continue;
                bestDistance = d;
                best = p;
            }
            return best;
        }

        return _candidates[Random.Range(0, _candidates.Count)];
    }

    private bool IsFree(Vector3 position)
    {
        float sqr = freeRadius * freeRadius;
        IReadOnlyList<Enemy> all = Enemy.All;
        for (int i = 0; i < all.Count; i++)
            if (((Vector2)(all[i].transform.position - position)).sqrMagnitude < sqr) return false;
        return true;
    }
}