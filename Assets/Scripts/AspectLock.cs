using UnityEngine;

// Fija la proporción de la imagen (240:272) con barras negras (letterbox/pillarbox).
// Ponlo en la cámara principal de cada escena. Como el canvas es "Screen Space - Camera",
// se ajusta solo al área visible.
[RequireComponent(typeof(Camera))]
public class AspectLock : MonoBehaviour
{
    [SerializeField] private float width = 240f;
    [SerializeField] private float height = 272f;

    private Camera _camera;
    private int _lastW, _lastH;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        CreateBackgroundCamera();
        Apply();
    }

    private void Update()
    {
        if (Screen.width != _lastW || Screen.height != _lastH) Apply();
    }

    private void Apply()
    {
        _lastW = Screen.width;
        _lastH = Screen.height;

        float target = width / height;
        float window = (float)_lastW / _lastH;

        Rect r;
        if (window > target)            // ventana más ancha: barras a los lados
        {
            float w = target / window;
            r = new Rect((1f - w) * 0.5f, 0f, w, 1f);
        }
        else                            // ventana más alta: barras arriba y abajo
        {
            float h = window / target;
            r = new Rect(0f, (1f - h) * 0.5f, 1f, h);
        }
        _camera.rect = r;
    }

    // Una cámara que solo pinta de negro toda la pantalla, por detrás de la del juego.
    private void CreateBackgroundCamera()
    {
        GameObject go = new GameObject("Letterbox Background");
        go.transform.SetParent(transform, false);

        Camera bg = go.AddComponent<Camera>();
        bg.clearFlags = CameraClearFlags.SolidColor;
        bg.backgroundColor = Color.black;
        bg.cullingMask = 0;
        bg.depth = _camera.depth - 1f;
        bg.rect = new Rect(0f, 0f, 1f, 1f);
        bg.allowHDR = false;
        bg.allowMSAA = false;
    }
}