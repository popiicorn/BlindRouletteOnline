using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // 通常のシーン移動
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ★追加：リザルト画面などから「データをリセットして」シーン移動したいとき用
    public void LoadSceneAndResetData(string sceneName)
    {
        // 次のゲームのためにすべて初期化
        GameManager.playerTotalMoney = 0;
        GameManager.hostMoney = 0;
        GameManager.currentTurn = 1;

        // シーンを移動
        SceneManager.LoadScene(sceneName);
    }
}