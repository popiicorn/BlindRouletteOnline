using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public CameraShake cameraShake;

    [Header("ゲーム設定")]
    public int maxTurns = 5;
    public float turnTime = 10f;

    [Header("プレイヤーの情報")]
    public PlayerController player;

    // ★ static にすることで、シーンがリロードされてもデータが消えずに保持されます
    public static int hostMoney = 0;
    public static int playerTotalMoney = 0;
    public static int currentTurn = 1;

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

    [Header("部屋選択UIの設定（GameScene内）")]
    public GameObject selectionPanel; // 部屋を選ぶボタンが乗っているパネル
    public TextMeshProUGUI roleText;   // 「爆発させる部屋を選んでください」などのテキスト

    [Header("爆発エフェクト")]
    public GameObject explosionPrefab;
    public Transform[] roomPositions;

    private float currentTime;
    private bool isTimerRunning = false;
    private bool isSelectingRoom = false;
    private static int selectedExplodeRoom = 1;

    [Header("扉の設定")]
    public GameObject[] doorObjects;

    void Awake()
    {
        // 今回は DontDestroyOnLoad を使わず、シーンごとに GameManager をクリーンに動かします
    }

    void Start()
    {
        SetupGameScene();
    }

    void Update()
    {
        if (isSelectingRoom || !isTimerRunning) return;

        currentTime -= Time.deltaTime;

        if (timerText != null) timerText.text = $"残り時間: {Mathf.CeilToInt(currentTime)}秒";
        if (playerMoneyText != null) playerMoneyText.text = $"プレイヤー: {playerTotalMoney.ToString("#,0")}円";
        if (hostMoneyText != null) hostMoneyText.text = $"仕掛け人: {hostMoney.ToString("#,0")}円";

        if (currentTime <= 0)
        {
            TimeUp();
        }
    }

    void SetupGameScene()
    {
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

        GameObject[] foundDoors = GameObject.FindGameObjectsWithTag("Door");
        if (foundDoors.Length > 0)
        {
            doorObjects = foundDoors;
        }

        // 部屋選択ボタンの自動配線
        SetupRoomButtons();

        // ゲーム開始フェーズへ
        treasuresToSpawnNextTurn = initialSpawnCount;
        StartNextTurn();
    }

    void StartNextTurn()
    {
        Debug.Log("★現在のターン数: " + currentTurn);

        isSelectingRoom = true;
        isTimerRunning = false;

        // UI（パネルとテキスト）を表示する
        if (selectionPanel != null) selectionPanel.SetActive(true);
        if (roleText != null)
        {
            roleText.gameObject.SetActive(true);
            roleText.text = "爆発させる部屋を選んでください";
        }
    }

    // ボタンから自動、または手動で呼ばれる
    public void OnRoomSelectedButton(int roomNumber)
    {
        if (!isSelectingRoom) return;

        selectedExplodeRoom = roomNumber;
        Debug.Log($"仕掛け人が部屋 {roomNumber} を選択しました！ゲームスタート！");

        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (roleText != null) roleText.gameObject.SetActive(false);

        SpawnTreasuresAndStartGame();
        isSelectingRoom = false;
    }

    void SpawnTreasuresAndStartGame()
    {
        for (int i = 0; i < treasuresToSpawnNextTurn; i++)
        {
            float randomX = Random.Range(-spawnRadiusX, spawnRadiusX);
            float randomZ = Random.Range(-spawnRadiusZ, spawnRadiusZ);
            float randomY = Random.Range(0f, 2f);

            Vector3 spawnPos = (spawnPoint != null ? spawnPoint.position : Vector3.zero) + new Vector3(randomX, randomY, randomZ);
            TreasureData selectedTreasure = ChooseRandomTreasure();

            if (selectedTreasure != null && selectedTreasure.prefab != null)
            {
                GameObject boxObj = Instantiate(selectedTreasure.prefab, spawnPos, Quaternion.identity);
                TreasureBox box = boxObj.GetComponent<TreasureBox>();
                if (box != null) box.UpdateRoom(0);
            }
        }

        treasuresToSpawnNextTurn = additionalSpawnPerTurn;
        currentTime = turnTime;
        isTimerRunning = true;
    }

    void TimeUp()
    {
        isTimerRunning = false;
        if (timerText != null) timerText.text = "審判の刻...！";

        RoomDetector explodeRoomObj = null;
        RoomDetector[] allRooms = FindObjectsOfType<RoomDetector>();

        foreach (var room in allRooms)
        {
            if (room.roomNumber == selectedExplodeRoom)
            {
                explodeRoomObj = room;
                break;
            }
        }

        if (explodeRoomObj == null && allRooms.Length > 0)
        {
            explodeRoomObj = allRooms[0];
        }

        StartCoroutine(ExecuteExplosionSequence(explodeRoomObj));
    }

    private System.Collections.IEnumerator ExecuteExplosionSequence(RoomDetector room)
    {
        foreach (GameObject door in doorObjects)
        {
            if (door != null)
            {
                Animator anim = door.GetComponentInChildren<Animator>();
                if (anim != null) anim.SetBool("Closed", true);
            }
        }

        yield return new WaitForSeconds(1.0f);

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

        if (room != null && explosionPrefab != null)
        {
            Instantiate(explosionPrefab, room.transform.position, Quaternion.identity);
        }
        if (cameraShake != null) cameraShake.PlayShake(0.5f, 0.3f);

        if (player != null && room != null && (player.currentRoom == room.roomNumber || player.currentRoom == 0))
        {
            playerTotalMoney = 0;
            player.UpdateAnimation(false, true);

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.up * 15f + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)), ForceMode.Impulse);
            }
        }

        TreasureBox[] allTreasures = FindObjectsOfType<TreasureBox>();
        foreach (TreasureBox treasure in allTreasures)
        {
            if (treasure == null) continue;
            if (treasure.IsCarried() && player != null) treasure.currentRoom = player.currentRoom;

            if (room != null && (treasure.currentRoom == room.roomNumber || treasure.currentRoom == 0))
            {
                Rigidbody rb = treasure.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddForce(Vector3.up * 15f + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)), ForceMode.Impulse);
                }
            }
            else if (player != null && treasure.currentRoom == player.currentRoom && treasure.data != null)
            {
                playerTotalMoney += treasure.data.moneyAmount;
                Destroy(treasure.gameObject);
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
        if (availableTreasures == null || availableTreasures.Length == 0) return null;

        int totalWeight = 0;
        foreach (TreasureData treasure in availableTreasures)
        {
            if (treasure != null) totalWeight += treasure.spawnWeight;
        }

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

    void CheckTurnResult()
    {
        currentTurn++;

        if (currentTurn > maxTurns)
        {
            SceneManager.LoadScene("ResultScene");
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    void SetupRoomButtons()
    {
        if (selectionPanel == null) return;

        UnityEngine.UI.Button[] buttons = selectionPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);

        foreach (var btn in buttons)
        {
            btn.onClick.RemoveAllListeners();

            int roomNumber = 1;
            if (btn.name.Contains("1")) roomNumber = 1;
            else if (btn.name.Contains("2")) roomNumber = 2;
            else if (btn.name.Contains("3")) roomNumber = 3;
            else if (btn.name.Contains("4")) roomNumber = 4;

            btn.onClick.AddListener(() => OnRoomSelectedButton(roomNumber));
        }
    }

    public int GetFinalPlayerMoney()
    {
        return playerTotalMoney;
    }

    public void SetSelectedExplodeRoom(int roomNumber)
    {
        selectedExplodeRoom = roomNumber;
    }

    public void StartGameFromSelection()
    {
        currentTurn = 1;
        playerTotalMoney = 0;
        hostMoney = 0;
        SceneManager.LoadScene("GameScene");
    }
}