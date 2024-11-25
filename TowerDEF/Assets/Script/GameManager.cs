using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // ゲームマネージャのインスタンス
    public int baseHealth = 100; // 拠点の初期体力
    public GameObject gameOverPanel; // ゲームオーバーパネル
    public GameObject nextwavebutton; // 次のウェーブの開始ボタン

    public int totalWaves = 50; // 総ウェーブ数
    public float waveDuration = 10f; // ウェーブの持続時間
    public Text waveText; // ウェーブ情報のテキスト
    public int currentWave = 0; // 現在のウェーブ数
    private float waveTimer; // ウェーブのカウントダウンタイマー

    public bool isPaused = false; // ゲームが一時停止中かどうかを示すフラグ

    public bool IsPaused // ゲームの一時停止状態を外部から取得するためのプロパティ
    {
        get { return isPaused; }
    }

    public delegate void OnWaveStart(); // ウェーブ開始時に発火するイベント
    public static event OnWaveStart WaveStarted; // イベント

    public enum ResourceType { OkiaMi, Benthos, Plankton, FeedA, FeedB, FeedC, Uroko, Pearl } // リソースの種類

    public enum ResourceFishType { Kani, Tyoutyou, Kaisou, Syako, Koban, Teppou, Manta, Uni } // リソースの種類
    // 各リソースの在庫を管理するDictionary
    public Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>();

    public Dictionary<ResourceFishType, int> finventory = new Dictionary<ResourceFishType, int>();
    public ResourceType? SelectedFeedType { get; set; } = ResourceType.OkiaMi; // デフォルトでOkiaMiを設定
    public ResourceFishType? SelectedFishType { get; set; } = null;

    // 各リソースのUIテキスト
    public TextMeshProUGUI okiaMiText, benthosText, planktonText, feedAText, feedBText, feedCText, urokoText, pearlText, Kani, Tyoutyou, Kaisou, Syako, Koban, Teppou, Manta, Uni;

    public int currentDay = 0; // 現在の日数
    public List<int> enemyDays = new List<int> { 4, 8, 12, 16, 20 }; // 敵が攻めてくる特定の日

    [SerializeField]
    public TextMeshProUGUI WaveText; // TextMeshProで表示するウェーブ情報

    private void Awake()
    {
        // Singletonパターン（インスタンスがすでに存在しない場合のみ新しく作成）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は破棄
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject); // シーンが変わっても破棄しない
        gameOverPanel.SetActive(false); // ゲームオーバーパネルを非表示
        nextwavebutton.SetActive(false); // 次のウェーブボタンを非表示
        ForceResumeGame();  // 初回ウェーブ開始時の一時停止解除を強制
        StartWave();  // 最初のウェーブ開始

        // 餌の初期在庫を0に設定
        foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType)))
        {
            inventory[resource] = 0;
        }

        // 魚の初期在庫を0に設定
        foreach (ResourceFishType resource in System.Enum.GetValues(typeof(ResourceFishType)))
        {
            finventory[resource] = 0;
        }

        // オキアミ、ベントス、プランクトンの在庫を3に設定
        inventory[ResourceType.OkiaMi] = 3;
        inventory[ResourceType.Benthos] = 3;
        inventory[ResourceType.Plankton] = 3;

        //動作確認のためにカニの在庫を3に設定
        finventory[ResourceFishType.Kani] = 3;

        UpdateResourceUI(); // リソースの初期UIを更新
    }

    private void Update()
    {
        // 「E」キーでゲームの一時停止・再開を切り替え
        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePause();
        }

        // ゲームが一時停止中でない場合、ウェーブのタイマーを更新
        if (!isPaused && currentWave < totalWaves)
        {
            waveTimer -= Time.deltaTime; // 時間の経過でタイマーを減らす
            if (waveTimer <= 0)
            {
                ShowNextWaveButton();  // ウェーブ終了後に次のウェーブボタンを表示
            }
        }

        // 特定の日に敵が攻めてくる
        if (enemyDays.Contains(currentDay))
        {
            Debug.Log("敵が攻めてくる日です: Day " + currentDay);
        }
    }

    // 一時停止と再開を切り替えるメソッド
    public void TogglePause()
    {
        isPaused = !isPaused; // 現在の一時停止状態を反転
        ApplyPauseState(); // 状態に応じた処理を適用
        Debug.Log("ゲームは " + (isPaused ? "一時停止中" : "再開中"));
    }

    // 一時停止を強制的に解除するメソッド
    private void ForceResumeGame()
    {
        isPaused = false; // 一時停止状態を解除
        ApplyPauseState(); // 再開状態に変更
    }

    // 一時停止の状態に基づいてゲームオブジェクトを一時停止・再開する処理
    private void ApplyPauseState()
    {
        // 敵の処理
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Enemyスクリプトを無効化
            MonoBehaviour enemyScript = enemy.GetComponent<MonoBehaviour>();
            if (enemyScript != null)
            {
                enemyScript.enabled = !isPaused; // 一時停止中は無効化
            }

            // NavMeshAgentの移動を一時停止・再開
            NavMeshAgent enemyAgent = enemy.GetComponent<NavMeshAgent>();
            if (enemyAgent != null)
            {
                enemyAgent.isStopped = isPaused; // 一時停止中は移動を停止

                // 一時停止中に速度をリセット
                if (isPaused)
                {
                    enemyAgent.velocity = Vector3.zero;
                }
            }
        }

        // 拠点の処理（敵と同様に移動を一時停止）
        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
        foreach (GameObject baseObj in bases)
        {
            // Baseスクリプトを無効化
            MonoBehaviour baseScript = baseObj.GetComponent<MonoBehaviour>();
            if (baseScript != null)
            {
                baseScript.enabled = !isPaused;
            }

            // NavMeshAgentの移動を一時停止・再開
            NavMeshAgent baseAgent = baseObj.GetComponent<NavMeshAgent>();
            if (baseAgent != null)
            {
                baseAgent.isStopped = isPaused; // 一時停止中は移動を停止

                // 一時停止中に速度をリセット
                if (isPaused)
                {
                    baseAgent.velocity = Vector3.zero;
                }
            }
        }

        // 味方キャラクターの処理（移動と攻撃を制御）
        GameObject[] allys = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject ally in allys)
        {
            // Kanisanスクリプトを無効化
            /*Kanisan kanisanScript = ally.GetComponent<Kanisan>();
            if (kanisanScript != null)
            {
                kanisanScript.enabled = !isPaused; // 一時停止中は無効化
            }*/

            // NavMeshAgentの移動を一時停止・再開
            NavMeshAgent allyAgent = ally.GetComponent<NavMeshAgent>();
            if (allyAgent != null)
            {
                allyAgent.isStopped = isPaused; // 一時停止中は移動を停止

                // 一時停止中に速度をリセット
                if (isPaused)
                {
                    allyAgent.velocity = Vector3.zero;
                }
            }
        }



        Debug.Log("ゲームは " + (isPaused ? "一時停止中" : "再開中"));
    }

    // 新しいウェーブを開始するメソッド
    public void StartWave()
    {
        ForceResumeGame();  // ウェーブがスタートしたら一時停止解除
        nextwavebutton.SetActive(false); // 次のウェーブボタンを非表示
        currentWave++; // 現在のウェーブ数を増加
        waveTimer = waveDuration; // ウェーブのタイマーをリセット
        Debug.Log("ウェーブ " + currentWave + " が開始されました");

        WaveStarted?.Invoke(); // ウェーブ開始イベントを発火

        // 特定のウェーブ番号で敵が攻めてくることを表示
        if (currentWave % 3 == 0 || currentWave % 4 == 0)
        {
            Debug.Log("ウェーブ " + currentWave + " で敵が攻めてきます");
        }
    }

    // 次のウェーブボタンを表示するメソッド
    public void ShowNextWaveButton()
    {
        isPaused = true; // 一時停止状態にする
        ApplyPauseState(); // ゲームの状態を更新
        nextwavebutton.SetActive(true); // 次のウェーブボタンを表示
    }

    // リソースを追加するメソッド
    public void AddResource(GameManager.ResourceType resourceType, int amount)
    {
        if (inventory.ContainsKey(resourceType))
        {
            inventory[resourceType] += amount;
            Debug.Log($"{resourceType} の在庫: {inventory[resourceType]}");
            UpdateResourceUI(); // UIを更新
        }
    }
    // FeedUIManagerから呼び出され、選択された餌タイプを設定するメソッド
    public void SetSelectedFeedType(ResourceType feedType)
    {
        SelectedFeedType = feedType;
        Debug.Log("GameManagerで選択された餌タイプ: " + SelectedFeedType);
    }

    //FishUIManagerから呼び出され、選択した魚タイプを設定するメソッド
    public void SetSelectedFishType(ResourceFishType fishType)
    {
        SelectedFishType = fishType;
    }

    // リソースUIを更新するメソッド
    public void UpdateResourceUI()
    {
        okiaMiText.text = "OkiaMi: " + inventory[ResourceType.OkiaMi];
        benthosText.text = "Benthos: " + inventory[ResourceType.Benthos];
        planktonText.text = "Plankton: " + inventory[ResourceType.Plankton];
        feedAText.text = "Feed A: " + inventory[ResourceType.FeedA];
        feedBText.text = "Feed B: " + inventory[ResourceType.FeedB];
        feedCText.text = "Feed C: " + inventory[ResourceType.FeedC];
        urokoText.text = "Uroko: " + inventory[ResourceType.Uroko];
        pearlText.text = "Pearl: " + inventory[ResourceType.Pearl];
        Kani.text ="×" + finventory[ResourceFishType.Kani].ToString();
        Tyoutyou.text = "×" + finventory[ResourceFishType.Tyoutyou].ToString();
        Kaisou.text = "×" + finventory[ResourceFishType.Kaisou].ToString();
        Syako.text = "×" + finventory[ResourceFishType.Syako].ToString();
        Koban.text = "×" + finventory[ResourceFishType.Koban].ToString();
        Teppou.text = "×" + finventory[ResourceFishType.Teppou].ToString();
        Manta.text = "×" + finventory[ResourceFishType.Manta].ToString();
        Uni.text = "×" + finventory[ResourceFishType.Uni].ToString();
    }

    // ゲームオーバー処理
    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Debug.Log("ゲームオーバー");
    }
}
