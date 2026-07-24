using UnityEngine;

public class TreasureBox : MonoBehaviour
{
    [Header("お宝のデータ")]
    public TreasureData data; // ★これを追加！先ほど作ったデータをセットする枠です

    [Header("今置いてある部屋の番号（0=ロビー）")]
    public int currentRoom = 0;

    private bool isCarried = false;
    private bool isPlayerInRange = false;
    private Transform playerTransform;
    private Rigidbody rb;
    private bool isPlayerInTrigger = false; // ★新しい判定フラグ

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ★データがセットされているか確認用のログ
        if (data != null)
        {
            Debug.Log($"この宝箱は {data.itemName} です。金額: {data.moneyAmount}");
        }
    }

    void Update()
    {
        // ★プレイヤーを自動で見つける（既に playerTransform がある場合はそれを使う）
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // 宝を投げる処理（isCarried が true なら）
        if (isCarried && Input.GetKeyDown(KeyCode.Space))
        {
            int currentRoom = 0;
            if (playerTransform != null)
            {
                var pc = playerTransform.GetComponent<PlayerController>();
                if (pc != null) currentRoom = pc.currentRoom;
            }
            Drop(currentRoom);
        }
        // ★拾う処理：距離ではなく、OnTriggerEnterで判定された isPlayerInRange を使う
        else if (!isCarried && isPlayerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            if (playerTransform != null)
            {
                PickUp(playerTransform);
            }
        }
    }

    public void Drop(int roomNumber)
    {
        isCarried = false;
        currentRoom = roomNumber;

        // ★ここで1回だけ宣言すればOK！
        Transform player = transform.parent;

        // プレイヤーが持っていない状態に戻す処理
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isHoldingTreasure = false;
                pc.SetSpeedMultiplier(1.0f); // ついでに速度戻し処理もここに入れました
            }

            // プレイヤーとの衝突無視を解除
            Collider playerCol = player.GetComponent<Collider>();
            foreach (Collider col in GetComponents<Collider>())
            {
                if (!col.isTrigger) Physics.IgnoreCollision(col, playerCol, false);
            }
        }

        transform.SetParent(null);

        if (rb != null) rb.isKinematic = false;

        if (player != null)
        {
            Vector3 throwDirection = player.forward * 5f + Vector3.up * 2f;
            rb.AddForce(throwDirection, ForceMode.Impulse);
        }

        UpdateRoom(roomNumber);
    }

    public bool IsCarried()
    {
        return isCarried;
    }

    void OnTriggerEnter(Collider other)
    {
        // プレイヤーの判定だけを残す
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = other.transform;
            Debug.Log("プレイヤーが範囲に入りました");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 【既存の処理】プレイヤーが離れたときの判定
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerTransform = null;
        }

        // ★【ここを追加！】宝箱が部屋（RoomDetector）から完全に外に出たら、一旦ロビー（0）に戻す
        RoomDetector room = other.GetComponent<RoomDetector>();
        if (room != null)
        {
            // 部屋のエリアから出たとき、現在覚えている部屋番号と同じなら0（ロビー）に戻す
            if (currentRoom == room.roomNumber)
            {
                currentRoom = 0;
                Debug.Log($"{name} が部屋から出てロビー（0）に戻りました");
            }
        }
    }

    // TreasureBox.cs
    public void UpdateRoom(int newRoomNumber)
    {
        // すでに同じ部屋なら何もしない
        if (currentRoom == newRoomNumber) return;

        // 1. 全ての部屋を取得して、正しい部屋を探す
        RoomDetector[] allRooms = FindObjectsOfType<RoomDetector>();

        // 2. 「今の部屋(currentRoom)」のリストから自分を削除する
        foreach (var room in allRooms)
        {
            if (room.roomNumber == currentRoom)
            {
                room.RegisterTreasure(this, false); // 削除
                Debug.Log($"[カウント] 部屋{currentRoom}から削除: {name}");
            }
        }

        // 3. 部屋番号を更新
        currentRoom = newRoomNumber;

        // 4. 「新しい部屋(newRoomNumber)」のリストに自分を追加する
        foreach (var room in allRooms)
        {
            if (room.roomNumber == newRoomNumber)
            {
                room.RegisterTreasure(this, true); // 追加
                Debug.Log($"[カウント] 部屋{newRoomNumber}に追加: {name}");
            }
        }
    }

    public void PickUp(Transform targetPlayer)
    {
        // ★追加：プレイヤーのPlayerControllerを取得して、既に持っていないか確認
        var pc = targetPlayer.GetComponent<PlayerController>();
        if (pc != null && pc.isHoldingTreasure)
        {
            Debug.Log("既に宝箱を持っています！");
            return; // 持っている場合はここで中断！
        }

        isCarried = true;
        playerTransform = targetPlayer;

        if (rb != null) rb.isKinematic = true;

        // 衝突無視の処理
        Collider playerCol = playerTransform.GetComponent<Collider>();
        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger) Physics.IgnoreCollision(col, playerCol, true);
        }

        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0, 2.5f, 0);

        if (data != null && playerTransform != null)
        {
            playerTransform.GetComponent<PlayerController>().SetSpeedMultiplier(data.moveSpeedRate);
        }
    }
}