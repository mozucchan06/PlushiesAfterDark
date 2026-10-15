using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 隠れ場。近づいて E で隠れ、もう一度 E で出る。
/// 隠れている間、プレイヤーの絵は消えて、隠れ場に入ったように見える。
/// </summary>
public class HideSpot : MonoBehaviour
{
    [Header("表示")]
    [SerializeField] private GameObject hideText;  // 「E:かくれる」
    [SerializeField] private GameObject exitText;  // 「E:でる」

    [Header("設定")]
    [SerializeField] private Transform hidePoint;       // 隠れている間のプレイヤー位置(空なら自分の位置)
    [SerializeField] private float useDistance = 1.5f;  // この距離以内なら E で隠れられる

    private PlayerController player;
    private Vector2 exitPosition; // 隠れる前にいた位置(出る時に戻る)
    private bool occupied = false;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        SetTexts(false, false);
    }

    private void Update()
    {
        if (player == null) return;

        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // この隠れ場に隠れている → E で出る
        if (occupied)
        {
            SetTexts(false, true);

            if (ePressed)
            {
                Exit();
            }
            return;
        }

        // 隠れられる条件:近い・他の場所に隠れていない・箱を掴んでいない
        bool canHide = !player.IsHidden() &&
                       GrabLock.Current == null &&
                       Vector2.Distance(player.transform.position, transform.position) <= useDistance;

        SetTexts(canHide, false);

        if (canHide && ePressed)
        {
            Enter();
        }
    }

    private void Enter()
    {
        exitPosition = player.transform.position;

        Vector2 spot = hidePoint != null ? (Vector2)hidePoint.position : (Vector2)transform.position;
        player.Hide(spot);

        occupied = true;
    }

    private void Exit()
    {
        player.Unhide(exitPosition);
        occupied = false;
    }

    private void SetTexts(bool showHide, bool showExit)
    {
        if (hideText != null) hideText.SetActive(showHide);
        if (exitText != null) exitText.SetActive(showExit);
    }
}