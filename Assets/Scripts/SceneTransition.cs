using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Carga de escenas con corte directo o fundido a negro, según GameSessionConfig.
public class SceneTransition : MonoBehaviour
{
    private static SceneTransition _instance;

    private static SceneTransition Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("SceneTransition").AddComponent<SceneTransition>();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _instance = null;

    private CanvasGroup _group;
    private bool _busy;

    public static void LoadNextRound() =>
        Instance.Begin(() => GameSession.Instance.NextRound(), () => GameSession.Instance.CurrentSceneName);

    public static void RestartRun() =>
        Instance.Begin(() => GameSession.Instance.StartNewRun(), () => GameSession.Instance.CurrentSceneName);

    public static void Load(string scene) => Instance.Begin(null, () => scene);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        GameObject black = new("Black", typeof(RectTransform), typeof(Image));
        black.transform.SetParent(transform, false);
        RectTransform rect = (RectTransform)black.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image image = black.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    private void Begin(Action beforeLoad, Func<string> sceneName)
    {
        if (_busy) return;
        StartCoroutine(Run(beforeLoad, sceneName));
    }

    private IEnumerator Run(Action beforeLoad, Func<string> sceneName)
    {
        _busy = true;
        GameSessionConfig config = GameSession.Instance.Config;
        bool fade = config != null && config.transition == TransitionStyle.Fade && config.fadeDuration > 0f;

        if (fade) yield return Fade(0f, 1f, config.fadeDuration);

        // El nombre se calcula con el estado ya actualizado (ronda siguiente, partida nueva).
        beforeLoad?.Invoke();
        string scene = sceneName();

        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogError("SceneTransition: no hay escena para esta ronda (revisa GameSessionConfig.levelScenes).");
        }
        else
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(scene);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f) yield return null;

            GameClock.Reset();                  // la escena nueva arranca con el tiempo normal
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
        }

        GameClock.Reset();
        if (fade) yield return Fade(1f, 0f, config.fadeDuration);
        _busy = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        _group.alpha = to;
    }
}