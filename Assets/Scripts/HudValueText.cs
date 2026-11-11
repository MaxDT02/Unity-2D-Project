using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class HudValueText : MonoBehaviour
{
    public enum Field { Score, HiScore, Round }

    [SerializeField] private Field field;
    [Tooltip("{0} = el valor. Para ROUND: -{0}-")]
    [SerializeField] private string format = "{0}";

    private TMP_Text _text;

    private void Awake() => _text = GetComponent<TMP_Text>();

    private void OnEnable()
    {
        GameSession session = GameSession.Instance;
        switch (field)
        {
            case Field.Score: session.ScoreChanged += Show; Show(session.Score); break;
            case Field.HiScore: session.HiScoreChanged += Show; Show(session.HiScore); break;
            case Field.Round: session.RoundChanged += Show; Show(session.Round); break;
        }
    }

    private void OnDisable()
    {
        if (!GameSession.HasInstance) return;
        GameSession session = GameSession.Instance;
        switch (field)
        {
            case Field.Score: session.ScoreChanged -= Show; break;
            case Field.HiScore: session.HiScoreChanged -= Show; break;
            case Field.Round: session.RoundChanged -= Show; break;
        }
    }

    // Cambia pocas veces por segundo, no hace falta optimizar la cadena.
    private void Show(int value) => _text.text = string.Format(format, value);
}