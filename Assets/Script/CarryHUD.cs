using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 持っている思い出のアイコンを画面端に表示する。
/// Canvas の下の Image に付ける。
/// </summary>
[RequireComponent(typeof(Image))]
public class CarryHUD : MonoBehaviour
{
    public static CarryHUD Instance { get; private set; }

    private Image iconImage;

    private void Awake()
    {
        Instance = this;
        iconImage = GetComponent<Image>();
        iconImage.enabled = false; // 最初は何も持っていない
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
        iconImage.enabled = icon != null;
    }

    public void Hide()
    {
        iconImage.enabled = false;
    }
}