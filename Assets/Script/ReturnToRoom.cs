using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToRoom : MonoBehaviour
{
    public void GoToRoomSelect()
    {
        SceneManager.LoadScene("RoomSelect");
    }
}
