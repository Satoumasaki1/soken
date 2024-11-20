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
            // �Q�[���}�l�[�W�����猻�݂̃E�F�[�u���擾
            currentWaveIndex = GameManager.Instance.currentWave;
        }
        // �X�|�[���v���Z�X���J�n
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
        Debug.Log("�J�n���̃E�F�[�u: " + wave.waveName);

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

// Unity�C���X�y�N�^�[�ł̎g�p���@�̗�:
// 1. �V�[�����ɋ��GameObject���쐬���A�uGODSpawner�v�Ɩ��O��t���܂��B
// 2. ����GODSpawner�X�N���v�g���A�^�b�`���܂��B
// 3. �uspawnPoints�v�z��ɃX�|�[���|�C���g�����蓖�Ă܂��B
// 4. �C���X�y�N�^�[�ŃE�F�[�u�̐ݒ���쐬:
//    - �uwaves�v��3�̃E�F�[�u�G���g����ǉ����܂��B
//    - �e�E�F�[�u�ɑ΂��ēG�p�^�[���A�G�v���n�u�A���A�C���^�[�o����ݒ肵�܂��B
