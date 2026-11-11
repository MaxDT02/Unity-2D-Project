using System;
using System.Collections;
using UnityEngine;

// Las letras caen desde arriba hasta donde las hayas colocado, con rebote y una tras otra.
// Coloca cada letra en su sitio final en el editor; al empezar se suben y caen hasta él.
public class LetterDrop : MonoBehaviour
{
    private const float FirstImpact = 1f / 2.75f;   // momento del primer golpe en la curva de rebote

    [SerializeField] private RectTransform[] letters;
    [Tooltip("Altura (unidades) desde la que caen. Mayor que media pantalla para que entren desde fuera.")]
    [SerializeField, Min(1f)] private float dropHeight = 160f;
    [SerializeField, Min(0.1f)] private float duration = 0.6f;
    [Tooltip("Retraso entre una letra y la siguiente.")]
    [SerializeField, Min(0f)] private float stagger = 0.1f;

    // Índice de la letra que acaba de golpear el suelo.
    public event Action<int> Landed;

    private Vector2[] _final;

    private void Awake()
    {
        _final = new Vector2[letters.Length];
        for (int i = 0; i < letters.Length; i++)
        {
            _final[i] = letters[i].anchoredPosition;
            letters[i].gameObject.SetActive(false);
        }
    }

    public IEnumerator Play()
    {
        int count = letters.Length;
        bool[] landed = new bool[count];
        float total = duration + stagger * (count - 1);
        float start = Time.unscaledTime;

        for (int i = 0; i < count; i++)
        {
            letters[i].anchoredPosition = _final[i] + Vector2.up * dropHeight;
            letters[i].gameObject.SetActive(true);
        }

        while (true)
        {
            float elapsed = Time.unscaledTime - start;
            for (int i = 0; i < count; i++)
            {
                float t = Mathf.Clamp01((elapsed - i * stagger) / duration);
                float offset = Mathf.Round(Mathf.Lerp(dropHeight, 0f, EaseOutBounce(t)));   // píxeles enteros
                letters[i].anchoredPosition = _final[i] + Vector2.up * offset;

                if (!landed[i] && t >= FirstImpact)
                {
                    landed[i] = true;
                    Landed?.Invoke(i);
                }
            }

            if (elapsed >= total) break;
            yield return null;
        }

        for (int i = 0; i < count; i++) letters[i].anchoredPosition = _final[i];
    }

    private static float EaseOutBounce(float t)
    {
        const float n = 7.5625f, d = 2.75f;
        if (t < 1f / d) return n * t * t;
        if (t < 2f / d) { t -= 1.5f / d; return n * t * t + 0.75f; }
        if (t < 2.5f / d) { t -= 2.25f / d; return n * t * t + 0.9375f; }
        t -= 2.625f / d;
        return n * t * t + 0.984375f;
    }
}