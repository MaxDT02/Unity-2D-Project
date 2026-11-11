using UnityEngine;

// Pregunta a la física "¿hay algo en esta dirección?" sin moverse. Una instancia por enemigo.
// Lanza una caja con la medida del collider del enemigo: no depende de que sea trigger.
public sealed class ContactProbe2D
{
    private readonly Collider2D _shape;
    private readonly float _gap;
    private readonly float _bottomInset;
    private readonly RaycastHit2D[] _hits;
    private ContactFilter2D _filter;

    // gap: hueco (unidades) que siempre queda con la pared al frenar.
    // bottomInset: recorta la caja por abajo (unidades). Un caminante apoyado en el suelo la necesita,
    //   o el propio suelo cuenta como pared.
    public ContactProbe2D(Collider2D shape, LayerMask mask, float gap = 0.02f, float bottomInset = 0f, int capacity = 8)
    {
        _shape = shape;
        _gap = gap;
        _bottomInset = bottomInset;
        _hits = new RaycastHit2D[capacity];
        _filter = new ContactFilter2D { useLayerMask = true, layerMask = mask, useTriggers = false };
    }

    public bool IsBlocked(Vector2 direction, float distance) => Cast(direction, distance, Vector2.zero) > 0;

    // Igual, pero con la caja desplazada "offset" (para probar un carril contiguo sin moverse).
    public bool IsBlocked(Vector2 direction, float distance, Vector2 offset) => Cast(direction, distance, offset) > 0;

    // Cuánto puede avanzar (hasta maxDistance) dejando siempre el hueco con la pared.
    public float FreeDistance(Vector2 direction, float maxDistance)
    {
        int count = Cast(direction, maxDistance + _gap, Vector2.zero);
        if (count == 0) return maxDistance;

        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
            if (_hits[i].distance < nearest) nearest = _hits[i].distance;

        return Mathf.Clamp(nearest - _gap, 0f, maxDistance);
    }

    private int Cast(Vector2 direction, float distance, Vector2 offset)
    {
        Bounds b = _shape.bounds;
        Vector2 center = (Vector2)b.center + offset;
        Vector2 size = b.size;
        if (_bottomInset > 0f)
        {
            center.y += _bottomInset * 0.5f;
            size.y = Mathf.Max(0.01f, size.y - _bottomInset);
        }
        return Physics2D.BoxCast(center, size, 0f, direction, _filter, _hits, distance);
    }
}