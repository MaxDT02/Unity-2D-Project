using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Pantalla de la tabla de récords. Dos modos:
//  - Entry: la puntuación de la partida entra en la tabla; el jugador escribe su nombre.
//  - View: solo muestra la tabla y vuelve al título al pasar el tiempo o al pulsar.
public class RankingScreen : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private string entryTitle = "FANTASTIC SCORE!";
    [SerializeField] private string entrySubtitle = "RECORD YOUR NAME";
    [SerializeField] private string viewTitle = "BEST PLAYERS";
    [SerializeField] private string viewSubtitle = "";

    [Header("Filas")]
    [SerializeField] private RankingRow rowPrefab;
    [SerializeField] private Transform rowsRoot;
    [Tooltip("Material con el color cambiante (el de SIDE-ONE y HI-SCORE).")]
    [SerializeField] private Material cycleMaterial;

    [Header("Entrada de nombre")]
    [SerializeField] private NameEntry nameEntry;
    [SerializeField, Min(0f)] private float afterConfirmDelay = 1.5f;

    [Header("Solo lectura")]
    [Tooltip("Vuelve al título; se activa solo en el modo View.")]
    [SerializeField] private ScreenTimer viewTimer;

    private readonly List<RankingRow> _rows = new();
    private int _entryRank = -1;
    private int _score;
    private int _round;

    private void Start()
    {
        GameSession session = GameSession.Instance;
        RankingMode mode = ScreenFlow.PendingRanking;
        _score = session.FinalScore;
        _round = session.FinalRound;

        _entryRank = mode == RankingMode.Entry ? HighScoreTable.RankFor(_score) : -1;
        bool entry = _entryRank >= 0;

        titleText.text = entry ? entryTitle : viewTitle;
        subtitleText.text = entry ? entrySubtitle : viewSubtitle;
        BuildRows();

        if (viewTimer != null) viewTimer.enabled = !entry;

        if (entry)
        {
            _rows[_entryRank].SetCycle(true, cycleMaterial);
            nameEntry.Confirmed += OnConfirmed;
            nameEntry.Begin(_rows[_entryRank].NameText);
        }
        else
        {
            if (nameEntry != null) nameEntry.End();   // modo lectura: ninguna fila lleva el efecto
        }
    }

    private void OnDestroy()
    {
        if (nameEntry != null) nameEntry.Confirmed -= OnConfirmed;
    }

    // Con un hueco para la entrada nueva: lo de debajo baja un puesto.
    private void BuildRows()
    {
        IReadOnlyList<HighScoreEntry> table = HighScoreTable.Entries;
        for (int i = 0; i < HighScoreTable.Capacity; i++)
        {
            RankingRow row = Instantiate(rowPrefab, rowsRoot);
            HighScoreEntry entry = i == _entryRank
                ? new HighScoreEntry { score = _score, name = string.Empty, round = _round }
                : table[_entryRank >= 0 && i > _entryRank ? i - 1 : i];
            row.Set(i, entry);
            _rows.Add(row);
        }
    }

    private void OnConfirmed(string playerName)
    {
        HighScoreTable.Insert(_score, playerName, _round);
        _rows[_entryRank].SetNameCycle(true, cycleMaterial);   // el nombre pasa a tener el color cambiante
        StartCoroutine(GoToView());
    }

    private IEnumerator GoToView()
    {
        yield return new WaitForSecondsRealtime(afterConfirmDelay);
        ScreenFlow.ShowRanking(RankingMode.View);
    }
}