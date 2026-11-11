using UnityEngine;
using UnityEngine.InputSystem;

// Solo para pruebas en el editor o en builds de desarrollo. Borra el objeto cuando termines.
//   K = matar a Jack
//   L = completar el nivel (sin bombas encendidas: sin bonus)
//   1 / 2 / 3 / 4 = completar el nivel con 20 / 21 / 22 / 23 bombas encendidas (10000 / 20000 / 30000 / 50000)
public class DebugRoundKeys : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.kKey.wasPressedThisFrame) PlayerLifecycle.ReportDeath();

        if (kb.lKey.wasPressedThisFrame) ClearWith(-1);
        if (kb.digit1Key.wasPressedThisFrame) ClearWith(20);
        if (kb.digit2Key.wasPressedThisFrame) ClearWith(21);
        if (kb.digit3Key.wasPressedThisFrame) ClearWith(22);
        if (kb.digit4Key.wasPressedThisFrame) ClearWith(23);
        if (kb.nKey.wasPressedThisFrame) GameSession.Instance.NextRound();
        if (kb.mKey.wasPressedThisFrame) GameSession.Instance.SetRound(11);
        if (kb.gKey.wasPressedThisFrame && EnemyDirector.Current != null) EnemyDirector.Current.SpawnMummy();
        if (kb.hKey.wasPressedThisFrame)
            foreach (HighScoreEntry e in HighScoreTable.Entries) Debug.Log($"{e.score}  {e.name}  ROUND {e.round}");
        if (kb.jKey.wasPressedThisFrame)
            Debug.Log($"Inserta puntuación actual: puesto {HighScoreTable.Insert(GameSession.Instance.Score, "TEST", GameSession.Instance.Round)}");
        if (kb.uKey.wasPressedThisFrame) HighScoreTable.ResetToDefaults();
        if (kb.iKey.wasPressedThisFrame) GameSession.Instance.AddScore(20000);
    }

    private static void ClearWith(int litBombs)
    {
        RoundController round = RoundController.Current;
        if (round == null) return;
        if (litBombs >= 0) round.Stats.LitBombs = litBombs;
        round.DebugForceClear();
    }
#endif
}