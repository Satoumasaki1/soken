using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Uni : MonoBehaviour, IDamageable
{
    public int hp = 15; // ����HP
    public int damage = 2; // �����_���[�W
    public int level = 1; // �������x��
    public int maxLevel = 3; // �ő僌�x��
    public float attackRange = 1.5f; // �U���͈�
    public float attackCooldown = 2f; // �U���N�[���_�E��
    private float attackTimer = 0f;

    private Transform target;

    private void Start()
    {
        UpdateStats(); // ���x���ɉ������X�e�[�^�X��ݒ�
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            FindNearestTarget();
            if (target != null)
            {
                AttackTargetIfInRange();
                attackTimer = attackCooldown;
            }
        }
    }

    // ���x���A�b�v����
    public void LevelUp()
    {
        if (level < maxLevel)
        {
            level++; // ���x�����グ��
            UpdateStats(); // ���x���ɉ�����HP�ƃ_���[�W���X�V
            Debug.Log("Uni leveled up to level " + level + "!");
        }
        else
        {
            Debug.Log("Uni has already reached the max level.");
        }
    }

    // ���x���ɉ����ăX�e�[�^�X���X�V
    private void UpdateStats()
    {
        switch (level)
        {
            case 2:
                hp = 20;
                damage = 5;
                break;
            case 3:
                hp = 30;
                damage = 7;
                break;
            default:
                hp = 15;
                damage = 2;
                break;
        }
    }

    // �^�[�Q�b�g��������֐�
    private void FindNearestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < nearestDistance && distanceToEnemy <= attackRange)
            {
                nearestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && nearestDistance <= attackRange)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    // �^�[�Q�b�g���U���͈͓��Ȃ�U������
    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= attackRange)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log(gameObject.name + " attacks " + target.name + " with " + damage + " damage.");
            }
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

    // �A�C�e�����g���ă��x���A�b�v����֐�
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LevelUpItem"))
        {
            LevelUp(); // ���x���A�b�v
            Destroy(other.gameObject); // �A�C�e��������
        }
    }
}
