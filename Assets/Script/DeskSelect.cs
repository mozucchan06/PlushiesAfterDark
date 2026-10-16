using UnityEngine;

public class DeskSelect : MonoBehaviour
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject lockIcon;                     // 鍵の絵
    [SerializeField] private GameObject clearText;                    // クリア済みの時に出す文字
    [SerializeField] private string clearKey = "Stage2Clear";           // このステージのクリア記録
    [SerializeField] private string requiredClearKey = "Stage1Clear"; // 前のステージのクリア記録

    private bool unlocked;

    private void Start()
    {
        unlocked = string.IsNullOrEmpty(requiredClearKey)
                   || PlayerPrefs.GetInt(requiredClearKey, 0) == 1;

        if (lockIcon != null) lockIcon.SetActive(!unlocked);

        // このステージをクリア済みなら、クリアの文字を出す
        if (clearText != null) clearText.SetActive(PlayerPrefs.GetInt(clearKey, 0) == 1);
    }

    private void OnMouseEnter()
    {
        // ロック中は光らせない
        if (unlocked) highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!unlocked)
        {
            Debug.Log("ステージ2はまだ遊べません(ステージ1をクリアしてください)");
            return;
        }

        SceneFader.LoadScene("Stage2");
    }
}