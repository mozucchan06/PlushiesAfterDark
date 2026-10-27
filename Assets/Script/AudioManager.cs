using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 効果音(SE)を鳴らす係。ゲーム開始時に自動で作られるので、シーンに置く必要はない。
/// 音のファイルは Assets/Resources/SE/ に置く(例: Resources/SE/click.wav)。
/// 使い方: AudioManager.PlaySE("click") / AudioManager.PlayRandomSE("SE/Step")
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Field

    private const int SourceCount = 8;          // 同時に鳴らせる数
    private const string SePrefix = "SE/";      // Resources の中の、効果音のフォルダ
    private const string VolumeKey = "SeVolume"; // 音量の保存先(あとで設定画面で使う)

    private static AudioManager _instance;

    private AudioSource[] _sources;
    private int _nextSource = 0;
    private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip[]> _folderCache = new Dictionary<string, AudioClip[]>();

    #endregion

    #region Property

    /// <summary>効果音の音量(0?1)。設定画面から変えられるように、保存される</summary>
    public static float seVolume
    {
        get { return PlayerPrefs.GetFloat(VolumeKey, 1f); }
        set { PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value)); }
    }

    #endregion

    #region UnityLifeCycle

    // ゲーム開始時(最初のシーンが読み込まれる前)に、自動で作る
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance != null) return;

        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<AudioManager>();
        _instance.CreateSources();
    }

    #endregion

    #region Public

    /// <summary>
    /// 効果音を1回鳴らす。
    /// </summary>
    /// <param name="clipName">Resources/SE/ の中のファイル名(拡張子なし)。例: "click"</param>
    /// <param name="volume">音の大きさ(0?1)</param>
    /// <param name="pitchRandom">音の高さをランダムにずらす幅。0なら、いつも同じ高さ</param>
    public static void PlaySE(string clipName, float volume = 1f, float pitchRandom = 0f)
    {
        if (_instance == null) return;

        AudioClip clip = _instance.LoadClip(clipName);
        if (clip != null) _instance.Play(clip, volume, pitchRandom);
    }

    /// <summary>
    /// フォルダの中の効果音から、1つをランダムに選んで鳴らす(足音など、同じ音が続かないようにする)。
    /// </summary>
    /// <param name="folder">Resources の中のフォルダ。例: "SE/Step"</param>
    /// <param name="volume">音の大きさ(0?1)</param>
    /// <param name="pitchRandom">音の高さをランダムにずらす幅</param>
    public static void PlayRandomSE(string folder, float volume = 1f, float pitchRandom = 0.05f)
    {
        if (_instance == null) return;

        AudioClip[] clips = _instance.LoadFolder(folder);
        if (clips == null || clips.Length == 0) return;

        _instance.Play(clips[Random.Range(0, clips.Length)], volume, pitchRandom);
    }

    #endregion

    #region Private

    private void CreateSources()
    {
        _sources = new AudioSource[SourceCount];
        for (int i = 0; i < SourceCount; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2Dの音(位置で聞こえ方を変えない)
            _sources[i] = source;
        }
    }

    private void Play(AudioClip clip, float volume, float pitchRandom)
    {
        // 順番に、空いているものを使う(音ごとに高さを変えられる)
        AudioSource source = _sources[_nextSource];
        _nextSource = (_nextSource + 1) % _sources.Length;

        source.clip = clip;
        source.volume = Mathf.Clamp01(volume) * seVolume;
        source.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
        source.Play();
    }

    private AudioClip LoadClip(string clipName)
    {
        if (_clipCache.TryGetValue(clipName, out AudioClip cached)) return cached;

        AudioClip clip = Resources.Load<AudioClip>(SePrefix + clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] LoadClip : Resources/{SePrefix}{clipName} が見つかりません");
        }

        _clipCache[clipName] = clip; // 見つからない時も覚えて、警告を何度も出さない
        return clip;
    }

    private AudioClip[] LoadFolder(string folder)
    {
        if (_folderCache.TryGetValue(folder, out AudioClip[] cached)) return cached;

        AudioClip[] clips = Resources.LoadAll<AudioClip>(folder);
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"[AudioManager] LoadFolder : Resources/{folder} に音がありません");
        }

        _folderCache[folder] = clips;
        return clips;
    }

    #endregion
}