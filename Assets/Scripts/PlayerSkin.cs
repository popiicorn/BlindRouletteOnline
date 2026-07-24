using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    public Renderer playerRenderer; // キャラクターの体（MeshRenderer）をここにドラッグ
    public Color[] skinColors;      // インスペクターで好きな色を増やして登録

    void Start()
    {
        // ゲーム開始時に保存されている色を反映
        ApplySavedColor();
    }

    public void ChangeColor(int index)
    {
        // 1. 色を保存する
        PlayerPrefs.SetInt("PlayerColorIndex", index);
        PlayerPrefs.Save();

        // 2. すぐに色を変える
        ApplySavedColor();
    }

    void ApplySavedColor()
    {
        int index = PlayerPrefs.GetInt("PlayerColorIndex", 0);
        if (playerRenderer != null && index < skinColors.Length)
        {
            playerRenderer.material.color = skinColors[index];
        }
    }
}