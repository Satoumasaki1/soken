using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaniheisya : MonoBehaviour
{
    public GameObject soldierPrefab;  // ���m�̃v���n�u
    public float spawnRange = 10f;    // ���ɂ̏o���͈�
    public float spawnCooldown = 3f;  // ���m�̏o���N�[���_�E��
    public int hp = 20;               // ���ɂ�HP
    private float spawnTimer = 0f;

    private Transform targetEnemy;

    void Update()
    {
        // ���ɂ̏o���^�C�~���O���v��
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            FindTargetEnemy();

            if (targetEnemy != null)
            {
                SpawnSoldier();
                spawnTimer = spawnCooldown;  // ���m�o����Ƀ^�C�}�[�����Z�b�g
            }
        }

        // HP��0�ȉ��Ȃ畺�ɂ�j��
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    // �G��T������
    void FindTargetEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= spawnRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= spawnRange)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // ���m���o��������
    void SpawnSoldier()
    {
        Instantiate(soldierPrefab, transform.position, Quaternion.identity);
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
    }
}
