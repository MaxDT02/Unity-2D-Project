using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SkinSystem : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Material _default;

    public bool IsOverridden => _renderer.sharedMaterial != _default;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _default = _renderer.sharedMaterial;
    }

    public void Apply(Material material)
    {
        if (material != null) _renderer.sharedMaterial = material;
    }

    public void Restore() => _renderer.sharedMaterial = _default;
}