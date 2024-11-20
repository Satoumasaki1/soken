using System.Collections;
using UnityEngine;
using System.Linq;

public class TekisupoSpawner : MonoBehaviour
{
    // �E�F�[�u�f�[�^�̎Q�ƁiScriptableObject���g���ăE�F�[�u���`�j
    public WaveData[] waves;
    public Transform[] spawnPoints; // �����̃X�|�[���|�C���g����I��

    // GameManager���猻�݂̃E�F�[�u���Q�Ƃ���
    private GameManager gameManager;

    // �X�|�[�����������̓����i�E�F�[�u�ԍ����w��j
    public int[] specialSpawnWaves = { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 50 }; // ����̃E�F�[�u�ԍ����w��

    void Start()
    {
        // GameManager�̎Q�Ƃ��擾
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log("GameManager�̎Q�Ƃ��擾���܂����B");
            // �E�F�[�u���X�V���ꂽ�Ƃ���StartWave���Ăяo��
            GameManager.WaveStarted += StartWave;
            // �ŏ��̃E�F�[�u���J�n
            StartWave();
        }
        else
        {
            Debug.LogError("GameManager�̎Q�Ƃ��擾�ł��܂���ł����BGameManager���V�[���ɑ��݂��邩�m�F���Ă��������B");
        }
    }

    public void StartWave()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager��������܂���B�E�F�[�u���J�n�ł��܂���B");
            return;
        }

        // ����̓����ɃX�|�[������ꍇ�̂ݓG���X�|�[��
        if (specialSpawnWaves.Contains(gameManager.currentWave))
        {
            Debug.Log($"���ʂȃE�F�[�u {gameManager.currentWave} ���J�n����܂����I");
            if (gameManager.currentWave - 1 < waves.Length) { StartCoroutine(SpawnSpecialWave(waves[gameManager.currentWave - 1])); } else { Debug.LogError("�w�肳�ꂽ�E�F�[�u�f�[�^�����݂��܂���Bwaves�z��͈̔͂𒴂��Ă��܂��B"); }
        }
        else
        {
            Debug.Log($"�E�F�[�u {gameManager.currentWave} �͓���̓����ł͂Ȃ����߁A�G�̓X�|�[�����܂���B");
        }
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        Debug.Log($"�E�F�[�u {gameManager.currentWave} �̓G���X�|�[�����܂��B");
        foreach (var enemyInfo in wave.enemies)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                // ���p�\�ȃX�|�[���|�C���g���烉���_���ɑI��
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Debug.Log($"�G {enemyInfo.enemyPrefab.name} ���X�|�[�����܂��B �X�|�[���|�C���g: {spawnPoint.position}");
                Instantiate(enemyInfo.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(1.0f); // �G�̃X�|�[���Ԋu�i�Œ�l�j
            }
        }
        Debug.Log($"�E�F�[�u {gameManager.currentWave} �̓G�̃X�|�[�����������܂����B");
    }

    IEnumerator SpawnSpecialWave(WaveData wave)
    {
        Debug.Log($"���ʂȃE�F�[�u {gameManager.currentWave} �̓G���X�|�[�����܂��B");
        foreach (var enemyInfo in wave.enemies)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                // ���p�\�ȃX�|�[���|�C���g���烉���_���ɑI��
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Debug.Log($"���ʂȓG {enemyInfo.enemyPrefab.name} ���X�|�[�����܂��B �X�|�[���|�C���g: {spawnPoint.position}");
                Instantiate(enemyInfo.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(1.0f); // �G�̃X�|�[���Ԋu�i�Œ�l�j
            }
        }
        Debug.Log($"���ʂȃE�F�[�u {gameManager.currentWave} �̓G�̃X�|�[�����������܂����B");
    }

    private void OnDestroy()
    {
        // �C�x���g�w�ǂ�����
        if (gameManager != null)
        {
            GameManager.WaveStarted -= StartWave;
        }
    }
}
