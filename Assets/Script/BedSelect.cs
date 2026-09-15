using UnityEngine;
using UnityEngine.SceneManagement;

public class BedSelect : MonoBehaviour
{
    private void OnMouseDown()
    {
        SceneManager.LoadScene("Stage1_Bed");
    }
}
