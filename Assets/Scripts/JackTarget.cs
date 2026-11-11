using UnityEngine;

public static class JackTarget
{
    // Centro del collider de Jack. false si no hay Jack en escena.
    public static bool TryGetPosition(out Vector2 position)
    {
        position = default;

        LevelContext level = LevelContext.Current;
        if (level == null || level.PlayerSpawner == null) return false;

        GameObject jack = level.PlayerSpawner.Current;
        if (jack == null) return false;

        position = jack.TryGetComponent(out Collider2D col) ? (Vector2)col.bounds.center : (Vector2)jack.transform.position;
        return true;
    }
}