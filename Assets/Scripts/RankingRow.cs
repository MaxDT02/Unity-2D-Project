using TMPro;
using UnityEngine;

// Una fila de la tabla: puesto, puntuación, nombre y ronda.
// Puede pasar sus textos al material con el color cambiante (el mismo del HUD) y volver al suyo.
// El efecto se mezcla con el color del texto, por eso al activarlo se pone en blanco y al quitarlo se restaura.
public class RankingRow : MonoBehaviour
{
    [SerializeField] private TMP_Text rank;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text round;

    private struct Look
    {
        public Material material;
        public Color color;
    }

    private Look _rank, _score, _name, _round;

    public TMP_Text NameText => nameText;

    private void Awake()
    {
        _rank = Capture(rank);
        _score = Capture(score);
        _name = Capture(nameText);
        _round = Capture(round);
    }

    public void Set(int index, HighScoreEntry entry)
    {
        rank.text = Ordinal(index + 1);
        score.text = entry.score.ToString();
        nameText.text = entry.name;
        round.text = $"ROUND{entry.round,2}";
    }

    // Puesto, puntuación y ronda.
    public void SetCycle(bool on, Material cycle)
    {
        Apply(rank, _rank, on, cycle);
        Apply(score, _score, on, cycle);
        Apply(round, _round, on, cycle);
    }

    public void SetNameCycle(bool on, Material cycle) => Apply(nameText, _name, on, cycle);

    private static Look Capture(TMP_Text text) => new Look { material = text.fontSharedMaterial, color = text.color };

    private static void Apply(TMP_Text text, Look original, bool on, Material cycle)
    {
        text.fontSharedMaterial = on ? cycle : original.material;
        text.color = on ? Color.white : original.color;
    }

    private static string Ordinal(int n) => n switch
    {
        1 => "1ST",
        2 => "2ND",
        3 => "3RD",
        _ => $"{n}TH",
    };
}