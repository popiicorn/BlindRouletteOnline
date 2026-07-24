using UnityEngine;

// ファイル名は TreasureData.cs のままでもOK
[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]
public class TreasureData : ScriptableObject
{
    [Header("基本情報")]
    public string itemName;
    public GameObject prefab; // 実際の3Dモデル（プレハブ）

    [Header("宝箱用データ（爆弾は0でOK）")]
    public int moneyAmount;
    public float moveSpeedRate = 1.0f;

    [Header("出現設定")]
    public int spawnWeight = 10;
}