using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Menú vertical con recuadro de neón. Arriba y abajo cambian de opción; aceptar ejecuta "selected";
// izquierda y derecha ejecutan "adjusted" (-1 / +1); cancelar ejecuta "cancelled".
// La opción activa: texto blanco y recuadro del color de su casilla. Las demás: texto gris apagado.
public class MenuController : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        [Tooltip("Rectángulo del texto de la opción (debe tener un TMP_Text).")]
        public RectTransform target;
        [Tooltip("Opcional: zona que debe rodear el recuadro (por ejemplo, la fila entera con su valor). Vacío = solo el texto.")]
        public RectTransform frameArea;
        [Tooltip("Color del recuadro cuando esta opción está activa.")]
        public Color frameColor = Color.cyan;
        [Tooltip("Opcional: objeto (por ejemplo un texto '>') que solo se muestra cuando esta opción está activa.")]
        public GameObject marker;
        public UnityEvent selected;
        [Tooltip("Izquierda y derecha: -1 / +1 (volúmenes, interruptores...).")]
        public UnityEvent<int> adjusted;

        [NonSerialized] public TMP_Text label;
    }

    [SerializeField] private Item[] items;
    [SerializeField] private bool wrap = true;
    [SerializeField, Min(0)] private int startIndex;
    [Tooltip("Ignora las pulsaciones al abrir la pantalla (por la pulsación que acaba de traer aquí).")]
    [SerializeField, Min(0f)] private float ignoreInputSeconds = 0.3f;

    [Header("Aspecto")]
    [Tooltip("Dibuja el recuadro de neón. Desactívalo en pantallas que usan solo marcadores '>'.")]
    [SerializeField] private bool showFrame = true;
    [SerializeField] private Color activeTextColor = Color.white;
    [SerializeField] private Color inactiveTextColor = new Color(0.65f, 0.65f, 0.65f, 0.5f);
    [Tooltip("Margen entre el texto y el borde del recuadro (unidades del canvas).")]
    [SerializeField] private Vector2 padding = new Vector2(4f, 2f);
    [SerializeField, Range(0f, 1f)] private float fillAlpha = 0.2f;
    [SerializeField, Range(0f, 1f)] private float glowAlpha = 0.35f;
    [Tooltip("Grosor del borde en píxeles.")]
    [SerializeField, Min(1)] private int borderThickness = 2;
    [Tooltip("Radio de las esquinas en píxeles. Más alto = más cápsula.")]
    [SerializeField, Min(0)] private int cornerRadius = 4;

    [Header("Eventos")]
    [Tooltip("Se lanza al cambiar de opción o de valor: reinicia el modo reposo, suena un efecto...")]
    [SerializeField] private UnityEvent moved;
    [Tooltip("Botón de cancelar (Esc, B del mando). Por ejemplo, volver al título.")]
    [SerializeField] private UnityEvent cancelled;

    private UiInputMap _input;
    private int _index;
    private float _enabledAt;
    private bool _dirty;

    private RectTransform _frame;
    private Image _fill, _border, _glow;
    private Sprite _ringSprite, _solidSprite;

    private void Awake()
    {
        if (items == null) return;
        foreach (Item item in items)
            if (item.target != null) item.label = item.target.GetComponent<TMP_Text>();
        if (showFrame) BuildFrame();
    }

    private void OnEnable()
    {
        _input ??= new UiInputMap();
        _input.Enable();
        _enabledAt = Time.unscaledTime;
        Select(startIndex, false);
    }

    private void OnDisable() => _input?.Disable();

    private void OnDestroy()
    {
        _input?.Dispose();
        _input = null;
        if (_ringSprite != null) Destroy(_ringSprite.texture);
        if (_solidSprite != null) Destroy(_solidSprite.texture);
    }

    private void Update()
    {
        if (items == null || items.Length == 0) return;
        if (Time.unscaledTime - _enabledAt < ignoreInputSeconds) return;

        if (_input.Up.WasPressedThisFrame()) Move(-1);
        else if (_input.Down.WasPressedThisFrame()) Move(1);
        else if (_input.Left.WasPressedThisFrame()) Adjust(-1);
        else if (_input.Right.WasPressedThisFrame()) Adjust(1);
        else if (_input.Submit.WasPressedThisFrame()) items[_index].selected.Invoke();
        else if (_input.Cancel.WasPressedThisFrame()) cancelled.Invoke();
    }

    // El recuadro se coloca al final del fotograma, cuando los layouts y los textos ya están calculados.
    private void LateUpdate()
    {
        if (!_dirty) return;
        _dirty = false;
        PlaceFrame();
    }

    private void Move(int delta)
    {
        int count = items.Length;
        int next = _index + delta;
        next = wrap ? (next % count + count) % count : Mathf.Clamp(next, 0, count - 1);
        if (next != _index) Select(next, true);
    }

    private void Adjust(int direction)
    {
        items[_index].adjusted.Invoke(direction);
        _dirty = true;   // el valor puede cambiar el ancho del texto
        moved.Invoke();
    }

    private void Select(int index, bool notify)
    {
        if (items == null || items.Length == 0) return;
        _index = Mathf.Clamp(index, 0, items.Length - 1);

        for (int i = 0; i < items.Length; i++)
        {
            Color targetColor = (i == _index) ? activeTextColor : inactiveTextColor;

            // 1. Cambia el color de la etiqueta principal
            if (items[i].label != null)
                items[i].label.color = targetColor;

            // 2. Cambia el color de todos los textos secundarios (Values) dentro de la fila
            if (items[i].target != null)
            {
                TMP_Text[] allTexts = items[i].target.GetComponentsInChildren<TMP_Text>();
                foreach (TMP_Text txt in allTexts)
                {
                    // Evitamos cambiar el color del Marker si quieres que mantenga su color neón propio
                    if (items[i].marker != null && txt.gameObject == items[i].marker)
                        continue;

                    txt.color = targetColor;
                }
            }

            // 3. Enciende/apaga el marcador '>'
            if (items[i].marker != null)
                items[i].marker.SetActive(i == _index);
        }

        _dirty = true;
        PlaceFrame();
        if (notify) moved.Invoke();
    }

    // --- Recuadro ---------------------------------------------------------------------------

    private void PlaceFrame()
    {
        Item item = items[_index];
        if (item.target == null || _frame == null) return;

        RectTransform parent = item.target.parent as RectTransform;
        if (parent == null) return;

        Canvas.ForceUpdateCanvases();

        Vector3 a, b;
        if (item.frameArea != null)
        {
            Vector3[] corners = new Vector3[4];
            item.frameArea.GetWorldCorners(corners);
            a = corners[0];
            b = corners[2];
        }
        else if (item.label != null)
        {
            item.label.ForceMeshUpdate();
            Bounds tb = item.label.textBounds;
            a = item.label.transform.TransformPoint(tb.min);
            b = item.label.transform.TransformPoint(tb.max);
        }
        else return;

        a = parent.InverseTransformPoint(a);
        b = parent.InverseTransformPoint(b);
        Vector2 min = Vector2.Min(a, b), max = Vector2.Max(a, b);

        if (_frame.parent != parent) _frame.SetParent(parent, false);
        _frame.SetAsFirstSibling();   // detrás de los textos
        _frame.localScale = Vector3.one;
        _frame.localPosition = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f);
        _frame.sizeDelta = (max - min) + padding * 2f;

        Color c = item.frameColor;
        _border.color = new Color(c.r, c.g, c.b, 1f);
        _glow.color = new Color(c.r, c.g, c.b, glowAlpha);
        _fill.color = new Color(c.r, c.g, c.b, fillAlpha);
        _frame.gameObject.SetActive(true);
    }

    private void BuildFrame()
    {
        BuildSprites();

        GameObject root = new("MenuHighlight", typeof(RectTransform), typeof(LayoutElement));
        root.GetComponent<LayoutElement>().ignoreLayout = true;
        _frame = (RectTransform)root.transform;
        _frame.anchorMin = _frame.anchorMax = _frame.pivot = new Vector2(0.5f, 0.5f);

        _glow = CreateImage("Glow", _ringSprite, 1f);     // aro exterior tenue (brillo)
        _fill = CreateImage("Fill", _solidSprite, 0f);    // cristal de color
        _border = CreateImage("Border", _ringSprite, 0f); // borde
        root.SetActive(false);
    }

    private Image CreateImage(string name, Sprite sprite, float grow)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(_frame, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-grow, -grow);
        rt.offsetMax = new Vector2(grow, grow);

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    // Sprites con esquinas redondeadas generados por código (sin assets), en 9 porciones:
    // el recuadro se estira a cualquier tamaño sin deformar el borde ni las esquinas.
    private void BuildSprites()
    {
        int slice = Mathf.Max(cornerRadius, borderThickness) + 1;
        int size = slice * 2 + 1;
        _ringSprite = MakeSprite(size, slice, true);
        _solidSprite = MakeSprite(size, slice, false);
    }

    private Sprite MakeSprite(int size, int slice, bool ring)
    {
        Texture2D tex = new(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        float half = size * 0.5f;
        float r = Mathf.Min(cornerRadius, half);
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float qx = Mathf.Abs(x + 0.5f - half) - (half - r);
                float qy = Mathf.Abs(y + 0.5f - half) - (half - r);
                float d = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude
                          + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;   // < 0 = dentro
                bool on = d <= 0f && (!ring || d > -borderThickness);
                pixels[y * size + x] = on ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(slice, slice, slice, slice));
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}