using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
    [Header("何番の部屋を爆発させるか")]
    public int selectedRoomNumber = 1;

    public void SelectRoom(int roomNumber)
    {
        selectedRoomNumber = roomNumber;
        Debug.Log($"仕掛け人が 【部屋 {roomNumber}】 を爆破対象に選びました！");

        // シーン内の GameManager を探してセットする
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.SetSelectedExplodeRoom(roomNumber);
        }
    }

    public void StartGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}