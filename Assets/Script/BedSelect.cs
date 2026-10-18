using UnityEngine;

public class BedSelect : MonoBehaviour
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject clearText;

    private void Start()
    {
        if (PlayerPrefs.GetInt("Stage1Clear", 0) == 1)
        {
            clearText.SetActive(true);
        }
    }

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
        SceneFader.LoadScene("Stage1");
    }
}