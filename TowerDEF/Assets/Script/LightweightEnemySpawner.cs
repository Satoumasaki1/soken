using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightweightEnemySpawner : MonoBehaviour
{
    // �E�F�[�u���Ƃ̓G�����蓮�Őݒ肷��
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab; // �o��������G��Prefab
        public int enemyCount; // �o��������G�̐�
        public int waveNumber; // �o������E�F�[�u�ԍ�
    }

    [SerializeField] private Wave[] waves; // �e�E�F�[�u�̏����C���X�y�N�^�[�Őݒ�
    [SerializeField] private float spawnInterval = 0.5f; // �G���X�|�[������Ԋu����
    [SerializeField] private int poolSize = 10; // �e�G�v�[���̃f�t�H���g�T�C�Y

    private GameManager gameManager;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        // GameManager�̎Q�Ƃ��擾
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
                int poolSizeForWave = Mathf.Max(wave.enemyCount, poolSize); // �K�v�Ȑ��ɉ����ăv�[���T�C�Y��ݒ�
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
                    yield return new WaitForSeconds(spawnInterval); // �y�ʂɂ��邽�ߏ����ҋ@
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
