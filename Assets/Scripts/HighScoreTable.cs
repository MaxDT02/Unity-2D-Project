using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HighScoreEntry
{
    public int score;
    public string name;
    public int round;
}

// Tabla de los 10 mejores. Se guarda sola en PlayerPrefs (JSON) cada vez que entra alguien.
public static class HighScoreTable
{
    public const int Capacity = 10;
    public const int MaxNameLength = 8;

    private const string Key = "BombJack.HighScores";
    private const int DefaultScore = 500;
    private static readonly string[] DefaultNames = { "J.J", "T.T", "E.E", "H.H", "K.K", "A.A", "N.N", "L.L", "T.T", "D.D" };

    [Serializable]
    private class Data
    {
        public HighScoreEntry[] entries;
    }

    private static HighScoreEntry[] _entries;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _entries = null;

    public static IReadOnlyList<HighScoreEntry> Entries => Get();
    public static int Top => Get()[0].score;

    // Puesto (0 = 1.º) que ocuparía esta puntuación, o -1 si no entra. Empatar no adelanta al que ya está.
    public static int RankFor(int score)
    {
        HighScoreEntry[] entries = Get();
        for (int i = 0; i < entries.Length; i++)
            if (score > entries[i].score) return i;
        return -1;
    }

    // Mete la puntuación en su puesto, desplaza al resto y guarda. Devuelve el puesto, o -1 si no entra.
    public static int Insert(int score, string name, int round)
    {
        int rank = RankFor(score);
        if (rank < 0) return -1;

        HighScoreEntry[] entries = Get();
        for (int i = entries.Length - 1; i > rank; i--) entries[i] = entries[i - 1];
        entries[rank] = new HighScoreEntry { score = score, name = Clean(name), round = round };

        Save();
        return rank;
    }

    public static string Clean(string name)
    {
        name = (name ?? string.Empty).Trim().ToUpperInvariant();
        if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength);
        return name.Length == 0 ? "???" : name;
    }

    public static void ResetToDefaults()
    {
        _entries = CreateDefaults();
        Save();
    }

    private static HighScoreEntry[] Get()
    {
        if (_entries != null) return _entries;

        string json = PlayerPrefs.GetString(Key, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            Data data = JsonUtility.FromJson<Data>(json);
            if (data != null && data.entries != null && data.entries.Length == Capacity)
            {
                _entries = data.entries;
                return _entries;
            }
        }

        _entries = CreateDefaults();
        return _entries;
    }

    private static HighScoreEntry[] CreateDefaults()
    {
        HighScoreEntry[] entries = new HighScoreEntry[Capacity];
        for (int i = 0; i < Capacity; i++)
            entries[i] = new HighScoreEntry { score = DefaultScore, name = DefaultNames[i % DefaultNames.Length], round = 1 };
        return entries;
    }

    private static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new Data { entries = _entries }));
        PlayerPrefs.Save();
    }
}