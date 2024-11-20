using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int health = 10;
    public float speed = 3.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 5;
    public float attackCooldown = 1.5f;

    private GameObject target;
    private float attackCooldownTimer;

    private void Start()
    {
        // �^�[�Q�b�g���ŏ��̓v���C���[�̋��_�ɐݒ�
        FindNearestTarget();
    }

    private void Update()
    {
        if (target != null)
        {
            MoveTowardsTarget();
            AttackTargetIfInRange();
        }
        else
        {
            // �^�[�Q�b�g���Ȃ��Ȃ�����V�����^�[�Q�b�g��T��
            FindNearestTarget();
        }
    }

    private void FindNearestTarget()
    {
        // �����̃^���[��D�悵�ă^�[�Q�b�g�ɂ���
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestAlly = null;

        foreach (GameObject ally in allies)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestAlly = ally;
            }
        }

        if (nearestAlly != null)
        {
            target = nearestAlly;
        }
        else
        {
            // ���������Ȃ��ꍇ�̓v���C���[�̋��_���^�[�Q�b�g�ɂ���
            target = GameObject.FindGameObjectWithTag("Base");
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            if (target.CompareTag("Base"))
            {
                // �v���C���[�̋��_�ɍU������
                target.GetComponent<PlayerBaseController>().TakeDamage(attackDamage);
            }
            else if (target.CompareTag("Ally"))
            {
                // �����̃^���[�ɍU������
                target.GetComponent<Healt>().TakeDamage(attackDamage);
            }
            attackCooldownTimer = attackCooldown;
        }

        // �N�[���_�E�������炷
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    // �_���[�W���󂯂��ۂ̏���
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͓|����܂���");
        Destroy(gameObject);
    }
}

public class Healt : MonoBehaviour
{
    public int currentHealth = 50;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
