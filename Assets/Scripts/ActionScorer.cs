using UnityEngine;

// 10 puntos x multiplicador por cada acción de Jack.
public class ActionScorer : MonoBehaviour
{
    [SerializeField] private ScoreConfig scoreConfig;

    private void OnEnable() => JackActions.Performed += OnAction;
    private void OnDisable() => JackActions.Performed -= OnAction;

    private void OnAction()
    {
        if (ScoreSystem.Current != null) ScoreSystem.Current.Award(scoreConfig.action);
    }
}