using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PaletteDriver : MonoBehaviour
{
    public enum Mode { Time, Indexed, Ramp }

    [Serializable]
    public class Target
    {
        public Material material;
        [Tooltip("TMP Bitmap: _Color. Shader_PaletteRamp: _PaletteColor.")]
        public string property = "_PaletteColor";

        [NonSerialized] public int id;
        [NonSerialized] public bool valid;
        [NonSerialized] public Color original;
    }

    [Serializable]
    public class Channel
    {
        [Tooltip("Nombre para Capture/Release (por ejemplo PowerupSkin).")]
        public string id;
        public Mode mode = Mode.Time;
        public PaletteCycle palette;
        [Tooltip("Solo Time: desfase en segundos. Negativo = va por detrás.")]
        public float phaseSeconds;
        [Tooltip("Solo Ramp: desfase en escalones de la rampa.")]
        public int rampOffset;
        public Target[] targets;
        public bool startDisabled;

        [NonSerialized] public bool frozen;
        [NonSerialized] public int frozenIndex;
        [NonSerialized] public bool applied;
        [NonSerialized] public Color lastColor;
        [NonSerialized] public bool disabled;
    }

    private static readonly int StepId = Shader.PropertyToID("_PaletteStep");
    private static readonly int LightId = Shader.PropertyToID("_PaletteLight");

    public static PaletteDriver Current { get; private set; }

    [Tooltip("Escalones de rampa por segundo (20 = ciclo de 8 escalones en 0,4 s).")]
    [SerializeField, Min(1f)] private float stepsPerSecond = 20f;
    [Tooltip("Como el original: los escalones alternan un intervalo corto y otro largo (1:2).")]
    [SerializeField] private bool irregularClock = true;
    [Tooltip("Cuánto se acerca al blanco el último escalón. Se envía al shader como _PaletteLight.")]
    [SerializeField, Range(0f, 1f)] private float rampLight = 0.92f;
    [Tooltip("Desactivado: las paletas se congelan con Time.timeScale = 0.")]
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private Channel[] channels;

    private readonly Dictionary<PaletteCycle, int> _liveIndex = new();
    private readonly Dictionary<string, Channel> _byId = new();
    private int _step = -1;
    private bool _dirty;

    // (paleta, nuevo índice). Lo usarán la bola P y el medidor.
    public event Action<PaletteCycle, int> IndexChanged;

    public int Step => _step;
    private float Now => useUnscaledTime ? Time.unscaledTime : Time.time;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Current = null;

    private void OnEnable()
    {
        Current = this;
        _liveIndex.Clear();
        _byId.Clear();
        _step = -1;
        _dirty = true;
        Shader.SetGlobalFloat(LightId, rampLight);
        if (channels == null) return;

        foreach (Channel channel in channels)
        {
            channel.frozen = false;
            channel.applied = false;
            channel.disabled = channel.startDisabled;

            if (!string.IsNullOrEmpty(channel.id) && !_byId.TryAdd(channel.id, channel))
                Debug.LogWarning($"PaletteDriver: el id '{channel.id}' está repetido.", this);

            if (channel.targets == null) continue;
            foreach (Target t in channel.targets)
            {
                t.id = Shader.PropertyToID(t.property);
                t.valid = t.material != null && t.material.HasProperty(t.id);

                if (t.valid) t.original = t.material.GetColor(t.id);
                else Debug.LogWarning($"PaletteDriver: el material no tiene la propiedad '{t.property}'.", this);
            }
        }

        foreach (Channel channel in channels)
            if (channel.mode == Mode.Indexed && channel.palette != null && !channel.disabled)
                Apply(channel, channel.palette.GetColor(0));
    }

    private void OnDisable()
    {
        if (Current == this) Current = null;
        Shader.SetGlobalFloat(StepId, 0f);
        if (channels == null) return;

        // En el editor los cambios a un material se quedan guardados: restauramos el color inicial.
        foreach (Channel channel in channels)
        {
            if (channel.targets == null) continue;
            foreach (Target t in channel.targets)
                if (t.valid) t.material.SetColor(t.id, t.original);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && Current == this)
            Shader.SetGlobalFloat(LightId, rampLight);
    }

    private void Update()
    {
        if (channels == null) return;
        float now = Now;

        int step = ComputeStep(now);
        bool stepChanged = step != _step;
        if (stepChanged)
        {
            _step = step;
            Shader.SetGlobalFloat(StepId, step);   // lo leen los shaders de sprites y del medidor
        }

        bool refresh = stepChanged || _dirty;
        _dirty = false;

        for (int i = 0; i < channels.Length; i++)
        {
            Channel channel = channels[i];
            PaletteCycle palette = channel.palette;
            if (palette == null) continue;
            if (channel.disabled) continue;

            if (channel.mode == Mode.Time)
            {
                if (palette.kind == PaletteCycle.Kind.Time)
                    Apply(channel, palette.EvaluateTime(now + channel.phaseSeconds));
            }
            else if (channel.mode == Mode.Ramp && refresh)
            {
                Apply(channel, palette.EvaluateRamp(IndexFor(channel), _step + channel.rampOffset, rampLight));
            }
        }
    }

    private int ComputeStep(float now)
    {
        if (irregularClock)
        {
            float pairs = now * stepsPerSecond * 0.5f;      // un par = un escalón corto + uno largo
            int pair = Mathf.FloorToInt(pairs);
            return (pair * 2 + (pairs - pair >= 1f / 3f ? 1 : 0)) % PaletteCycle.RampSteps;
        }
        return Mathf.FloorToInt(now * stepsPerSecond) % PaletteCycle.RampSteps;
    }

    public int GetIndex(PaletteCycle palette) =>
        palette != null && _liveIndex.TryGetValue(palette, out int index) ? index : 0;

    public void SetIndex(PaletteCycle palette, int index)
    {
        if (palette == null || palette.Count == 0 || channels == null) return;

        int n = palette.Count;
        index = ((index % n) + n) % n;
        if (_liveIndex.TryGetValue(palette, out int current) && current == index) return;

        _liveIndex[palette] = index;
        _dirty = true;
        foreach (Channel channel in channels)
            if (channel.palette == palette && channel.mode == Mode.Indexed && !channel.frozen && !channel.disabled)
                Apply(channel, palette.GetColor(index));

        IndexChanged?.Invoke(palette, index);
    }

    public void Advance(PaletteCycle palette, int steps = 1) => SetIndex(palette, GetIndex(palette) + steps);

    // Congela el canal en el color vivo de este instante (el personaje "se queda" con el color de la P).
    public void Capture(string id)
    {
        if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out Channel channel) || channel.palette == null) return;

        channel.frozen = true;
        channel.frozenIndex = GetIndex(channel.palette);
        _dirty = true;
        if (channel.mode == Mode.Indexed)
            Apply(channel, channel.palette.GetColor(channel.frozenIndex));
    }

    public void Release(string id)
    {
        if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out Channel channel) || channel.palette == null) return;

        channel.frozen = false;
        _dirty = true;
        if (channel.mode == Mode.Indexed)
            Apply(channel, channel.palette.GetColor(GetIndex(channel.palette)));
    }

    public void SetChannelActive(string id, bool active)
    {
        if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out Channel channel)) return;
        channel.disabled = !active;
        if (!active) return;
        channel.applied = false;
        _dirty = true;
        if (channel.mode == Mode.Indexed && channel.palette != null)
            Apply(channel, channel.palette.GetColor(IndexFor(channel)));
    }

    private int IndexFor(Channel channel) => channel.frozen ? channel.frozenIndex : GetIndex(channel.palette);

    private static void Apply(Channel channel, Color color)
    {
        if (channel.applied && channel.lastColor == color) return;
        channel.applied = true;
        channel.lastColor = color;

        if (channel.targets == null) return;
        foreach (Target t in channel.targets)
            if (t.valid) t.material.SetColor(t.id, color);
    }
}