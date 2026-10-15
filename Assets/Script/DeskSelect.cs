using UnityEngine;

public class DeskSelect : MonoBehaviour
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject lockIcon;                     // 鍵の絵など(なくてもよい)
    [SerializeField] private string requiredClearKey = "Stage1Clear"; // 前のステージのクリア記録

    private bool unlocked;

    private void Start()
    {
        unlocked = string.IsNullOrEmpty(requiredClearKey)
                   || PlayerPrefs.GetInt(requiredClearKey, 0) == 1;

        if (lockIcon != null) lockIcon.SetActive(!unlocked);

        // 原因調べ用のログ(直ったら消してよい)
        Debug.Log($"ステージ2(机): 必要なクリア記録='{requiredClearKey}' 保存されている値={PlayerPrefs.GetInt(requiredClearKey, 0)} 解放={unlocked}");
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