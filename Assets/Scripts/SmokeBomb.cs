using UnityEngine;

public class SmokeBomb : MonoBehaviour
{
    [Header("地面に着いた時に出す煙のプレハブ")]
    public GameObject smokeEffectPrefab;

    private bool isCarried = false;
    private bool isPlayerInRange = false;
    private Transform playerTransform;
    private Rigidbody rb;
    private bool isThrown = false; // 投げられたかどうかのフラグ

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // プレイヤーを自動で見つける
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // 投げる処理（持っている状態でスペースキー）
        if (isCarried && Input.GetKeyDown(KeyCode.Space))
        {
            Drop();
        }
        // 拾う処理（範囲内にいてスペースキー）
        else if (!isCarried && isPlayerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            if (playerTransform != null)
            {
                PickUp(playerTransform);
            }
        }
    }

    // 拾う処理（宝箱の仕組みをそのまま流用）
    public void PickUp(Transform targetPlayer)
    {
        var pc = targetPlayer.GetComponent<PlayerController>();
        if (pc != null && pc.isHoldingTreasure)
        {
            Debug.Log("既にアイテムを持っています！");
            return;
        }

        isCarried = true;
        playerTransform = targetPlayer;

        if (pc != null) pc.isHoldingTreasure = true;
        if (rb != null) rb.isKinematic = true;

        // プレイヤーとの衝突を無視
        Collider playerCol = playerTransform.GetComponent<Collider>();
        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger) Physics.IgnoreCollision(col, playerCol, true);
        }

        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0, 2.5f, 0);
    }

    // 投げる（Drop）処理
    public void Drop()
    {
        isCarried = false;
        isThrown = true; // 「投げた」フラグをON

        Transform player = transform.parent;
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isHoldingTreasure = false;
                pc.SetSpeedMultiplier(1.0f);
            }

            // 衝突無視を解除
            Collider playerCol = player.GetComponent<Collider>();
            foreach (Collider col in GetComponents<Collider>())
            {
                if (!col.isTrigger) Physics.IgnoreCollision(col, playerCol, false);
            }
        }

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            // 前方に投げる力
            if (player != null)
            {
                Vector3 throwDirection = player.forward * 5f + Vector3.up * 2f;
                rb.AddForce(throwDirection, ForceMode.Impulse);
            }
        }
    }

    // プレイヤーが範囲に入った時の判定（拾う用）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerTransform = null;
        }
    }

    // ★【爆弾だけの新機能】地面や壁にぶつかった瞬間
    void OnCollisionEnter(Collision collision)
    {
        if (isThrown) // 投げられた後なら
        {
            // 煙のエフェクトを生成
            if (smokeEffectPrefab != null)
            {
                Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
            }

            // 爆弾本体を消す
            Destroy(gameObject);
        }
    }
}