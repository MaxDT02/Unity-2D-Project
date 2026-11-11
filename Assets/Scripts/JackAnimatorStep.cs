using System.Collections;
using UnityEngine;

// Lanza un estado de la capa "Sequences" de Jack y espera a que acabe. Jack se queda donde está.
public class JackAnimatorStep : JackStep
{
    [SerializeField] private string layerName = "Sequences";
    [SerializeField] private string stateName = "Clear";

    public override IEnumerator Run(RoundStats stats)
    {
        Animator animator = GetAnimator();
        if (animator == null) yield break;

        int layer = animator.GetLayerIndex(layerName);
        if (layer < 0)
        {
            Debug.LogError($"JackAnimatorStep: el Animator no tiene la capa '{layerName}'.", this);
            yield break;
        }

        yield return animator.PlayAndWait(layer, Animator.StringToHash(stateName));
    }
}