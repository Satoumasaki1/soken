using UnityEngine;
using UnityEngine.AI;

public class Ikasan : MonoBehaviour, IDamageable
{
    public string targetTag = "Ally"; // �U���Ώۂ̃^�O��ݒ�
    public string fallbackTag = "Base"; // �^�[�Q�b�g��������Ȃ������ꍇ�̑�փ^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 10; // Ikasan�̗̑�
    public int attackDamage = 2; // �U����
    public float attackRange = 3f; // �U���͈�
    public float attackCooldown = 1f; // �U���N�[���_�E������

    private float lastAttackTime;
    private NavMeshAgent agent;

    // ��დŊ֘A�̐ݒ�
    public bool isPoisoned = false; // ��დŏ�Ԃ��ǂ���
    public float poisonDuration = 5f; // ��დł̎�������
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // ��დłɂ��X�s�[�h������
    private float originalAttackCooldown;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalAttackCooldown = attackCooldown;
        FindTarget();
    }

    void Update()
    {
        if (isPoisoned)
        {
            // ��დł̌��ʂ������ԁA�ړ����x�ƍU���N�[���_�E������������
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

        if (target == null)
        {
            FindTarget();
        }

        if (target != null)
        {
            // �^�[�Q�b�g�Ɍ������Ĉړ�����
            agent.SetDestination(target.position);

            // �U���͈͓��Ƀ^�[�Q�b�g������ꍇ�A�U������
            if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackTarget();
                lastAttackTime = Time.time;
            }
        }
    }

    void FindTarget()
    {
        // �D��^�[�Q�b�g�iAlly�^�O�j��T��
        GameObject allyTarget = GameObject.FindGameObjectWithTag(targetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // Ally��������Ȃ��ꍇ�ABase�^�O�̃^�[�Q�b�g��T��
        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void ApplyPoison(float duration, float slowEffect)
    {
        isPoisoned = true;
        poisonEndTime = Time.time + duration;
        poisonSlowEffect = slowEffect;
        agent.speed *= slowEffect; // �ړ����x������������
        attackCooldown *= 2; // �U���N�[���_�E���𒷂�����
        Debug.Log($"{name} ����დł̌��ʂ��󂯂܂����B��������: {duration}�b�A�X���[����: {slowEffect}");
    }

    private void RemovePoisonEffect()
    {
        isPoisoned = false;
        agent.speed /= poisonSlowEffect; // �ړ����x�����ɖ߂�
        attackCooldown = originalAttackCooldown; // �U���N�[���_�E�������ɖ߂�
        Debug.Log($"{name} �̖�დł̌��ʂ���������܂����B");
    }

    private void Die()
    {
        // Ikasan���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }
}
