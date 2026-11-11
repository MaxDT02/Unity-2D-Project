using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Bomb : Collectible
{
    [SerializeField] private ScoreConfig scoreConfig;

    private static readonly int IsLitHash = Animator.StringToHash("IsLit");

    public bool IsLit { get; private set; }

    public void Light()
    {
        if (IsCollected || IsLit) return;
        IsLit = true;
        Anim.SetBool(IsLitHash, true);
    }

    protected override int GetPoints() => IsLit ? scoreConfig.litBomb : scoreConfig.unlitBomb;
}