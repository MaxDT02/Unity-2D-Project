using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MeterDisplay : MonoBehaviour
{
    private static readonly int ImageWidthId = Shader.PropertyToID("_ImageWidth");
    private static readonly int BarWidthId = Shader.PropertyToID("_BarWidth");
    private static readonly int BoxWidthId = Shader.PropertyToID("_BoxWidth");
    private static readonly int IsFullId = Shader.PropertyToID("_IsFull");
    private static readonly int CyclesId = Shader.PropertyToID("_CyclesPerSecond");
    private static readonly int TimeId = Shader.PropertyToID("_TimeSec");
    private static readonly int PowerColorId = Shader.PropertyToID("_PowerColor");
    private static readonly int ImageHeightId = Shader.PropertyToID("_ImageHeight");
    private static readonly int BoxHeightId = Shader.PropertyToID("_BoxHeight");

    [Header("Referencias")]
    [SerializeField] private BonusMeter meter;
    [SerializeField] private TMP_Text digits;

    [Header("Paletas")]
    [SerializeField] private PaletteCycle powerPalette;     // Palette_P
    [SerializeField] private PaletteCycle rainbowPalette;   // Palette_Rainbow (Kind = Time)

    [Header("Canales del PaletteDriver (dígitos)")]
    [SerializeField] private string digitsFillChannel = "MeterDigitsFill";
    [SerializeField] private string digitsFullChannel = "MeterDigitsFull";

    [Header("Geometría (píxeles de juego)")]
    [SerializeField] private float emptyWidth = 37.5f;
    [SerializeField] private float boxWidth = 22f;
    [SerializeField] private float fullWidth = 75f;
    [SerializeField] private float boxHeight = 10f;

    [Header("Dígitos")]
    [Tooltip("La x más pequeña que el 1, como el original. {0} = multiplicador.")]
    [SerializeField] private string digitsFormat = "<size=72%>x</size>{0}";

    private Material _material;
    private PaletteDriver _driver;
    private ScoreSystem _score;
    private int _digitsState = -1;   // -1 sin aplicar, 0 llenándose, 1 lleno

    private void Awake()
    {
        Image image = GetComponent<Image>();
        _material = new Material(image.material);   // instancia propia: no modifica el asset
        image.material = _material;
        if (!_material.HasProperty(BarWidthId))
            Debug.LogError("MeterDisplay: el Image no usa Mat_Meter (shader SG_Meter).", this);
    }

    private void OnEnable() => meter.ValueChanged += OnValueChanged;
    private void OnDisable() => meter.ValueChanged -= OnValueChanged;

    private void Start()
    {
        float currentWidth = ((RectTransform)transform).rect.width;
        float currentHeight = ((RectTransform)transform).rect.height;
        Debug.Log($"[MeterDisplay Check] Width: {currentWidth} | Height: {currentHeight}", this);

        // Si la jerarquía de UI altera el ancho, usamos fullWidth explícitamente
        float targetWidth = (currentWidth > 0 && Mathf.Approximately(currentWidth, fullWidth)) ? currentWidth : fullWidth;

        _material.SetFloat(ImageWidthId, targetWidth);
        _material.SetFloat(BoxWidthId, boxWidth);
        _material.SetFloat(CyclesId, rainbowPalette.cyclesPerSecond);
        _material.SetFloat(ImageHeightId, currentHeight > 0 ? currentHeight : 14f);
        _material.SetFloat(BoxHeightId, boxHeight);

        _material.SetFloat(ImageWidthId, ((RectTransform)transform).rect.width);
        _material.SetFloat(BoxWidthId, boxWidth);
        _material.SetFloat(CyclesId, rainbowPalette.cyclesPerSecond);
        RectTransform rect = (RectTransform)transform;
        _material.SetFloat(ImageHeightId, rect.rect.height);
        _material.SetFloat(BoxHeightId, boxHeight);

        _driver = PaletteDriver.Current;
        if (_driver != null)
        {
            _driver.IndexChanged += OnPaletteIndexChanged;
            SetPowerColor(_driver.GetIndex(powerPalette));
        }

        _score = ScoreSystem.Current;
        if (_score != null)
        {
            _score.MultiplierChanged += OnMultiplierChanged;
            OnMultiplierChanged(_score.Multiplier);
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (_driver != null) _driver.IndexChanged -= OnPaletteIndexChanged;
        if (_score != null) _score.MultiplierChanged -= OnMultiplierChanged;
        if (_material != null) Destroy(_material);
    }

    private void Update()
    {
        if (!meter.IsFull) _material.SetFloat(TimeId, Time.unscaledTime);   // reloj del arcoíris = el del driver
    }

    private void OnValueChanged(float _) => Refresh();
    private void OnMultiplierChanged(int multiplier) =>
        digits.text = string.Format(digitsFormat, multiplier);

    private void OnPaletteIndexChanged(PaletteCycle palette, int index)
    {
        if (palette == powerPalette) SetPowerColor(index);
    }

    private void SetPowerColor(int index) =>
        _material.SetColor(PowerColorId, powerPalette.GetColor(index));

    private void Refresh()
    {
        _material.SetFloat(BarWidthId, Mathf.Lerp(emptyWidth, fullWidth, meter.Normalized));
        _material.SetFloat(IsFullId, meter.IsFull ? 1f : 0f);

        int state = meter.IsFull ? 1 : 0;
        if (state == _digitsState || _driver == null) return;
        _digitsState = state;
        _driver.SetChannelActive(digitsFillChannel, state == 0);
        _driver.SetChannelActive(digitsFullChannel, state == 1);
    }

    [ContextMenu("Debug/Siguiente color P")]
    private void DebugNextColor() { if (_driver != null) _driver.Advance(powerPalette); }

    [ContextMenu("Debug/Color P = azul")]
    private void DebugBlue() { if (_driver != null) _driver.SetIndex(powerPalette, 0); }
}