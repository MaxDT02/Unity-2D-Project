using System.Collections;
using UnityEngine;

// Utilidades para lanzar estados "sueltos" (sin transiciones) en una capa del Animator.
public static class AnimatorExtensions
{
    // Pone la capa a peso 1 y lanza el estado desde el principio, saltándose las transiciones.
    public static void PlayState(this Animator animator, int layer, int stateHash)
    {
        animator.SetLayerWeight(layer, 1f);
        animator.Play(stateHash, layer, 0f);
        animator.Update(0f);   // lo aplica ya: normalizedTime vuelve a 0 en este mismo frame
    }

    // Lanza un estado NO cíclico y espera a que termine. Se queda en el último fotograma.
    public static IEnumerator PlayAndWait(this Animator animator, int layer, int stateHash)
    {
        animator.PlayState(layer, stateHash);
        while (true)
        {
            yield return null;
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layer);
            // Si el estado no existe, Unity avisa en consola y aquí salimos sin colgar nada.
            if (info.shortNameHash != stateHash || info.normalizedTime >= 1f) yield break;
        }
    }

    // Devuelve el control a las capas base.
    public static void ReleaseLayer(this Animator animator, int layer) => animator.SetLayerWeight(layer, 0f);
}