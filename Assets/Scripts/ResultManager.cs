using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    void Start()
    {
        // GameManager.playerTotalMoney から直接、稼いだ金額を取得する！
        int finalMoney = GameManager.playerTotalMoney;

        if (resultText != null)
        {
            resultText.text = $"今回の獲得金額: {finalMoney.ToString("#,0")}円";
        }
        else
        {
            Debug.LogWarning("ResultText がアタッチされていません！");
        }
    }

    public void GoToTitle()
    {
        // タイトルに戻るときにデータをリセット
        GameManager.currentTurn = 1;
        GameManager.playerTotalMoney = 0;
        GameManager.hostMoney = 0;

        SceneManager.LoadScene("TitleScene");
    }
}