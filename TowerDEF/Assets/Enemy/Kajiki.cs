using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kajiki : MonoBehaviour, IDamageable
{
    public float speed = 6.0f; // �ړ����x�i�����j
    public int attackPower = 30; // �U���́i�����j
    public int health = 150; // �̗́i���j
    public float dashSpeedMultiplier = 10.0f; // �ːi���̑��x�{��
    public float dashInterval = 10.0f; // �ːi���s���Ԋu�i�b�j
    public float stunDuration = 2.0f; // �ːi��̒�~���ԁi�b�j
    public float dashDistance = 10.0f; // �ːi���鋗��

    private Transform targetAlly;
    private Transform targetBase;
    private Transform currentTarget;
    private float dashTimer;
    private bool isStunned = false;
    private bool isDashing = false;

    private void Start()
    {
        targetAlly = GameObject.FindGameObjectWithTag("Ally")?.transform;
        targetBase = GameObject.FindGameObjectWithTag("Base").transform;
        currentTarget = targetAlly != null ? targetAlly : targetBase;
        dashTimer = dashInterval;
    }

    private void Update()
    {
        if (isStunned || isDashing)
        {
            return; // �X�^�����܂��͓ːi���͓����Ȃ�
        }

        if (currentTarget == null || (!currentTarget.CompareTag("Ally") && !currentTarget.CompareTag("Base")))
        {
            // ���݂̃^�[�Q�b�g���Ȃ��Ȃ����ꍇ�A�ēx�^�[�Q�b�g��T��
            targetAlly = GameObject.FindGameObjectWithTag("Ally")?.transform;
            currentTarget = targetAlly != null ? targetAlly : targetBase;
        }

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            StartCoroutine(DashTowardsTarget());
            dashTimer = dashInterval;
        }
        else if (currentTarget != null)
        {
            MoveTowardsTarget(currentTarget);
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private IEnumerator DashTowardsTarget()
    {
        if (currentTarget != null)
        {
            isDashing = true;
            Vector3 startPosition = transform.position;
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            float distanceTraveled = 0f;

            while (distanceTraveled < dashDistance)
            {
                float step = speed * dashSpeedMultiplier * Time.deltaTime;
                transform.position += direction * step;
                distanceTraveled += step;
                yield return null;
            }

            health = Mathf.Max(0, health - (int)(150 * 0.1f)); // �ő�̗͂�10%������������
            Debug.Log(gameObject.name + " �������ːi���s���܂����B�c��̗�: " + health);

            isDashing = false;
            StartCoroutine(StunAfterDash());
        }
    }

    private IEnumerator StunAfterDash()
    {
        isStunned = true;
        Debug.Log(gameObject.name + " �͓ːi��ɒ�~���Ă��܂��B");
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        Debug.Log(gameObject.name + " ���Ăѓ����n�߂܂����B");
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " ���U�����󂯂܂����B�c��̗�: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " �͓|����܂����B");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Base") || other.CompareTag("Ally"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackPower);
                Debug.Log(gameObject.name + " �� " + other.name + " �ɍU�����s���܂����B�_���[�W: " + attackPower);
            }
        }
    }
}
