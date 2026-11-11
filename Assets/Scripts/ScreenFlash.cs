using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Destello de pantalla completa que se apaga en escalones (más arcade que un fundido suave).
[RequireComponent(typeof(Image))]
public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;
    [SerializeField, Range(2, 8)] private int steps = 4;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.raycastTarget = false;
        SetAlpha(0f);
    }

    public IEnumerator Play(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            float remaining = 1f - elapsed / seconds;
            SetAlpha(Mathf.Ceil(remaining * steps) / steps);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color c = color;
        c.a = alpha;
        _image.color = c;
    }
}