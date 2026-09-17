using UnityEngine;
using UnityEngine.InputSystem;

public class MemoryItem : MonoBehaviour
{
    [SerializeField] private GameObject pickupText;

    private bool playerNear = false;

    private void Start()
    {
        pickupText.SetActive(false);
    }


    private void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        pickupText.SetActive(false);

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            player.SetCarrying(true);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            pickupText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            pickupText.SetActive(false);
        }
    }
}
