using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Cierre (y apertura) en iris con un círculo negro. Usa un material del shader SG_Iris.
// El radio se mide en alturas de pantalla: 0 = todo negro, openRadius = todo visible.
[RequireComponent(typeof(Image))]
public class IrisTransition : MonoBehaviour
{
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int AspectId = Shader.PropertyToID("_Aspect");
    private static readonly int GridId = Shader.PropertyToID("_Grid");

    [Tooltip("Radio con el que ya no se ve el negro. 0,8 cubre cualquier esquina.")]
    [SerializeField, Min(0.1f)] private float openRadius = 0.8f;
    [Tooltip("Empieza cerrado y se abre solo (útil al entrar en una pantalla).")]
    [SerializeField] private bool openOnStart;
    [SerializeField, Min(0.05f)] private float openSeconds = 0.6f;

    private Material _material;

    private void Awake()
    {
        Image image = GetComponent<Image>();
        image.raycastTarget = false;
        _material = new Material(image.material);   // instancia propia: no toca el asset
        image.material = _material;

        if (!_material.HasProperty(RadiusId))
            Debug.LogError("IrisTransition: el Image no usa un material con el shader SG_Iris.", this);

        Rect rect = ((RectTransform)transform).rect;
        _material.SetFloat(AspectId, rect.width / rect.height);
        _material.SetVector(GridId, new Vector4(rect.width, rect.height, 0f, 0f));
        _material.SetVector(CenterId, new Vector4(0.5f, 0.5f, 0f, 0f));
        SetRadius(openOnStart ? 0f : openRadius);
    }

    private void Start()
    {
        if (openOnStart) StartCoroutine(Open(openSeconds));
    }

    private void OnDestroy()
    {
        if (_material != null) Destroy(_material);
    }

    // center en coordenadas de la imagen (0,0 abajo-izquierda; 1,1 arriba-derecha).
    public IEnumerator Close(float seconds, Vector2 center)
    {
        _material.SetVector(CenterId, new Vector4(center.x, center.y, 0f, 0f));
        yield return Animate(openRadius, 0f, seconds);
    }

    public IEnumerator Open(float seconds) => Animate(0f, openRadius, seconds);

    private IEnumerator Animate(float from, float to, float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            SetRadius(Mathf.Lerp(from, to, elapsed / seconds));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        SetRadius(to);
    }

    private void SetRadius(float radius) => _material.SetFloat(RadiusId, radius);
}