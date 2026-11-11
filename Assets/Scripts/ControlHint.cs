using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Text))]
public class ControlHint : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [SerializeField] private string label = "JUMP";

    [Header("UI References")]
    [Tooltip("Texto secundario a la derecha para mostrar las teclas alineadas.")]
    [SerializeField] private TMP_Text valueText;

    [Tooltip("Si no está vacío, muestra este texto en lugar de los controles leídos.")]
    [SerializeField] private string overrideControls;

    private TMP_Text _labelText;

    private void Awake() => _labelText = GetComponent<TMP_Text>();

    private void OnEnable()
    {
        string controls = overrideControls;
        if (string.IsNullOrEmpty(controls) && action != null && action.action != null)
        {
            controls = action.action
                .GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions)
                .ToUpperInvariant();
        }

        if (_labelText != null) _labelText.text = label;

        if (valueText != null)
        {
            valueText.text = controls;
        }
        else if (_labelText != null)
        {
            // Fallback si mantienes un solo texto
            _labelText.text = $"{label}   {controls}";
        }
    }
}