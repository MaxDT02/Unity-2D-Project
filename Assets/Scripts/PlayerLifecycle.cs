using System;
using UnityEngine;

public static class PlayerLifecycle
{
    public static event Action Died;
    // Jack reaparece tras una muerte (no se lanza en el arranque del nivel).
    public static event Action Respawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Died = null;
        Respawned = null;
    }

    public static void ReportDeath() => Died?.Invoke();
    public static void ReportRespawn() => Respawned?.Invoke();
}