using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Score Config")]
public class ScoreConfig : ScriptableObject
{
    public int unlitBomb = 100;
    public int litBomb = 200;
    public int action = 10;
    public int bonusCoin = 1000;
}