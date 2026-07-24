using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("何秒後に消すか")]
    public float destroyTime = 3.0f;

    void Start()
    {
        // 作成されてから指定秒数後に消滅させる
        Destroy(gameObject, destroyTime);
    }
}