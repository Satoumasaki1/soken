using UnityEngine;
using UnityEngine.AI;

public class OOKAMIUO : MonoBehaviour, IDamageable
{
    public string targetTag = "koukaku"; // �U���Ώۂ̃^�O��ݒ�
    public string fallbackTag = "Base"; // �^�[�Q�b�g��������Ȃ������ꍇ�̑�փ^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 25; // OOKAMIUO�̗̑�
    public int attackDamage = 8; // �U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 2f; // �U���N�[���_�E������

    private float lastAttackTime;
    private NavMeshAgent agent;

    // ��დŊ֘A�̐ݒ�
    public bool isPoisoned = false; // ��დŏ�Ԃ��ǂ���
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // ��დłɂ��X�s�[�h������
    private float originalAttackCooldown;
    private float originalSpeed;
    private bool poisonEffectApplied = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
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

        if (target == null || !target.CompareTag(targetTag))
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
        // �D��^�[�Q�b�g�ikoukaku�^�O�j��T��
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(targetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        // koukaku��������Ȃ��ꍇ�ABase�^�O�̃^�[�Q�b�g��T��
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
        Debug.Log($"{name} ���_���[�W���󂯂܂���: {damageAmount}, �c��̗�: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    public void ApplyPoison(float duration, float slowEffect)
    {
        isPoisoned = true;
        poisonEndTime = Time.time + duration;
        if (!poisonEffectApplied)
        {
            agent.speed = originalSpeed * slowEffect; // �ړ����x������������
            attackCooldown = originalAttackCooldown * 2; // �U���N�[���_�E���𒷂�����
            poisonEffectApplied = true;
            Debug.Log($"{name} ����დł̌��ʂ��󂯂܂����B��������: {duration}�b�A�X���[����: {slowEffect}");
        }
    }

    private void RemovePoisonEffect()
    {
        isPoisoned = false;
        agent.speed = originalSpeed; // �ړ����x�����ɖ߂�
        attackCooldown = originalAttackCooldown; // �U���N�[���_�E�������ɖ߂�
        poisonEffectApplied = false;
        Debug.Log($"{name} �̖�დł̌��ʂ���������܂����B");
    }

    private void Die()
    {
        // OOKAMIUO���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Debug.Log($"{name} ���|��܂����B");
        Destroy(gameObject);
    }
}
