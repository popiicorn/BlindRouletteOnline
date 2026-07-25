using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    void Start()
    {
        // GameManager ‚©‚çŒ‹‰Ê‚ğæ“¾‚µ‚Ä•\¦iFindObjectOfType‚É•ÏXj
        GameManager gm = FindObjectOfType<GameManager>();
        int finalMoney = (gm != null) ? gm.GetFinalPlayerMoney() : 0;

        if (resultText != null)
        {
            resultText.text = $"¡‰ñ‚ÌŠl“¾‹àŠz: {finalMoney.ToString("#,0")}‰~";
        }
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}