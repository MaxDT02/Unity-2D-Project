using System;
using UnityEngine;

// Pausa del juego con contador: varios sistemas pueden pausar y solo se reanuda al soltar todos.
public static class GameClock
{
    private static int _holds;

    public static bool IsPaused => _holds > 0;
    public static event Action<bool> PausedChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _holds = 0;
        PausedChanged = null;
        Time.timeScale = 1f;
    }

    public static void Pause()
    {
        if (_holds++ > 0) return;
        Time.timeScale = 0f;
        PausedChanged?.Invoke(true);
    }

    public static void Resume()
    {
        if (_holds == 0) return;
        if (--_holds > 0) return;
        Time.timeScale = 1f;
        PausedChanged?.Invoke(false);
    }

    // Antes de cargar una escena: suelta todas las pausas.
    public static void Reset()
    {
        bool wasPaused = _holds > 0;
        _holds = 0;
        Time.timeScale = 1f;
        if (wasPaused) PausedChanged?.Invoke(false);
    }
}