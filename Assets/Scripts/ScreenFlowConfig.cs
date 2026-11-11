using UnityEngine;

// Escenas de las pantallas. Un nombre vacío = esa pantalla no existe y el flujo se salta.
[CreateAssetMenu(menuName = "BombJack/Screen Flow Config")]
public class ScreenFlowConfig : ScriptableObject
{
    public string titleScene;
    public string instructionsScene;
    public string gameOverScene;
    public string rankingScene;
    public string optionsScene;
}