using UnityEngine;

// Base de los pasos de ronda que actúan sobre el Jack actual.
public abstract class JackStep : RoundStep
{
    protected GameObject Jack
    {
        get
        {
            LevelContext level = LevelContext.Current;
            return level != null && level.PlayerSpawner != null ? level.PlayerSpawner.Current : null;
        }
    }

    protected Animator GetAnimator()
    {
        GameObject jack = Jack;
        return jack != null ? jack.GetComponentInChildren<Animator>() : null;
    }
}