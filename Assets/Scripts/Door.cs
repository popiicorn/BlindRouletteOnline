using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("この扉が担当する部屋番号")]
    public int roomNumber;

    // Animatorをここに持たせておく
    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // 扉の状態を更新する関数
    public void SetDoorClosed(bool isClosed)
    {
        // 1. Animatorに「Closed」パラメータを渡す
        if (anim != null)
        {
            anim.SetBool("Closed", isClosed);
        }

        // 2. もしアニメーション開始の合図（Trigger）も必要ならここで呼ぶ
        if (isClosed)
        {
            anim.SetTrigger("Close"); // 閉める合図
        }
    }
}