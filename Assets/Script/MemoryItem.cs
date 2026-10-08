using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 思い出アイテム。近づいて E で拾う。
/// すでに思い出を持っている間や、箱を掴んでいる間は拾えない。
/// Collider2D は Is Trigger をオンにしておくこと。
/// </summary>
public class MemoryItem : MonoBehaviour
{
    [SerializeField] private GameObject pickupText;
    [SerializeField] private Sprite icon; // 画面端に表示する絵(空なら自分のスプライトを使う)

    private bool playerNear = false;
    private PlayerController player;

    private void Start()
    {
        pickupText.SetActive(false);
    }

    private void Update()
    {
        if (!playerNear || player == null) return;

        // 持っていない & 箱を掴んでいない時だけ拾える
        bool canPickUp = !player.IsCarrying() && GrabLock.Current == null;
        pickupText.SetActive(canPickUp);

        if (canPickUp && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        pickupText.SetActive(false);

        // 先に持った状態にするので、同じフレームの他のアイテムは拾えなくなる
        player.SetCarrying(true);

        if (CarryHUD.Instance != null)
        {
            CarryHUD.Instance.Show(GetIcon());
        }

        Destroy(gameObject);
    }

    private Sprite GetIcon()
    {
        if (icon != null) return icon;

        var sr = GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            player = other.GetComponentInParent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            player = null;
            pickupText.SetActive(false);
        }
    }
}