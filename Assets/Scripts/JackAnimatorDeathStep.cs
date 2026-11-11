using System.Collections;
using UnityEngine;

// Muerte de Jack con el Animator: giro, caída hasta el suelo, aterrizaje y desaparición.
// El juego está pausado: la caída se mueve a mano en tiempo sin escalar, sin física.
public class JackAnimatorDeathStep : JackStep
{
    [Header("Animator (capa y estados)")]
    [SerializeField] private string layerName = "Sequences";
    [SerializeField] private string spinState = "DeathSpin";
    [Tooltip("Estado CÍCLICO (Loop Time): se repite hasta que Jack toca suelo.")]
    [SerializeField] private string fallState = "DeathFall";
    [SerializeField] private string landState = "DeathLand";

    [Header("Caída")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] private float startSpeedPixels = 0f;
    [SerializeField] private float accelerationPixels = 900f;
    [SerializeField] private float maxSpeedPixels = 300f;
    [Tooltip("Si ya hay suelo a menos de esta distancia (unidades), no cae.")]
    [SerializeField] private float groundedTolerance = 0.03f;
    [Tooltip("Seguridad: si no hay suelo, la caída termina pasado este tiempo.")]
    [SerializeField] private float maxFallSeconds = 3f;

    [Header("Final")]
    [SerializeField] private bool hideAtEnd = true;

    private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

    public override IEnumerator Run(RoundStats stats)
    {
        GameObject jack = Jack;
        Animator animator = GetAnimator();
        if (jack == null || animator == null) yield break;

        int layer = animator.GetLayerIndex(layerName);
        if (layer < 0)
        {
            Debug.LogError($"JackAnimatorDeathStep: el Animator no tiene la capa '{layerName}'.", this);
            yield break;
        }

        Collider2D col = jack.GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D { useLayerMask = true, layerMask = groundMask, useTriggers = false };

        // 1) Giro en el sitio.
        yield return animator.PlayAndWait(layer, Animator.StringToHash(spinState));

        // 2) Caída (solo si no estaba ya en el suelo).
        if (col != null && !HitsBelow(col, filter, groundedTolerance, out _))
        {
            animator.PlayState(layer, Animator.StringToHash(fallState));

            float speed = startSpeedPixels / pixelsPerUnit;
            float accel = accelerationPixels / pixelsPerUnit;
            float maxSpeed = maxSpeedPixels / pixelsPerUnit;
            float elapsed = 0f;

            while (elapsed < maxFallSeconds)
            {
                float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                elapsed += dt;
                speed = Mathf.Min(speed + accel * dt, maxSpeed);
                float distance = speed * dt;

                bool landed = HitsBelow(col, filter, distance, out float hitDistance);
                jack.transform.position += Vector3.down * (landed ? hitDistance : distance);
                Physics2D.SyncTransforms();

                if (landed) break;
                yield return null;
            }
        }

        // 3) Aterrizaje y desaparición.
        yield return animator.PlayAndWait(layer, Animator.StringToHash(landState));
        if (hideAtEnd)
            foreach (SpriteRenderer r in jack.GetComponentsInChildren<SpriteRenderer>())
                r.enabled = false;
    }

    // ¿Hay suelo a menos de "distance" por debajo? Devuelve la distancia al más cercano.
    private bool HitsBelow(Collider2D col, ContactFilter2D filter, float distance, out float hitDistance)
    {
        hitDistance = distance;
        int count = col.Cast(Vector2.down, filter, _hits, distance);
        if (count == 0) return false;

        float min = float.MaxValue;
        for (int i = 0; i < count; i++) min = Mathf.Min(min, _hits[i].distance);
        hitDistance = Mathf.Clamp(min, 0f, distance);
        return true;
    }
}