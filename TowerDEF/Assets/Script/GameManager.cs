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
    public GameObject CanvasPanel;
    public EsaSpawner es;

    public int totalWaves = 52; // 総ウェーブ数
    public float waveDuration = 10f; // ウェーブの持続時間
    public Text waveText; // ウェーブ情報のテキスト
    public int currentWave = 0; // 現在のウェーブ数
    private float waveTimer; // ウェーブのカウントダウンタイマー

    public bool isPaused = false; // ゲームが一時停止中かどうかを示すフラグ

    // ボタンの参照を設定
    public Button playButton;
    public Button pauseButton;

    public bool IsPaused // ゲームの一時停止状態を外部から取得するためのプロパティ
    {
        get { return isPaused; }
    }

    public delegate void OnWaveStart(); // ウェーブ開始時に発火するイベント
    public static event OnWaveStart WaveStarted; // イベント

    public delegate void SeasonChangeHandler(GameManager.Season newSeason);
    public static event SeasonChangeHandler SeasonChanged;

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
    public List<int> enemyDays = new List<int> { 5, 9, 13, 18, 22, 26, 31, 35, 39, 44, 48, 52}; // 敵が攻めてくる特定の日

    [SerializeField]
    public TextMeshProUGUI WaveText; // TextMeshProで表示するウェーブ情報

    // ランダムな魚のPrefabリスト
    [SerializeField] private List<GameObject> fishPrefabs;

    // 季節の列挙型
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public Season currentSeason = Season.Spring; // 初期は春の設定
    private Season? previousSeason = null; // 前のシーズンを保持する変数

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
        CanvasPanel.SetActive(true);
        // ボタンのクリックイベントに関数を設定
        playButton.onClick.AddListener(OnPlayButtonClick);
        pauseButton.onClick.AddListener(OnPauseButtonClick);

        // 餃の初期在庫を0に設定
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
        //finventory[ResourceFishType.Kani] = 3;
        //finventory[ResourceFishType.Tyoutyou] = 2;

        UpdateResourceUI(); // リソースの初期UIを更新
    }

    private void Update()
    {

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
        waveText.text = currentWave.ToString() + "タイド";
    }

    // 一時停止と再開を切り替えるメソッド
    public void TogglePause()
    {
        isPaused = !isPaused; // 現在の一時停止状態を反転
        ApplyPauseState(); // 状態に応じた処理を適用
        Debug.Log("Game " + (isPaused ? "Paused" : "Resumed"));
    }
    public void OnPlayButtonClick()
    {
        if (isPaused)  // 一時停止状態なら解除
        {
            TogglePause();
        }
    }

    // 一時停止ボタンがクリックされたとき
    public void OnPauseButtonClick()
    {
        if (!isPaused)  // ゲームが再生中なら一時停止
        {
            TogglePause();
        }
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

        // 命作キャラの処理（移動と攻撃を制御）
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

        Debug.Log("Game " + (isPaused ? "Paused" : "Resumed"));
    }

    // 新しいウェーブを開始するメソッド
    public void StartWave()
    {
        ForceResumeGame();  // ウェーブがスタートしたら一時停止解除
        nextwavebutton.SetActive(false); // 次のウェーブボタンを非表示
        currentWave++; // 現在のウェーブ数を増加
        Debug.Log("Current Season: " + currentSeason); // 現在のシーズンをデバッグログに表示
        waveTimer = waveDuration; // ウェーブのタイマーをリセット
        Debug.Log("Wave " + currentWave + " started");
        

        WaveStarted?.Invoke(); // ウェーブ開始イベントを発火

        // 特定のウェーブ番号で敵が攻めてくることを表示
        /*if (currentWave % 3 == 0 || currentWave % 4 == 0)
        {
            Debug.Log("Wave " + currentWave + " has enemies attacking");
        }*/

        ConvertFeedToFish();

        // 13ウェーブごとに季節を変える
        if (currentWave % 13 == 1)
        {
            ChangeSeason();
        }

        ResetSeasonalEffects(); // 季節のバフとデバフをリセット
        ApplySeasonalEffects(); // ウェーブ開始時に季節の効果を適用
        es.currentEsaCount = 0; //餌場の制限をリセット
    }

    // 季節を変えるメソッド
    private void ChangeSeason()
    {
        if (currentSeason != previousSeason)
        {
            previousSeason = currentSeason;
            currentSeason = (Season)(((int)currentSeason + 1) % System.Enum.GetValues(typeof(Season)).Length);
            Debug.Log("Season changed to " + currentSeason);
            // 季節変更イベントを発火
            SeasonChanged?.Invoke(currentSeason);
        }
    }

    // 季節のバフとデバフをリセットするメソッド
    private void ResetSeasonalEffects()
    {
        GameObject[] allCreatures = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject creature in allCreatures)
        {
            ISeasonEffect seasonEffect = creature.GetComponent<ISeasonEffect>();
            if (seasonEffect != null)
            {
                seasonEffect.ResetSeasonEffect();
            }
        }
    }

    // 季節の効果を全生物に適用するメソッド
    private void ApplySeasonalEffects()
    {
        GameObject[] allCreatures = GameObject.FindGameObjectsWithTag("Ally");
        foreach (GameObject creature in allCreatures)
        {
            ISeasonEffect seasonEffect = creature.GetComponent<ISeasonEffect>();
            if (seasonEffect != null)
            {
                seasonEffect.ApplySeasonEffect(currentSeason);
            }
        }
    }

    void ConvertFeedToFish()
    {
        // "esa" タグを持つすべてのオブジェクトを取得
        GameObject[] feeds = GameObject.FindGameObjectsWithTag("esa");

        foreach (GameObject feed in feeds)
        {
            // 餃オブジェクトをランダムな魚に変換
            int randomIndex = Random.Range(0, fishPrefabs.Count);
            GameObject fish = Instantiate(fishPrefabs[randomIndex], feed.transform.position, feed.transform.rotation);

            // 餃オブジェクトを削除（必要であれば）
            Destroy(feed);
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
    // FeedUIManagerから呼び出され、選択された餃タイプを設定するメソッド
    public void SetSelectedFeedType(ResourceType feedType)
    {
        SelectedFeedType = feedType;
        Debug.Log("Selected Feed Type: " + SelectedFeedType);
    }

    //FishUIManagerから呼び出され、選択した魚タイプを設定するメソッド
    public void SetSelectedFishType(ResourceFishType fishType)
    {
        SelectedFishType = fishType;
    }

    // リソースUIを更新するメソッド
    public void UpdateResourceUI()
    {
        okiaMiText.text = ": " + inventory[ResourceType.OkiaMi].ToString();
        benthosText.text = " :" + inventory[ResourceType.Benthos].ToString();
        planktonText.text = ": " + inventory[ResourceType.Plankton].ToString();
        feedAText.text = ": " + inventory[ResourceType.FeedA].ToString();
        feedBText.text = ": " + inventory[ResourceType.FeedB].ToString();
        feedCText.text = ": " + inventory[ResourceType.FeedC].ToString();
        urokoText.text = ": " + inventory[ResourceType.Uroko].ToString();
        pearlText.text = ": " + inventory[ResourceType.Pearl].ToString();
        Kani.text = "x" + finventory[ResourceFishType.Kani].ToString();
        Tyoutyou.text = "x" + finventory[ResourceFishType.Tyoutyou].ToString();
        Kaisou.text = "x" + finventory[ResourceFishType.Kaisou].ToString();
        Syako.text = "x" + finventory[ResourceFishType.Syako].ToString();
        Koban.text = "x" + finventory[ResourceFishType.Koban].ToString();
        Teppou.text = "x" + finventory[ResourceFishType.Teppou].ToString();
        Manta.text = "x" + finventory[ResourceFishType.Manta].ToString();
        Uni.text = "x" + finventory[ResourceFishType.Uni].ToString();
    }

    // ゲームオーバー処理
    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Debug.Log("Game Over");
    }
}
