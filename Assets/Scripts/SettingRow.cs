using TMPro;
using UnityEngine;

public class SettingRow : MonoBehaviour
{
    public enum Setting { Music, Sfx, Fullscreen }

    [SerializeField] private Setting setting;

    [Header("Referencias de UI")]
    [Tooltip("Texto para la etiqueta (ej: MUSIC). Si es el propio GameObject, puedes dejarlo vacío.")]
    [SerializeField] private TMP_Text labelText;
    [Tooltip("Texto secundario a la derecha donde se mostrará el valor (ej: 6, FULL, WINDOW).")]
    [SerializeField] private TMP_Text valueText;

    [Header("Formatos")]
    [SerializeField] private string labelName = "MUSIC";
    [SerializeField] private string onText = "FULL";
    [SerializeField] private string offText = "WINDOW";

    private void Awake()
    {
        if (labelText == null) labelText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        GameSettings.Changed += Refresh;
        Refresh();
    }

    private void OnDisable() => GameSettings.Changed -= Refresh;

    public void Adjust(int direction)
    {
        switch (setting)
        {
            case Setting.Music: GameSettings.MusicVolume += direction; break;
            case Setting.Sfx: GameSettings.SfxVolume += direction; break;
            case Setting.Fullscreen: GameSettings.Fullscreen = !GameSettings.Fullscreen; break;
        }
    }

    private void Refresh()
    {
        string value = setting switch
        {
            Setting.Music => GameSettings.MusicVolume.ToString(),
            Setting.Sfx => GameSettings.SfxVolume.ToString(),
            _ => GameSettings.Fullscreen ? onText : offText,
        };

        // Si asignaste un objeto separado para el valor:
        if (valueText != null)
        {
            if (labelText != null) labelText.text = labelName;
            valueText.text = value;
        }
        else if (labelText != null)
        {
            // Si prefieres mantener un solo texto, usamos una tabulación/formato
            labelText.text = $"{labelName}  {value}";
        }
    }
}