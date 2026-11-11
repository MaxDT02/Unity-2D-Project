using System;
using System.Collections.Generic;
using UnityEngine;

public class ScorePopupPool : MonoBehaviour
{
    [Serializable]
    private struct Entry
    {
        public int number;
        public Sprite sprite;
    }

    public static ScorePopupPool Current { get; private set; }

    [SerializeField] private ScorePopup popupPrefab;
    [SerializeField] private int initialSize = 8;
    [SerializeField] private Entry[] values;
    [SerializeField] private Entry[] multipliers;

    private readonly List<ScorePopup> _popups = new();
    private readonly Dictionary<int, Sprite> _valueSprites = new();
    private readonly Dictionary<int, Sprite> _multiplierSprites = new();

    private void Awake()
    {
        foreach (Entry e in values) _valueSprites[e.number] = e.sprite;
        foreach (Entry e in multipliers) _multiplierSprites[e.number] = e.sprite;

        for (int i = 0; i < initialSize; i++) Create();
    }

    private void OnEnable() => Current = this;

    private void OnDisable()
    {
        if (Current == this) Current = null;
    }

    public void Show(Vector3 position, int points, int multiplier)
    {
        if (!_valueSprites.TryGetValue(points, out Sprite valueSprite) || valueSprite == null)
        {
            Debug.LogWarning($"ScorePopupPool: no hay sprite para el valor {points}.");
            return;
        }

        Sprite multiplierSprite = null;
        if (multiplier > 1) _multiplierSprites.TryGetValue(multiplier, out multiplierSprite);

        GetFree().Show(position, valueSprite, multiplierSprite);
    }

    private ScorePopup GetFree()
    {
        for (int i = 0; i < _popups.Count; i++)
            if (!_popups[i].gameObject.activeSelf) return _popups[i];
        return Create();
    }

    private ScorePopup Create()
    {
        ScorePopup popup = Instantiate(popupPrefab, transform);
        popup.gameObject.SetActive(false);
        _popups.Add(popup);
        return popup;
    }
}