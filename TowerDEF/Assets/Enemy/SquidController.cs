using System.Collections;
using UnityEngine;

public class SquidController : MonoBehaviour
{
    public int health = 10;
    public float speed = 2.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 3;
    public float attackCooldown = 1.2f;

    public GameObject target;
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

    public GameObject GetTarget()
    {
        return target;
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
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void AttackTargetIfInRange()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget <= attackRange && attackCooldownTimer <= 0f)
        {
            if (target.CompareTag("Base"))
            {
                // �v���C���[�̋��_�ɍU������
                PlayerBaseController baseController = target.GetComponent<PlayerBaseController>();
                if (baseController != null)
                {
                    baseController.TakeDamage(attackDamage);
                }
            }
            else if (target.CompareTag("Ally"))
            {
                // �����̃^���[�ɍU������
                Health healthComponent = target.GetComponent<Health>();
                if (healthComponent != null)
                {
                    healthComponent.TakeDamage(attackDamage);
                }
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
