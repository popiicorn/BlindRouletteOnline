using UnityEngine;
using TMPro; // UI表示に必要
using System.Collections.Generic;

public class RoomDetector : MonoBehaviour
{
    [Header("この部屋の番号")]
    public int roomNumber;

    [Header("金額表示用のUI")]
    public TextMeshProUGUI moneyText;

    // この部屋にある宝箱を管理するリスト
    private List<TreasureBox> treasuresInRoom = new List<TreasureBox>();

    void Start()
    {
        UpdateDisplay(); // 最初の一回だけ計算
    }

    // 宝箱がこの部屋に入った/出たときに宝箱から呼んでもらう関数
    public void RegisterTreasure(TreasureBox treasure, bool isAdding)
    {
        {
            Door[] allDoors = FindObjectsOfType<Door>();
            foreach (var door in allDoors)
            {
                // ★修正1: ちゃんとこの部屋の扉か確認
                if (door.roomNumber == this.roomNumber)
                {
                    Animator doorAnim = door.GetComponentInChildren<Animator>();

                    // ★修正2: ログを絶対に出すようにする
                    bool isClosed = (doorAnim != null && doorAnim.GetBool("Closed"));
                    Debug.Log($"[チェック] 部屋{roomNumber}の扉の状態: Closed={isClosed}");

                    if (isClosed)
                    {
                        Debug.Log($"★ブロック！部屋{roomNumber}の扉が閉まっているので登録しません");
                        return; // ここで終了
                    }
                }
            }
        }

        if (isAdding)
        {
            if (!treasuresInRoom.Contains(treasure)) treasuresInRoom.Add(treasure);
        }
        else
        {
            if (treasuresInRoom.Contains(treasure)) treasuresInRoom.Remove(treasure);
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (moneyText == null) return;

        int total = 0;
        foreach (var t in treasuresInRoom)
        {
            if (t != null && t.data != null) total += t.data.moneyAmount;
        }
        moneyText.text = $"{total.ToString("#,0")}円";
    }

    // プレイヤーの出入り処理（既存の処理）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().currentRoom = roomNumber;

            // ★追加：プレイヤーが部屋に入ったら、持っているお宝の部屋情報も更新
            TreasureBox carried = other.GetComponentInChildren<TreasureBox>();
            if (carried != null) carried.UpdateRoom(roomNumber);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().currentRoom = 0;

            // ★追加：プレイヤーが部屋から出たら、持っているお宝の部屋情報も更新
            TreasureBox carried = other.GetComponentInChildren<TreasureBox>();
            if (carried != null) carried.UpdateRoom(0);
        }
    }
}