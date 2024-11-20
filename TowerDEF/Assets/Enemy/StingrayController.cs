using System.Collections;
using UnityEngine;

public class StingrayController : MonoBehaviour
{
    public int health = 20;
    public float speed = 1.5f; // ���x�͒x��
    public float attackRange = 1.0f;
    public int attackDamage = 15;
    public float poisonDuration = 5.0f; // �ł̌p������
    public int poisonDamagePerSecond = 2;
    public float attackCooldown = 2.0f;

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
        // ���������^�O����������D�悵�ă^�[�Q�b�g�ɂ���
        GameObject[] koukakuTargets = GameObject.FindGameObjectsWithTag("koukaku");
        float nearestDistance = Mathf.Infinity;
        GameObject nearestKoukaku = null;

        foreach (GameObject koukaku in koukakuTargets)
        {
            float distance = Vector3.Distance(transform.position, koukaku.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestKoukaku = koukaku;
            }
        }

        if (nearestKoukaku != null)
        {
            target = nearestKoukaku;
        }
        else
        {
            // ���������^�O�̐��������Ȃ��ꍇ�̓v���C���[�̋��_���^�[�Q�b�g�ɂ���
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
                ApplyPoison(target);
            }
            else if (target.CompareTag("koukaku"))
            {
                // ���������^�O�̐����ɍU������
                target.GetComponent<Health>().TakeDamage(attackDamage);
                ApplyPoison(target);
            }
            attackCooldownTimer = attackCooldown;
        }

        // �N�[���_�E�������炷
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void ApplyPoison(GameObject target)
    {
        PoisonEffect poison = target.GetComponent<PoisonEffect>();
        if (poison == null)
        {
            poison = target.AddComponent<PoisonEffect>();
        }
        poison.ApplyPoison(poisonDuration, poisonDamagePerSecond);
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

public class PoisonEffect : MonoBehaviour
{
    private float poisonTimer;
    private int poisonDamage;
    private bool isPoisoned;

    public void ApplyPoison(float duration, int damagePerSecond)
    {
        poisonTimer = duration;
        poisonDamage = damagePerSecond;
        if (!isPoisoned)
        {
            isPoisoned = true;
            StartCoroutine(ApplyPoisonDamage());
        }
    }

    private IEnumerator ApplyPoisonDamage()
    {
        while (poisonTimer > 0)
        {
            GetComponent<Health>().TakeDamage(poisonDamage);
            poisonTimer -= 1.0f;
            yield return new WaitForSeconds(1.0f);
        }
        isPoisoned = false;
    }
}
