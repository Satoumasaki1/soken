using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSwarmManager : MonoBehaviour
{
    [Header("Fish Swarm Settings")]
    public GameObject fishSwarmPrefab; // ���Q�̃v���n�u
    public GameObject fishPrefab; // ���̂̃v���n�u
    public int fishCount = 10; // ���̐�
    public float fishSpacing = 1f; // �����m�̋���
    public List<Transform> lines; // ���C�����X�g
    public float movementSpeed = 5f; // ���Q�S�̂̈ړ����x
    public float swarmDuration = 10f; // ���Q���A�N�e�B�u�Ȋ���
    public int damagePerSecond = 1; // ���Q���^����_���[�W��
    public float damageRadius = 2f; // �_���[�W�͈�

    [Header("Game State Settings")]
    public string currentSeason = "Autumn"; // ���݂̋G��
    public int currentWave; // ���݂̃E�F�[�u�ԍ�
    public List<int> swarmTriggerWaves = new List<int> { 5, 9, 13, 18, 22, 26, 31, 35, 39, 44, 48, 52 };

    private void Update()
    {
        Debug.Log($"Current Season: {currentSeason}, Current Wave: {currentWave}");

        // �G�߂��H�Ŏw�肳�ꂽ�E�F�[�u�̏ꍇ�ɋ��Q�𔭐�
        if (currentSeason == "Autumn" && swarmTriggerWaves.Contains(currentWave) && !IsSwarmActive())
        {
            Debug.Log("Spawning Fish Swarm!");
            StartCoroutine(SpawnFishSwarm());
        }
    }

    private bool IsSwarmActive()
    {
        // ���Q�����ɃA�N�e�B�u���ǂ������m�F
        return GameObject.FindGameObjectWithTag("FishSwarm") != null;
    }

    private IEnumerator SpawnFishSwarm()
    {
        // �����_���ȃ��C����I��
        if (lines.Count == 0)
        {
            Debug.LogError("Lines list is empty! Please assign lines.");
            yield break;
        }

        Transform selectedLine = lines[Random.Range(0, lines.Count)];
        Debug.Log($"Selected Line: {selectedLine.name}");

        // ���Q�I�u�W�F�N�g�𐶐�
        GameObject fishSwarm = new GameObject("FishSwarm");
        fishSwarm.tag = "FishSwarm";
        fishSwarm.transform.position = selectedLine.position;
        fishSwarm.transform.rotation = Quaternion.LookRotation(selectedLine.forward);

        // ���𐶐����Ĕz�u
        for (int i = 0; i < fishCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-fishSpacing, fishSpacing),
                Random.Range(-fishSpacing, fishSpacing),
                Random.Range(-fishSpacing, fishSpacing)
            );

            GameObject fish = Instantiate(fishPrefab, fishSwarm.transform.position + offset, Quaternion.identity, fishSwarm.transform);
        }

        // ���Q�����C���ɉ����Ĉړ�
        float elapsedTime = 0f;
        while (elapsedTime < swarmDuration)
        {
            fishSwarm.transform.position += selectedLine.forward * movementSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��莞�Ԍ�ɋ��Q���폜
        Destroy(fishSwarm);
    }
}
