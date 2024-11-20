using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnWave
    {
        public int day; // 出現する日数
        public List<GameObject> enemyTypes; // この日に出現する敵のリスト
    }

    public List<SpawnWave> spawnWaves; // 各日数ごとの敵の種類
    public Transform[] spawnPoints; // 敵がスポーンする位置の配列
    public float spawnInterval = 1.0f; // 敵をスポーンさせる間隔
    public int enemiesPerWave = 5; // 各ウェーブでスポーンする敵の数

    private int currentEnemyCount = 0;
    private float timeSinceLastSpawn;

    private void Update()
    {
        Debug.Log("Update メソッドが呼び出されました");
        int currentDay = GameManager.Instance.currentDay;
        Debug.Log("現在の日数: " + currentDay);
        if (currentDay > 0 && currentDay % 4 == 0)
        {
            timeSinceLastSpawn += Time.deltaTime;
            Debug.Log("timeSinceLastSpawn の値: " + timeSinceLastSpawn);
            if (timeSinceLastSpawn >= spawnInterval && currentEnemyCount < enemiesPerWave)
            {
                Debug.Log("敵をスポーンするウェーブ: " + currentDay);
                SpawnEnemiesForDay(currentDay);
                timeSinceLastSpawn = 0f;
            }
        }
    }

    private void SpawnEnemiesForDay(int day)
    {
        Debug.Log("SpawnEnemiesForDay メソッドが呼び出されました。日数: " + day);
        foreach (var wave in spawnWaves)
        {
            if (wave.day == day)
            {
                foreach (GameObject enemy in wave.enemyTypes)
                {
                    SpawnEnemy(enemy);
                }
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        Debug.Log("SpawnEnemy メソッドが呼び出されました: " + enemyPrefab.name);
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("スポーンポイントが指定されていません");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("敵がスポーンしました: " + enemyPrefab.name + " at " + spawnPoint.position);
        currentEnemyCount++;
    }
}
