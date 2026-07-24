using UnityEngine;
using TMPro;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    void Start()
    {
        // GameManager から結果を取得して表示
        int finalMoney = GameManager.Instance.GetFinalPlayerMoney();
        resultText.text = $"今回の獲得金額: {finalMoney.ToString("#,0")}円";
    }

    // 「タイトルに戻る」や「もう一度遊ぶ」ボタン用
    public void GoToTitle()
    {
        // 必要ならここで GameManager を破棄する処理を入れてもOK
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }
}