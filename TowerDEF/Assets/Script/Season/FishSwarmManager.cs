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
    public int[] waveNumbers = { 5, 9, 13, 18, 22, 26, 31, 35, 39, 44, 48, 52 }; // ���Q�𔭐�������E�F�[�u�ԍ�

    private bool hasSwarmSpawnedThisWave = false; // ���̃E�F�[�u�ŋ��Q������������

    private void Update()
    {
        // �G�߂��H�Ŏw��E�F�[�u�Ȃ狛�Q�𔭐�������
        if (currentSeason == "Autumn" && ShouldSpawnSwarm())
        {
            StartCoroutine(SpawnFishSwarm());
            hasSwarmSpawnedThisWave = true; // ���̃E�F�[�u�ł͋��Q�𔭐�������
        }
    }

    private bool ShouldSpawnSwarm()
    {
        // �w�肳�ꂽ�E�F�[�u�ԍ��ŋ��Q���܂��������Ă��Ȃ��ꍇ��true��Ԃ�
        foreach (int wave in waveNumbers)
        {
            if (wave == currentWave && !hasSwarmSpawnedThisWave)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator SpawnFishSwarm()
    {
        // �����_���ȃ��C����I��
        Transform selectedLine = lines[Random.Range(0, lines.Count)];

        // ���Q�I�u�W�F�N�g�𐶐�
        GameObject fishSwarm = new GameObject("FishSwarm");
        fishSwarm.tag = "FishSwarm";
        fishSwarm.transform.position = selectedLine.position;

        // �v���n�u�����C���̐i�s�����������悤�ɉ�]
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
        hasSwarmSpawnedThisWave = false; // ���̃E�F�[�u�ōēx���Q�𔭐��\�ɂ���
    }

    private void ApplyDamage(Vector3 swarmPosition)
    {
        // ���Q�̎��͂ɂ���Ώۂ����o
        Collider[] colliders = Physics.OverlapSphere(swarmPosition, damageRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Ally") || collider.CompareTag("Enemy"))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // �t���[�����[�g�Ɉˑ����Ȃ��_���[�W��^����
                    damageable.TakeDamage(Mathf.CeilToInt(damagePerSecond * Time.deltaTime));
                }
            }
        }
    }
}
