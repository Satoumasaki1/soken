using System.Collections;
using UnityEngine;
using System.Linq;

public class TekisupoSpawner : MonoBehaviour
{
    // ウェーブデータの参照（ScriptableObjectを使ってウェーブを定義）
    public WaveData[] waves;
    public Transform[] spawnPoints; // 複数のスポーンポイントから選択

    // GameManagerから現在のウェーブを参照する
    private GameManager gameManager;

    // スポーンさせる特定の日数（ウェーブ番号を指定）
    public int[] specialSpawnWaves = { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 50 }; // 特定のウェーブ番号を指定

    void Start()
    {
        // GameManagerの参照を取得
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log("GameManagerの参照を取得しました。");
            // ウェーブが更新されたときにStartWaveを呼び出す
            GameManager.WaveStarted += StartWave;
            // 最初のウェーブを開始
            StartWave();
        }
        else
        {
            Debug.LogError("GameManagerの参照を取得できませんでした。GameManagerがシーンに存在するか確認してください。");
        }
    }

    public void StartWave()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManagerが見つかりません。ウェーブを開始できません。");
            return;
        }

        // 特定の日数にスポーンする場合のみ敵をスポーン
        if (specialSpawnWaves.Contains(gameManager.currentWave))
        {
            Debug.Log($"特別なウェーブ {gameManager.currentWave} が開始されました！");
            if (gameManager.currentWave - 1 < waves.Length) { StartCoroutine(SpawnSpecialWave(waves[gameManager.currentWave - 1])); } else { Debug.LogError("指定されたウェーブデータが存在しません。waves配列の範囲を超えています。"); }
        }
        else
        {
            Debug.Log($"ウェーブ {gameManager.currentWave} は特定の日数ではないため、敵はスポーンしません。");
        }
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        Debug.Log($"ウェーブ {gameManager.currentWave} の敵をスポーンします。");
        foreach (var enemyInfo in wave.enemies)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                // 利用可能なスポーンポイントからランダムに選択
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Debug.Log($"敵 {enemyInfo.enemyPrefab.name} をスポーンします。 スポーンポイント: {spawnPoint.position}");
                Instantiate(enemyInfo.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(1.0f); // 敵のスポーン間隔（固定値）
            }
        }
        Debug.Log($"ウェーブ {gameManager.currentWave} の敵のスポーンが完了しました。");
    }

    IEnumerator SpawnSpecialWave(WaveData wave)
    {
        Debug.Log($"特別なウェーブ {gameManager.currentWave} の敵をスポーンします。");
        foreach (var enemyInfo in wave.enemies)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                // 利用可能なスポーンポイントからランダムに選択
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Debug.Log($"特別な敵 {enemyInfo.enemyPrefab.name} をスポーンします。 スポーンポイント: {spawnPoint.position}");
                Instantiate(enemyInfo.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(1.0f); // 敵のスポーン間隔（固定値）
            }
        }
        Debug.Log($"特別なウェーブ {gameManager.currentWave} の敵のスポーンが完了しました。");
    }

    private void OnDestroy()
    {
        // イベント購読を解除
        if (gameManager != null)
        {
            GameManager.WaveStarted -= StartWave;
        }
    }
}
