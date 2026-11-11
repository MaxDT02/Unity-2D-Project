using System.Collections;
using TMPro;
using UnityEngine;

// Pantalla de bonus de fin de ronda. Es un RoundStep: va en clearSteps, después de la animación de Jack.
// Con el juego pausado: todo va en tiempo sin escalar.
public class BonusScreen : RoundStep
{
    private const float MaxFrameTime = 0.05f;   // un tirón de frame no se come trozos de la cuenta atrás

    [SerializeField] private RoundFlowConfig config;
    [Tooltip("La pantalla entera (cobertura negra, panel y textos). Debe estar desactivada de inicio.")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text litBombsText;    // el "23"
    [SerializeField] private TMP_Text bonusText;       // el "50000", baja hasta 0

    // Solo con bonus (20 o más bombas encendidas).
    public override bool ShouldRun(RoundStats stats) => config != null && config.BonusFor(stats.LitBombs) > 0;

    public override IEnumerator Run(RoundStats stats)
    {
        int bonus = config.BonusFor(stats.LitBombs);
        litBombsText.text = stats.LitBombs.ToString();
        bonusText.text = bonus.ToString();
        root.SetActive(true);

        yield return new WaitForSecondsRealtime(config.bonusPreDelay);

        // Cuenta atrás: los puntos salen del número y entran en la puntuación a la vez.
        GameSession session = GameSession.Instance;
        int paid = 0;
        float elapsed = 0f;
        while (paid < bonus)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFrameTime);
            int target = Mathf.Min(bonus, Mathf.FloorToInt(elapsed * config.countdownPointsPerSecond));
            if (target > paid)
            {
                session.AddScore(target - paid);
                paid = target;
                bonusText.text = (bonus - paid).ToString();
            }
            yield return null;
        }

        yield return new WaitForSecondsRealtime(config.bonusPostDelay);
        // La pantalla se queda puesta: la siguiente ronda se carga a continuación y la sustituye.
    }
}