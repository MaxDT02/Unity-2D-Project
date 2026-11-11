using UnityEngine;

// Acciones de ScreenFlow para enlazar desde el Inspector (UnityEvent de ScreenTimer, opciones de menú...).
public class ScreenFlowActions : MonoBehaviour
{
    public void ShowTitle() => ScreenFlow.ShowTitle();
    public void ShowInstructions() => ScreenFlow.ShowInstructions();
    public void ShowOptions() => ScreenFlow.ShowOptions();
    public void ShowRankingView() => ScreenFlow.ShowRanking(RankingMode.View);
    public void StartGame() => ScreenFlow.StartGame();
}