using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public CameraShake cameraShake;
    private bool isSetup = false; // クラスの先頭あたりに追加

    [Header("ゲーム設定")]
    public int maxTurns = 5;
    public float turnTime = 10f;

    [Header("プレイヤーの情報")]
    public PlayerController player;
    public int hostMoney = 0;
    public int playerTotalMoney = 0; // ★ここに移動しました！

    [Header("出現するお宝の種類")]
    public TreasureData[] availableTreasures;

    [Header("お宝の生成")]
    public Transform spawnPoint;
    public float spawnRadiusX = 10f;
    public float spawnRadiusZ = 2f;

    [Header("お宝の生成設定")]
    public int initialSpawnCount = 5;
    public int additionalSpawnPerTurn = 0;
    private int treasuresToSpawnNextTurn = 0;

    [Header("UI表示")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playerMoneyText;
    public TextMeshProUGUI hostMoneyText;

    [Header("爆発エフェクト")]
    public GameObject explosionPrefab;
    public Transform[] roomPositions;

    private int currentTurn = 0;
    private float currentTime;
    private bool isTimerRunning = false;
    private int doubleRoom = 0;

    [Header("扉の設定")]
    public GameObject[] doorObjects;

    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        /* 
        最初のシーンで起動したとき、既にGameSceneなら即セットアップ
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            SetupGameScene();
        }
        */
    }

    void Update()
    {
        // GameScene以外、またはタイマー停止中は処理しない
        if (SceneManager.GetActiveScene().name != "GameScene" || !isTimerRunning) return;

        currentTime -= Time.deltaTime;

        if (timerText != null) timerText.text = $"残り時間: {Mathf.CeilToInt(currentTime)}秒";
        if (playerMoneyText != null) playerMoneyText.text = $"プレイヤー: {playerTotalMoney.ToString("#,0")}円";
        if (hostMoneyText != null) hostMoneyText.text = $"仕掛け人: {hostMoney.ToString("#,0")}円";

        if (currentTime <= 0)
        {
            TimeUp();
        }
    }

    // シーン切り替え時に自動実行される
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            isSetup = false;
            SetupGameScene();
        }
        // ★ここを追加：LobbySceneに来たら自動でリセットする
        else if (scene.name == "LobbyScene")
        {
            currentTurn = 0;
            playerTotalMoney = 0;
            hostMoney = 0;
            isSetup = false;
            Debug.Log("ロビーに戻ったのでゲームデータをリセットしました！");
        }
    }

    void SetupGameScene()
    {
        if (isSetup) return; // すでにセットアップ済みなら何もしない
        isSetup = true;

        // 1. 各種UIやオブジェクトの再検索
        timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        playerMoneyText = GameObject.Find("PlayerMoneyText")?.GetComponent<TextMeshProUGUI>();
        hostMoneyText = GameObject.Find("HostMoneyText")?.GetComponent<TextMeshProUGUI>();
        player = FindObjectOfType<PlayerController>();

        cameraShake = FindObjectOfType<CameraShake>();
        if (cameraShake == null)
        {
            Debug.LogWarning("シーン内に CameraShake が見つかりませんでした！");
        }

        GameObject sp = GameObject.Find("SpawnPoint");
        if (sp != null) spawnPoint = sp.transform;

        // ★追加：扉のリストを再検索する
        // 扉オブジェクトに「Door」というタグを付けておくのが一番安全です！
        GameObject[] foundDoors = GameObject.FindGameObjectsWithTag("Door");
        if (foundDoors.Length > 0)
        {
            doorObjects = foundDoors;
        }
        else
        {
            Debug.LogError("タグ 'Door' が付いたオブジェクトが見つかりません！");
        }

        // ゲーム開始
        treasuresToSpawnNextTurn = initialSpawnCount;
        StartNextTurn();
    }

    void StartNextTurn()
    {
        currentTurn++;
        Debug.Log("★現在のターン数: " + currentTurn); // これで確認します！

        if (currentTurn > maxTurns)
        {
            Debug.Log("★最大ターンを超えたのでリザルトへ行きます！");
            CheckTurnResult();
            return;
        }

        // ...以下、宝生成などの処理
    

    doubleRoom = Random.Range(1, 6);

        for (int i = 0; i < treasuresToSpawnNextTurn; i++)
        {
            float randomX = Random.Range(-spawnRadiusX, spawnRadiusX);
            float randomZ = Random.Range(-spawnRadiusZ, spawnRadiusZ);
            float randomY = Random.Range(0f, 2f);

            Vector3 spawnPos = (spawnPoint != null ? spawnPoint.position : Vector3.zero) + new Vector3(randomX, randomY, randomZ);
            TreasureData selectedTreasure = ChooseRandomTreasure();

            GameObject boxObj = Instantiate(selectedTreasure.prefab, spawnPos, Quaternion.identity);
            TreasureBox box = boxObj.GetComponent<TreasureBox>();

            // 生成された宝箱を「ロビー（0）」として登録（これで最初の金額表示が正しくなります）
            box.UpdateRoom(0);
        }

        treasuresToSpawnNextTurn = additionalSpawnPerTurn;
        currentTime = turnTime;
        isTimerRunning = true;
    }

    void TimeUp()
    {
        isTimerRunning = false;
        timerText.text = "審判の刻...！";

        // 1. 爆発する部屋を「部屋1」に固定する
        RoomDetector explodeRoomObj = null;
        RoomDetector[] allRooms = FindObjectsOfType<RoomDetector>();

        foreach (var room in allRooms)
        {
            if (room.roomNumber == 1) // 部屋番号が1のものを見つける
            {
                explodeRoomObj = room;
                break;
            }
        }

        // もし部屋1が見つからない場合を考慮して、念のためランダムも残す
        if (explodeRoomObj == null)
        {
            int randomIndex = Random.Range(0, allRooms.Length);
            explodeRoomObj = allRooms[randomIndex];
        }

        // 2. 演出付き爆発コルーチンを開始
        StartCoroutine(ExecuteExplosionSequence(explodeRoomObj));


        /*↓ランダム
        isTimerRunning = false;
        timerText.text = "審判の刻...！";

        // 1. 爆発する部屋を決める
        RoomDetector[] allRooms = FindObjectsOfType<RoomDetector>();
        int randomIndex = Random.Range(0, allRooms.Length);
        RoomDetector explodeRoomObj = allRooms[randomIndex];

        // 2. 演出付き爆発コルーチンを開始（完了後にシーン遷移させる）
        StartCoroutine(ExecuteExplosionSequence(explodeRoomObj));
        */
    }

    // 演出コルーチンにシーン遷移の判定を追加
    // これが「新しい方」です。この1つだけが存在するようにしてください。
    private System.Collections.IEnumerator ExecuteExplosionSequence(RoomDetector room)
    {
        // A. 扉を全部閉じる
        foreach (GameObject door in doorObjects)
        {
            if (door != null)
            {
                Animator anim = door.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    // Trigger ではなく Bool で「閉まっている状態」をONにする
                    anim.SetBool("Closed", true);
                }
            }
        }

        yield return new WaitForSeconds(1.0f);

        // B. 爆発する部屋と一致する扉だけを点滅させる
        bool foundExplodingDoor = false;
        foreach (GameObject door in doorObjects)
        {
            Door doorScript = door.GetComponent<Door>();
            if (doorScript != null && doorScript.roomNumber == room.roomNumber)
            {
                StartCoroutine(FlashDoor(door));
                foundExplodingDoor = true;
            }
        }

        if (foundExplodingDoor) yield return new WaitForSeconds(1.8f);

        // C. 爆発処理
        Instantiate(explosionPrefab, room.transform.position, Quaternion.identity);
        if (cameraShake != null) cameraShake.PlayShake(0.5f, 0.3f);

        // 1. プレイヤーの判定（部屋またはロビーにいる場合）
        if (player != null && (player.currentRoom == room.roomNumber || player.currentRoom == 0))
        {
            playerTotalMoney = 0;
            player.UpdateAnimation(false, true);
            Debug.Log("爆発に巻き込まれた！所持金が没収されました！");

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 物理演算を有効にして真上に力を加える
                rb.isKinematic = false;
                rb.AddForce(Vector3.up * 15f + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)), ForceMode.Impulse);
            }
        }

        // 2. お宝の判定
        TreasureBox[] allTreasures = FindObjectsOfType<TreasureBox>();
        foreach (TreasureBox treasure in allTreasures)
        {
            if (treasure.IsCarried()) treasure.currentRoom = player.currentRoom;

            // 爆発対象：指定の部屋、またはロビー（部屋0）にいる場合
            if (treasure.currentRoom == room.roomNumber || treasure.currentRoom == 0)
            {
                // ★追加：吹き飛ばす処理
                Rigidbody rb = treasure.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    // 爆発の力を加える（プレイヤーより少し強めに飛ばすと面白いです）
                    rb.AddForce(Vector3.up * 15f + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)), ForceMode.Impulse);
                }

                // 吹き飛ばした後、少し遅らせて消滅させる（吹き飛ぶ見た目を作るため）
                //Destroy(treasure.gameObject, 1.0f);
                Debug.Log("宝箱が爆風で吹き飛んだ！");
            }
            // 爆発しない部屋にいる宝は獲得
            else if (treasure.currentRoom == player.currentRoom)
            {
                playerTotalMoney += treasure.data.moneyAmount;
                Destroy(treasure.gameObject);
                Debug.Log("宝箱を獲得！");
            }
            else
            {
                Destroy(treasure.gameObject);
            }
        }

        yield return new WaitForSeconds(3.0f);

        if (player != null) player.UpdateAnimation(false, false);

        CheckTurnResult();
    }

    private TreasureData ChooseRandomTreasure()
    {
        // ★修正：配列が空ならエラーを回避し、デバッグログを出す
        if (availableTreasures == null || availableTreasures.Length == 0)
        {
            Debug.LogError("お宝データが設定されていません！インスペクターを確認してください。");
            return null; // または適当な代替品を返す
        }

        int totalWeight = 0;
        foreach (TreasureData treasure in availableTreasures)
        {
            // データがnullでないか確認
            if (treasure != null) totalWeight += treasure.spawnWeight;
        }

        // 重みが0だった場合の回避
        if (totalWeight <= 0) return availableTreasures[0];

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (TreasureData treasure in availableTreasures)
        {
            if (treasure == null) continue;
            currentWeight += treasure.spawnWeight;
            if (randomValue < currentWeight)
            {
                return treasure;
            }
        }
        return availableTreasures[0];
    }

    private System.Collections.IEnumerator FlashDoor(GameObject door)
    {
        Renderer r = door.GetComponentInChildren<Renderer>();
        if (r == null) yield break;

        Color originalColor = r.material.color;
        for (int i = 0; i < 3; i++)
        {
            r.material.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            r.material.color = originalColor;
            yield return new WaitForSeconds(0.3f);
        }
    }



    // ゲーム開始ボタンが押された時などに呼ぶ
    public void StartGameFromSelection()
    {
        isSetup = false; // ここでリセット！
        currentTurn = 0;
        playerTotalMoney = 0;

        // ターンをカウントアップして、GameSceneへ移動する
        // currentTurn++; // ターンを進める
        SceneManager.LoadScene("GameScene");
    }

    void CheckTurnResult()
    {
        if (currentTurn >= maxTurns)
        {
            // 5ターン終了！リザルトへ
            SceneManager.LoadScene("ResultScene");
        }
        else
        {
            // 抽選へ
            SceneManager.LoadScene("SelectionScene");
        }
    }

    // GameManager.cs に追加
    public int GetFinalPlayerMoney()
    {
        return playerTotalMoney;
    }
}