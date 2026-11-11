using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer valueRenderer;
    [SerializeField] private SpriteRenderer multiplierRenderer;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float gap = 0.05f;

    private float _timer;

    public void Show(Vector3 position, Sprite valueSprite, Sprite multiplierSprite)
    {
        transform.position = position;
        valueRenderer.sprite = valueSprite;

        bool hasMultiplier = multiplierSprite != null;
        multiplierRenderer.gameObject.SetActive(hasMultiplier);

        if (!hasMultiplier)
        {
            Place(valueRenderer, 0f);
        }
        else
        {
            multiplierRenderer.sprite = multiplierSprite;
            float valueWidth = valueSprite.bounds.size.x;
            float multiplierWidth = multiplierSprite.bounds.size.x;
            float total = valueWidth + gap + multiplierWidth;

            // Conjunto centrado sobre la posición de la bomba.
            Place(valueRenderer, -total * 0.5f + valueWidth * 0.5f);
            Place(multiplierRenderer, total * 0.5f - multiplierWidth * 0.5f);
        }

        _timer = duration;
        gameObject.SetActive(true);
    }

    private static void Place(SpriteRenderer renderer, float centerX)
    {
        Bounds b = renderer.sprite.bounds;
        renderer.transform.localPosition = new Vector3(centerX - b.center.x, -b.center.y, 0f);
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) gameObject.SetActive(false);
    }
}