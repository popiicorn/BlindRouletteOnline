using UnityEngine;

public class TreasureRoomSensor : MonoBehaviour
{
    private TreasureBox parentBox;

    void Start()
    {
        // 親（宝箱本体）にあるTreasureBoxスクリプトを探す
        parentBox = GetComponentInParent<TreasureBox>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 部屋に入った時
        RoomDetector room = other.GetComponent<RoomDetector>();
        if (room != null && parentBox != null)
        {
            parentBox.UpdateRoom(room.roomNumber);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 部屋から出た時
        RoomDetector room = other.GetComponent<RoomDetector>();
        if (room != null && parentBox != null)
        {
            // 現在の部屋から出たらロビー（0）に戻す
            if (parentBox.currentRoom == room.roomNumber)
            {
                parentBox.UpdateRoom(0);
            }
        }
    }
}