using System.Collections;
using UnityEngine;

public class BarracudaController : MonoBehaviour
{
    public int health = 20;
    public float speed = 2.0f;
    public float attackRange = 1.0f;
    public int attackDamage = 5;
    public float attackCooldown = 1.0f;

    private GameObject target;
    private float attackCooldownTimer;

    private void Start()
    {
        // �^�[�Q�b�g���ŏ��ɐݒ�
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
        // Ally�^�O�����I�u�W�F�N�g��D�悵�ă^�[�Q�b�g�ɂ���
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
            // Ally�^�O�̃I�u�W�F�N�g�����Ȃ��ꍇ�̓v���C���[�̋��_���^�[�Q�b�g�ɂ���
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
                // Ally�^�O�̃I�u�W�F�N�g�ɍU������
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
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

    // �_���[�W���󂯂�֐�
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

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
