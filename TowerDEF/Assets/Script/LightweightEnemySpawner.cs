using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightweightEnemySpawner : MonoBehaviour
{
    // ウェーブごとの敵情報を手動で設定する
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab; // 出現させる敵のPrefab
        public int enemyCount; // 出現させる敵の数
        public int waveNumber; // 出現するウェーブ番号
    }

    [SerializeField] private Wave[] waves; // 各ウェーブの情報をインスペクターで設定
    [SerializeField] private float spawnInterval = 0.5f; // 敵をスポーンする間隔時間
    [SerializeField] private int poolSize = 10; // 各敵プールのデフォルトサイズ

    private GameManager gameManager;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        // GameManagerの参照を取得
        gameManager = FindObjectOfType<GameManager>();
        InitializeObjectPool();
    }

    void InitializeObjectPool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (Wave wave in waves)
        {
            string key = GetPoolKey(wave);
            if (!poolDictionary.ContainsKey(key))
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                int poolSizeForWave = Mathf.Max(wave.enemyCount, poolSize); // 必要な数に応じてプールサイズを設定
                for (int i = 0; i < poolSizeForWave; i++)
                {
                    GameObject obj = Instantiate(wave.enemyPrefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                poolDictionary.Add(key, objectPool);
            }
        }
    }

    public void StartWave(int waveIndex)
    {
        StartCoroutine(SpawnWave(waveIndex));
    }

    IEnumerator SpawnWave(int waveIndex)
    {
        foreach (Wave wave in waves)
        {
            if (wave.waveNumber == waveIndex)
            {
                for (int i = 0; i < wave.enemyCount; i++)
                {
                    GameObject enemy = GetPooledObject(wave);
                    if (enemy != null)
                    {
                        enemy.transform.position = transform.position;
                        enemy.transform.rotation = transform.rotation;
                        enemy.SetActive(true);
                    }
                    yield return new WaitForSeconds(spawnInterval); // 軽量にするため少し待機
                }
            }
        }
    }

    GameObject GetPooledObject(Wave wave)
    {
        string key = GetPoolKey(wave);
        if (poolDictionary.ContainsKey(key))
        {
            GameObject obj = poolDictionary[key].Dequeue();
            poolDictionary[key].Enqueue(obj);
            return obj;
        }
        return null;
    }

    string GetPoolKey(Wave wave)
    {
        return wave.enemyPrefab.name + "_Wave" + wave.waveNumber;
    }
}
