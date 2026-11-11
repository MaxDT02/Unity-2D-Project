using UnityEngine;
using UnityEngine.UI;

// Marco de cuatro tramos alrededor del nombre. Las esquinas van incluidas en los tramos de arriba y abajo,
// que deben tener bordes de 9 porciones (izquierdo y derecho = ancho de la esquina).
// Medidas en píxeles de juego (1 píxel del sprite = 1 unidad del canvas).
public class NameFrame : MonoBehaviour
{
    [SerializeField] private Image top;
    [SerializeField] private Image bottom;
    [SerializeField] private Image left;
    [SerializeField] private Image right;

    [Tooltip("Ancho de un carácter de la fuente.")]
    [SerializeField, Min(1f)] private float charWidth = 8f;
    [Tooltip("Alto del hueco interior.")]
    [SerializeField, Min(1f)] private float innerHeight = 10f;
    [Tooltip("Margen entre el marco y las letras, a cada lado.")]
    [SerializeField, Min(0f)] private float padding = 1f;

    private RectTransform _rect;

    private void Init()
    {
        if (_rect != null) return;
        _rect = (RectTransform)transform;
        foreach (Image image in new[] { top, bottom, left, right })
        {
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
        }
    }

    // Cuelga el marco de un texto, alineado a su izquierda y centrado en vertical.
    public void AttachTo(RectTransform target)
    {
        Init();
        _rect.SetParent(target, false);
        _rect.anchorMin = _rect.anchorMax = new Vector2(0f, 0.5f);
        _rect.pivot = new Vector2(0f, 0.5f);
        _rect.anchoredPosition = new Vector2(-padding, 0f);
        _rect.sizeDelta = Vector2.zero;
        _rect.localScale = Vector3.one;
    }

    // Ajusta el ancho para ese número de caracteres.
    public void Fit(int slots)
    {
        gameObject.SetActive(true);
        Init();

        float width = Mathf.Max(0, slots) * charWidth + padding * 2f;
        float height = innerHeight;
        float leftW = left.sprite.rect.width;
        float rightW = right.sprite.rect.width;
        float topH = top.sprite.rect.height;
        float bottomH = bottom.sprite.rect.height;
        float full = width + leftW + rightW;

        Place(left, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(leftW, height));
        Place(right, new Vector2(0f, 0.5f), new Vector2(width, 0f), new Vector2(rightW, height));
        Place(top, new Vector2(0f, 0f), new Vector2(-leftW, height * 0.5f), new Vector2(full, topH));
        Place(bottom, new Vector2(0f, 1f), new Vector2(-leftW, -height * 0.5f), new Vector2(full, bottomH));
    }

    public void Hide() => gameObject.SetActive(false);

    private static void Place(Image image, Vector2 pivot, Vector2 position, Vector2 size)
    {
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}