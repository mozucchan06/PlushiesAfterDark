using UnityEngine;

/// <summary>
/// BGMを管理するクラス(シーンをまたいで常駐)
/// </summary>
public class BGMManager : MonoBehaviour
{
    #region Field
    public static BGMManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [Range(0f, 1f)]
    [SerializeField] private float _defaultVolume = 0.5f;
    #endregion

    #region UnityLifeCycle
    private void Awake()
    {
        // 重複生成を防ぐ
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_audioSource == null)
        {
            Debug.LogError("[BgmManager] Awake : _audioSource is Null!!! ");
            return;
        }
        _audioSource.volume = _defaultVolume;
    }
    #endregion

    /// <summary>
    /// BGMを切り替えて再生する
    /// </summary>
    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[BgmManager] PlayBgm : clip is Null");
            return;
        }
        if (_audioSource.clip == clip && _audioSource.isPlaying) return;

        _audioSource.clip = clip;
        _audioSource.Play();
    }
}