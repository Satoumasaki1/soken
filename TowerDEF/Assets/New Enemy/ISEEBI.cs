using UnityEngine;
using UnityEngine.AI;

public class ISEEBI : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string targetTag = "Ally"; // �U���Ώۂ̃^�O��ݒ�
    public string fallbackTag = "Base"; // �^�[�Q�b�g��������Ȃ������ꍇ�̑�փ^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 15; // ISEEBI�̗̑�
    public int attackDamage = 5; // �U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 2f; // �U���N�[���_�E������

    private float lastAttackTime;
    private NavMeshAgent agent;

    // ��დŊ֘A�̐ݒ�
    public bool isPoisoned = false; // ��დŏ�Ԃ��ǂ���
    public float poisonDuration = 5f; // ��დł̎�������
    private float poisonEndTime;
    public float poisonSlowEffect = 0.5f; // ��დłɂ��X�s�[�h������
    private float originalAttackCooldown;
    private float originalSpeed;
    private bool poisonEffectApplied = false;

    // �X�^���֘A�̐ݒ�
    private bool isStunned = false;
    private float stunEndTime;

    // �V�[�Y�����ʓK�p�t���O
    private bool seasonEffectApplied = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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

        if (target == null || !target.CompareTag(targetTag))
        {
            FindTarget();
        }

        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                // �U���͈͓��Ƀ^�[�Q�b�g������ꍇ�A�ړ����~���čU������
                agent.isStopped = true;

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    AttackTarget();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // �U���͈͊O�̏ꍇ�̓^�[�Q�b�g�Ɍ������Ĉړ�����
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }
    }

    void FindTarget()
    {
        // �D��^�[�Q�b�g�ikoukaku�^�O�j��T��
        GameObject allyTarget = GameObject.FindGameObjectWithTag(targetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
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
        // ISEEBI���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Destroy(gameObject);
    }

    // �V�[�Y���̌��ʂ�K�p���郁�\�b�h
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.85f);
                health = Mathf.Max(health - 2, 1);
                Debug.Log("�t�̃f�o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                health += 5;
                Debug.Log("�Ẵo�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.75f);
                health = Mathf.Max(health - 4, 1);
                Debug.Log("�H�̃f�o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health += 8;
                Debug.Log("�~�̃o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
        }

        seasonEffectApplied = true;
    }

    // �V�[�Y���̌��ʂ����Z�b�g���郁�\�b�h
    public void ResetSeasonEffect()
    {
        attackDamage = 5;
        health = 15;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
