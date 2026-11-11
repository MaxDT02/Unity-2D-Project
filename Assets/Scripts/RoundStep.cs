using System.Collections;
using UnityEngine;

// Los pasos se ejecutan en orden con el juego pausado: usa tiempo sin escalar (WaitForSecondsRealtime).
public abstract class RoundStep : MonoBehaviour
{
    // false = se salta este paso.
    public virtual bool ShouldRun(RoundStats stats) => true;

    public abstract IEnumerator Run(RoundStats stats);
}