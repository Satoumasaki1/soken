using UnityEngine;
using UnityEngine.AI;

public class ONIKAMASU : MonoBehaviour, IDamageable, IStunnable
{
    public string primaryTargetTag = "koukaku"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string secondaryTargetTag = "Ally"; // ���ɗD�悷��^�[�Q�b�g�̃^�O
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 45; // ONIKAMASU�̗̑�
    public int attackDamage = 10; // �U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 1.5f; // �U���N�[���_�E������
    public float moveSpeed = 6f; // �ړ����x������

    private float lastAttackTime;
    private NavMeshAgent agent;

    // ��დŊ֘A�̐ݒ�
    public bool isPoisoned = false; // ��დŏ�Ԃ��ǂ���
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // ��დłɂ��X�s�[�h������
    private float originalAttackCooldown;
    private float originalSpeed;
    private bool poisonEffectApplied = false;

    // �X�^���֘A�̐ݒ�
    private bool isStunned = false;
    private float stunEndTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // �ړ����x��ݒ�
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        FindTarget();
    }

    void Update()
    {
        if (isStunned)
        {
            if (Time.time > stunEndTime)
            {
                RemoveStunEffect();
            }
            else
            {
                return; // �X�^�����͉������Ȃ�
            }
        }

        if (isPoisoned)
        {
            // ��დł̌��ʂ������ԁA�ړ����x�ƍU���N�[���_�E������������
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(secondaryTargetTag) && !target.CompareTag(fallbackTag)))
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
        GameObject koukakuTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
        if (koukakuTarget != null)
        {
            target = koukakuTarget.transform;
            return;
        }

        // ���ɗD�悷��^�[�Q�b�g�iAlly�^�O�j��T��
        GameObject allyTarget = GameObject.FindGameObjectWithTag(secondaryTargetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        // ����ł�������Ȃ��ꍇ�ABase�^�O�̃^�[�Q�b�g��T��
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

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.isStopped = true; // �X�^�����͈ړ����~�߂�
        Debug.Log($"{name} ���X�^����ԂɂȂ�܂����B��������: {duration}�b");
    }

    private void RemoveStunEffect()
    {
        isStunned = false;
        agent.isStopped = false; // �ړ����ĊJ����
        Debug.Log($"{name} �̃X�^�����ʂ���������܂����B");
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
        // ONIKAMASU���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Debug.Log($"{name} ���|��܂����B");
        Destroy(gameObject);
    }
}
