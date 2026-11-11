using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Collectible : MonoBehaviour
{
    [SerializeField] private LayerMask collectorLayers;

    private static readonly int CollectedHash = Animator.StringToHash("Collected");

    private Collider2D _collider;
    private bool _hasCollectedTrigger;
    private int _points;
    private int _multiplier = 1;

    protected Animator Anim { get; private set; }
    public bool IsCollected { get; private set; }
    public event Action<Collectible> Collected;

    // Cada tipo decide cuántos puntos base da en el momento de recogerlo.
    protected abstract int GetPoints();
    // Un tipo puede negarse a ser recogido en ciertos momentos (por ejemplo, el enemigo que aún no es moneda).
    protected virtual bool CanCollect => true;

    // Efecto propio del tipo (sube el multiplicador, da vida...). Se llama tras el evento Collected.
    protected virtual void OnCollected() { }

    // Qué pasa al terminar: por defecto se desactiva (las bombas), las monedas se destruyen.
    protected virtual void Finish() => gameObject.SetActive(false);

    private void Reset() => collectorLayers = LayerMask.GetMask("Player");

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
        Anim = GetComponent<Animator>();
        _hasCollectedTrigger = HasTrigger(Anim, CollectedHash);
    }

    private static bool HasTrigger(Animator animator, int hash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Trigger) return true;
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || !CanCollect) return;
        if ((collectorLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Collect();
    }

    private void Collect()
    {
        IsCollected = true;
        _collider.enabled = false;

        // Los puntos se suman en el instante del contacto.
        _points = GetPoints();
        if (ScoreSystem.Current != null)
            _multiplier = ScoreSystem.Current.Award(_points);

        OnCollected();
        Collected?.Invoke(this);

        if (_hasCollectedTrigger) Anim.SetTrigger(CollectedHash);
        else OnCollectAnimationFinished();   // sin animación: termina al instante
    }

    // Se llama desde un Animation Event al final de la explosión (o directamente si no hay animación).
    public void OnCollectAnimationFinished()
    {
        if (ScorePopupPool.Current != null)
            ScorePopupPool.Current.Show(transform.position, _points, _multiplier);

        Finish();
    }
}