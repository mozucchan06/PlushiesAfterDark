using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage3Select : MonoBehaviour
{
    [SerializeField] private GameObject highlight;

    private void OnMouseEnter()
    {
        highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        SceneManager.LoadScene("Stage3");
    }
}
