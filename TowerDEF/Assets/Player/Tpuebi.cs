using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tpuebi : MonoBehaviour, IDamageable
{
    public float attackRange = 10f;  // �^���b�g�̍U���͈�
    public float attackCooldown = 1f; // �U���N�[���_�E��
    public int damage = 1; // �_���[�W��
    private float attackTimer = 0f;

    public int hp = 15; // ����HP
    private Transform targetEnemy;

    void Update()
    {
        // �^���b�g�̍U���^�C�~���O���v��
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            FindTargetEnemy();

            if (targetEnemy != null)
            {
                Attack(targetEnemy);
                attackTimer = attackCooldown;  // �U����Ƀ^�C�}�[�����Z�b�g
            }
        }

        // HP��0�ȉ��Ȃ�^���b�g��j��
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
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= attackRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= attackRange)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // �G���U������
    void Attack(Transform enemy)
    {
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log(gameObject.name + " attacks " + enemy.name + " with " + damage + " damage.");
        }
    }

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Remaining HP: " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject); // HP��0�ɂȂ�����j��
        }
    }
}

