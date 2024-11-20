using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnWave
    {
        public int day; // �o���������
        public List<GameObject> enemyTypes; // ���̓��ɏo������G�̃��X�g
    }

    public List<SpawnWave> spawnWaves; // �e�������Ƃ̓G�̎��
    public Transform[] spawnPoints; // �G���X�|�[������ʒu�̔z��
    public float spawnInterval = 1.0f; // �G���X�|�[��������Ԋu
    public int enemiesPerWave = 5; // �e�E�F�[�u�ŃX�|�[������G�̐�

    private int currentEnemyCount = 0;
    private float timeSinceLastSpawn;

    private void Update()
    {
        Debug.Log("Update ���\�b�h���Ăяo����܂���");
        int currentDay = GameManager.Instance.currentDay;
        Debug.Log("���݂̓���: " + currentDay);
        if (currentDay > 0 && currentDay % 4 == 0)
        {
            timeSinceLastSpawn += Time.deltaTime;
            Debug.Log("timeSinceLastSpawn �̒l: " + timeSinceLastSpawn);
            if (timeSinceLastSpawn >= spawnInterval && currentEnemyCount < enemiesPerWave)
            {
                Debug.Log("�G���X�|�[������E�F�[�u: " + currentDay);
                SpawnEnemiesForDay(currentDay);
                timeSinceLastSpawn = 0f;
            }
        }
    }

    private void SpawnEnemiesForDay(int day)
    {
        Debug.Log("SpawnEnemiesForDay ���\�b�h���Ăяo����܂����B����: " + day);
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
        Debug.Log("SpawnEnemy ���\�b�h���Ăяo����܂���: " + enemyPrefab.name);
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("�X�|�[���|�C���g���w�肳��Ă��܂���");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("�G���X�|�[�����܂���: " + enemyPrefab.name + " at " + spawnPoint.position);
        currentEnemyCount++;
    }
}
