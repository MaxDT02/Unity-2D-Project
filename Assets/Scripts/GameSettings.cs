using System;
using UnityEngine;

// Ajustes del jugador, guardados en PlayerPrefs. Cualquier sistema los lee de aquí
// (el de sonido usará MusicGain y SfxGain, de 0 a 1).
public static class GameSettings
{
    public const int MaxVolume = 10;

    private const string MusicKey = "BJ_MusicVolume";
    private const string SfxKey = "BJ_SfxVolume";
    private const string FullscreenKey = "BJ_Fullscreen";

    private static bool _loaded;
    private static int _music = 7;
    private static int _sfx = 7;
    private static bool _fullscreen;

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _loaded = false;
        Changed = null;
    }

    // Al arrancar se aplica el modo de pantalla guardado.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyOnStart()
    {
        Load();
        ApplyScreen();
    }

    public static int MusicVolume
    {
        get { Load(); return _music; }
        set
        {
            Load();
            value = Mathf.Clamp(value, 0, MaxVolume);
            if (value == _music) return;
            _music = value;
            Save();
        }
    }

    public static int SfxVolume
    {
        get { Load(); return _sfx; }
        set
        {
            Load();
            value = Mathf.Clamp(value, 0, MaxVolume);
            if (value == _sfx) return;
            _sfx = value;
            Save();
        }
    }

    public static bool Fullscreen
    {
        get { Load(); return _fullscreen; }
        set
        {
            Load();
            if (value == _fullscreen) return;
            _fullscreen = value;
            ApplyScreen();
            Save();
        }
    }

    public static float MusicGain => MusicVolume / (float)MaxVolume;
    public static float SfxGain => SfxVolume / (float)MaxVolume;

    private static void Load()
    {
        if (_loaded) return;
        _loaded = true;
        _music = Mathf.Clamp(PlayerPrefs.GetInt(MusicKey, _music), 0, MaxVolume);
        _sfx = Mathf.Clamp(PlayerPrefs.GetInt(SfxKey, _sfx), 0, MaxVolume);
        _fullscreen = PlayerPrefs.GetInt(FullscreenKey, 0) == 1; // <--- Cambiado a 0 por defecto
    }

    private static void ApplyScreen() =>
        Screen.fullScreenMode = _fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

    private static void Save()
    {
        PlayerPrefs.SetInt(MusicKey, _music);
        PlayerPrefs.SetInt(SfxKey, _sfx);
        PlayerPrefs.SetInt(FullscreenKey, _fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}