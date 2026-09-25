using UnityEngine;
using UnityEngine.InputSystem;

public class ToyBox : MonoBehaviour
{
    [SerializeField] private GameObject putText;
    [SerializeField] private GameObject clearPanel;

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

            if (player.IsCarrying() && Keyboard.current.eKey.wasPressedThisFrame)
            {
                DeliverMemory();
            }
        }
    }

    private void DeliverMemory()
    {
        Debug.Log("思い出をおもちゃ箱に入れた！");

        putText.SetActive(false);

        player.SetCarrying(false);

        PlayerPrefs.SetInt("Stage1Clear",1);
        PlayerPrefs.Save();

        clearPanel.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            player = other.GetComponent<PlayerController>();
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