using System;
using System.Collections.Generic;
using UnityEngine;

public class BombSequence : MonoBehaviour
{
    public event Action AllCollected;
    public event Action<Bomb, bool> BombCollected;   // bomba y si estaba encendida

    private readonly List<Bomb> _bombs = new();
    private int _remaining;
    private int _litIndex = -1;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).TryGetComponent(out Bomb bomb)) continue;
            _bombs.Add(bomb);
            bomb.Collected += OnBombCollected;
        }
        _remaining = _bombs.Count;
    }

    private void OnDestroy()
    {
        foreach (Bomb bomb in _bombs)
            if (bomb != null) bomb.Collected -= OnBombCollected;
    }

    private void OnBombCollected(Collectible collectible)
    {
        Bomb bomb = (Bomb)collectible;
        bool wasLit = bomb.IsLit;              // antes de encender la siguiente
        int index = _bombs.IndexOf(bomb);
        _remaining--;
        BombCollected?.Invoke(bomb, wasLit);

        // Sin ninguna encendida (primera del nivel) o recogida la encendida: se enciende la siguiente.
        // Una apagada recogida mientras hay otra encendida no cambia nada.
        if (_litIndex < 0 || index == _litIndex)
            LightNextAfter(index);

        if (_remaining <= 0) AllCollected?.Invoke();
    }

    // Busca la siguiente bomba pendiente en el orden, dando la vuelta al llegar al final.
    private void LightNextAfter(int index)
    {
        _litIndex = -1;
        for (int step = 1; step < _bombs.Count; step++)
        {
            int candidate = (index + step) % _bombs.Count;
            if (_bombs[candidate].IsCollected) continue;

            _litIndex = candidate;
            _bombs[candidate].Light();
            return;
        }
    }

#if UNITY_EDITOR
    // Dibuja el orden de encendido en la Scene view.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            UnityEditor.Handles.Label(t.position + Vector3.up * 0.4f, (i + 1).ToString());
            if (i > 0) Gizmos.DrawLine(transform.GetChild(i - 1).position, t.position);
        }
    }
#endif
}