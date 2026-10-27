using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIのボタンに付けると、押した時に効果音が鳴る。
/// 音は Assets/Resources/SE/ に置いたファイル名(拡張子なし)で指定する。
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    #region Field

    [Tooltip("Resources/SE/ の中のファイル名(拡張子なし)")]
    [SerializeField] private string _clipName = "click";

    [Tooltip("音の大きさ(0?1)")]
    [SerializeField, Range(0f, 1f)] private float _volume = 0.8f;

    #endregion

    #region UnityLifeCycle

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    #endregion

    #region Private

    private void OnClicked()
    {
        AudioManager.PlaySE(_clipName, _volume);
    }

    #endregion
}