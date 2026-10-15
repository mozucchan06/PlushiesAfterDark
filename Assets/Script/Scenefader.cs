using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    private const float FadeDuration = 0.4f; // 暗くなる/明るくなるのにかかる秒数

    private static SceneFader instance;

    private Image fadeImage;
    private bool isFading = false;

    // ゲーム開始時に自動で作る(最初のシーンが読み込まれる前)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;

        var go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);

        instance = go.AddComponent<SceneFader>();
        instance.Build();
        instance.StartCoroutine(instance.FirstFadeIn());
    }

    // ---------- 外から呼ぶ ----------

    public static void LoadScene(string sceneName)
    {
        // 何らかの理由で作られていなければ、フェードなしで切り替える
        if (instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (instance.isFading) return; // 切り替え中の二重押しを無視する
        instance.StartCoroutine(instance.LoadRoutine(sceneName, false));
    }

    public static void ReloadCurrent()
    {
        if (instance == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (instance.isFading) return;
        instance.StartCoroutine(instance.LoadRoutine(null, true));
    }

    // ---------- 内部 ----------

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue; // 一番手前に描く

        var imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(transform, false);

        fadeImage = imageObject.AddComponent<Image>();

        // 画面いっぱいに広げる
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Resources に TransitionImage があれば、それを使う。無ければ黒
        Sprite sprite = Resources.Load<Sprite>("TransitionImage");
        if (sprite != null)
        {
            fadeImage.sprite = sprite;
            fadeImage.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }

        fadeImage.raycastTarget = false;
    }

    private IEnumerator FirstFadeIn()
    {
        isFading = true;
        SetAlpha(1f);
        yield return Fade(1f, 0f);
        isFading = false;
    }

    private IEnumerator LoadRoutine(string sceneName, bool reload)
    {
        isFading = true;

        // 暗くする(この間、ボタンは押せない)
        yield return Fade(0f, 1f);

        Time.timeScale = 1f;
        if (reload)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }

        yield return null; // 読み込みを1フレーム待つ

        // 明るくする
        yield return Fade(1f, 0f);

        isFading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        fadeImage.raycastTarget = true; // フェード中は、後ろのボタンを押せなくする

        float time = 0f;
        while (time < FadeDuration)
        {
            time += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, time / FadeDuration));
            yield return null;
        }

        SetAlpha(to);
        fadeImage.raycastTarget = to > 0.5f;
    }

    private void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}