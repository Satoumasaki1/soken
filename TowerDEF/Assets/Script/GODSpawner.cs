using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

public class GODSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WavePattern
    {
        public string name;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<WavePattern> patterns;
    }

    public List<Wave> waves;
    public Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            // ゲームマネージャから現在のウェーブを取得
            currentWaveIndex = GameManager.Instance.currentWave;
        }
        // スポーンプロセスを開始
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        while (currentWaveIndex < waves.Count && (GameManager.Instance == null || currentWaveIndex < GameManager.Instance.totalWaves))
        {
            if (!isSpawning)
            {
                yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
                currentWaveIndex++;
            }
            yield return null;
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;
        Debug.Log("開始中のウェーブ: " + wave.waveName);

        foreach (WavePattern pattern in wave.patterns)
        {
            for (int i = 0; i < pattern.enemyCount; i++)
            {
                SpawnEnemy(pattern.enemyPrefab);
                yield return new WaitForSeconds(pattern.spawnInterval);
            }
        }

        isSpawning = false;
    }

    void SpawnEnemy(GameObject enemy)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
    }
}

// Unityインスペクターでの使用方法の例:
// 1. シーン内に空のGameObjectを作成し、「GODSpawner」と名前を付けます。
// 2. このGODSpawnerスクリプトをアタッチします。
// 3. 「spawnPoints」配列にスポーンポイントを割り当てます。
// 4. インスペクターでウェーブの設定を作成:
//    - 「waves」に3つのウェーブエントリを追加します。
//    - 各ウェーブに対して敵パターン、敵プレハブ、数、インターバルを設定します。
