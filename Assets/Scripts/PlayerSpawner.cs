using System;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    public GameObject Current { get; private set; }

    // Se lanza cada vez que aparece un Jack nuevo (inicio del nivel y reapariciones).
    public event Action<GameObject> Spawned;

    private void Start() => Spawn();

    public GameObject Spawn()
    {
        if (Current != null) Destroy(Current);
        Current = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        Spawned?.Invoke(Current);
        return Current;
    }
}