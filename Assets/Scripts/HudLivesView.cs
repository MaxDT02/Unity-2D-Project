using UnityEngine;

public class HudLivesView : MonoBehaviour
{
    [Tooltip("Un icono por vida posible, de izquierda a derecha. Se activan tantos como vidas de reserva.")]
    [SerializeField] private GameObject[] icons;

    private void OnEnable()
    {
        GameSession session = GameSession.Instance;
        session.LivesChanged += Show;
        Show(session.Lives);
    }

    private void OnDisable()
    {
        if (GameSession.HasInstance) GameSession.Instance.LivesChanged -= Show;
    }

    private void Show(int lives)
    {
        for (int i = 0; i < icons.Length; i++)
            if (icons[i] != null) icons[i].SetActive(i < lives);
    }
}