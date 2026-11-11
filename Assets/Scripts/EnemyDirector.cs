using System.Collections;
using UnityEngine;

// Decide qué enemigos salen y cuándo: pájaros al empezar y momias cada cierto tiempo.
public class EnemyDirector : MonoBehaviour
{
    public static EnemyDirector Current { get; private set; }

    [SerializeField] private EnemyDifficulty difficulty;
    [SerializeField] private Enemy birdPrefab;
    [SerializeField] private Enemy mummyPrefab;

    private PlayerSpawner _player;
    private Coroutine _routine;

    private void OnEnable()
    {
        Current = this;
        EnemyDifficulty.Active = difficulty;

        LevelContext level = LevelContext.Current;
        _player = level != null ? level.PlayerSpawner : null;
        if (_player != null) _player.Spawned += OnPlayerSpawned;
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        if (EnemyDifficulty.Active == difficulty) EnemyDifficulty.Active = null;
        if (_player != null) _player.Spawned -= OnPlayerSpawned;
        _player = null;
    }

    // Cada vez que aparece Jack (inicio y reapariciones) se retira todo y se empieza de cero.
    private void OnPlayerSpawned(GameObject jack)
    {
        Enemy.DestroyAll();
        SparkEffect.DestroyAll();
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(difficulty.birdSpawnDelay);   // tiempo de juego: respeta la pausa

        LevelContext level = LevelContext.Current;
        EnemySpawner spawner = level != null ? level.Enemies : null;
        if (spawner == null)
        {
            Debug.LogError("EnemyDirector: el LevelContext no tiene EnemySpawner.", this);
            yield break;
        }

        int count = difficulty.BirdCount(GameSession.Instance.Round);
        for (int i = 0; i < count; i++) spawner.Spawn(birdPrefab, EnemySpawnPoint.PointKind.Corner);

        if (mummyPrefab == null) yield break;
        yield return new WaitForSeconds(difficulty.mummyFirstDelay);

        while (true)
        {
            int round = GameSession.Instance.Round;
            if (Enemy.All.Count + SparkEffect.ActiveCount < difficulty.MaxEnemies(round)) SpawnMummy();
            yield return new WaitForSeconds(difficulty.MummyInterval(round));
        }
    }

    public void SpawnMummy()
    {
        LevelContext level = LevelContext.Current;
        EnemySpawner spawner = level != null ? level.Enemies : null;
        if (spawner == null || mummyPrefab == null) return;
        spawner.SpawnWithSpark(mummyPrefab, EnemySpawnPoint.PointKind.Spark, difficulty.mummySpawnPick);
    }
}