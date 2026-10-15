using UnityEngine;
using UnityEngine.InputSystem;

public class MemoryItem : MonoBehaviour
{
    [SerializeField] private GameObject pickupText;
    [SerializeField] private Sprite icon; // 画面端に表示する絵(空なら自分のスプライトを使う)

    private bool playerNear = false;
    private PlayerController player;

    private void Start()
    {
        SetPickupText(false);
    }

    private void Update()
    {
        if (!playerNear || player == null) return;

        // 持っていない & 箱を掴んでいない時だけ拾える
        bool canPickUp = !player.IsCarrying() && GrabLock.Current == null;
        SetPickupText(canPickUp);

        if (canPickUp && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        SetPickupText(false);

        // 先に持った状態にするので、同じフレームの他のアイテムは拾えなくなる
        player.SetCarrying(true);

        if (CarryHUD.Instance != null)
        {
            CarryHUD.Instance.Show(GetIcon());
        }

        Destroy(gameObject);
    }

    // 「E:拾う」の表示を切り替える。アイテムと一緒に文字が消えた後でも、エラーにならない
    private void SetPickupText(bool visible)
    {
        if (pickupText != null) pickupText.SetActive(visible);
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
            SetPickupText(false);
        }
    }
}