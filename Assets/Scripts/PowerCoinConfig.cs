using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Power Coin Config")]
public class PowerCoinConfig : ScriptableObject
{
    [Tooltip("Palette_P: el mismo asset que usa el PaletteDriver.")]
    public PaletteCycle palette;

    [Tooltip("Puntos base por color de la P, y también la cadena de enemigos-moneda (mismo orden).")]
    public int[] points = { 100, 200, 300, 500, 800, 1200, 2000 };

    [Min(0.1f)] public float duration = 5f;

    [Header("Efecto en Jack")]
    public Material skinMaterial;
    public string skinChannelId = "PowerupSkin";

    public int PointsFor(int colorIndex) => points[Mathf.Clamp(colorIndex, 0, points.Length - 1)];
    public int ChainPoints(int n) => points[Mathf.Clamp(n, 0, points.Length - 1)];

    private void OnValidate()
    {
        if (palette != null && points.Length != palette.Count) Array.Resize(ref points, palette.Count);
    }
}