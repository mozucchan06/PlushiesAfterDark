using UnityEngine;
using UnityEngine.InputSystem;

public class ToyBox : MonoBehaviour
{
    [SerializeField] private GameObject putText;
    [SerializeField] private GameObject clearPanel;

    [Header("クリア条件")]
    [SerializeField] private int requiredCount = 3;           // ステージにある思い出の数
    [SerializeField] private string clearKey = "Stage1Clear"; // ステージ2なら "Stage2Clear" にする

    private int depositedCount = 0;
    private bool playerNear = false;
    private PlayerController player;

    private void Start()
    {
        putText.SetActive(false);
        clearPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerNear && player != null)
        {
            // 思い出を持っているときだけ「E：入れる」を表示
            putText.SetActive(player.IsCarrying());

            if (player.IsCarrying() &&
                Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                DeliverMemory();
            }
        }
    }

    private void DeliverMemory()
    {
        depositedCount++;
        Debug.Log($"思い出をおもちゃ箱に入れた！ ({depositedCount}/{requiredCount})");

        putText.SetActive(false);
        player.SetCarrying(false);
        // 持っているアイテム表示を消す
        if (CarryHUD.Instance != null)
        {
            CarryHUD.Instance.Hide();
        }

        // まだ全部そろっていなければ、ここで終わり(クリアにしない)
        if (depositedCount < requiredCount)
        {
            return;
        }

        PlayerPrefs.SetInt(clearKey, 1);
        PlayerPrefs.Save();

        clearPanel.SetActive(true);
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

            if (putText != null)
            {
                putText.SetActive(false);
            }
        }
    }
}