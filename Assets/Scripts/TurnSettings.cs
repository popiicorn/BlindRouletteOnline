using UnityEngine;
using TMPro;

public class TurnSettings : MonoBehaviour
{
    [Header("設定するターン数を表示するテキスト")]
    public TextMeshProUGUI turnDisplay;

    private int currentSettingTurns = 5; // 初期値

    void Start()
    {
        // PlayerPrefs から保存されているターン数を読み込む（なければデフォルト5）
        currentSettingTurns = PlayerPrefs.GetInt("SettingMaxTurns", 5);
        UpdateDisplay();
    }

    // ボタンが押されたときに呼ぶ関数（+1 したり -1 したりする）
    public void ChangeTurns(int amount)
    {
        currentSettingTurns += amount;

        // ★最小1、最大10の範囲に収める（10を超えたら1に戻り、1未満になったら10に戻るループ）
        if (currentSettingTurns > 10) currentSettingTurns = 1;
        if (currentSettingTurns < 1) currentSettingTurns = 10;

        // 保存して画面を更新
        PlayerPrefs.SetInt("SettingMaxTurns", currentSettingTurns);
        PlayerPrefs.Save();

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (turnDisplay != null)
        {
            turnDisplay.text = $"ターン数: {currentSettingTurns}";
        }
    }
}