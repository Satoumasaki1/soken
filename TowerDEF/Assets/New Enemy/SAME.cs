using UnityEngine;
using UnityEngine.AI;

public class SAME : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string fallbackTag = "Base"; // �Ō�Ɏ낤�^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 150; // SAME�̗̑͂�����
    public int attackDamage = 80; // SAME�̍U���͂�����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 5f; // �U���N�[���_�E������
    public float moveSpeed = 5f; // �ʏ�̈ړ����x

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
    public bool isStunned = false; // �X�^����Ԃ��ǂ���
    private float stunEndTime;

    // �V�[�Y�����ʊ֘A�̐ݒ�
    private bool seasonEffectApplied = false;
    private GameManager.Season currentSeason;
    private int originalHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // �ړ����x��ݒ�
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        originalHealth = health;
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
                return; // �X�^�����͍s�����Ȃ�
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

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
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
                    PerformAreaAttack();
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
        // �D��^�[�Q�b�g�iAlly�^�O�j��T��
        GameObject allyTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
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

    void PerformAreaAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
        int targetCount = 0;
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(fallbackTag))
            {
                targetCount++;
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int totalDamage = attackDamage + (targetCount - 1) * 10; // �U���͈͂ɂ���^�[�Q�b�g���ɉ����ă_���[�W����
                    damageable.TakeDamage(totalDamage);
                    Debug.Log("SAME���͈͍U�����s���܂���: " + collider.name + " �_���[�W: " + totalDamage);
                }
            }
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
        agent.isStopped = true; // �ړ����~����
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
        // SAME���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
        Debug.Log($"{name} ���|��܂����B");
        Destroy(gameObject);
    }

    // �V�[�Y���̌��ʂ�K�p���郁�\�b�h
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied && this.currentSeason == currentSeason) return;

        ResetSeasonEffect();
        this.currentSeason = currentSeason;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                moveSpeed = originalSpeed * 1.2f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} �͏t�̃V�[�Y���ŋ�������܂����B�U����: {attackDamage}, �ړ����x: {moveSpeed}");
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
                moveSpeed = originalSpeed * 1.4f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} �͉ẴV�[�Y���ő啝�ɋ�������܂����B�U����: {attackDamage}, �ړ����x: {moveSpeed}");
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                moveSpeed = originalSpeed * 0.9f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} �͏H�̃V�[�Y���Ŏ኱��̉����܂����B�U����: {attackDamage}, �ړ����x: {moveSpeed}");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                moveSpeed = originalSpeed * 0.6f;
                agent.speed = moveSpeed;
                Debug.Log($"{name} �͓~�̃V�[�Y���ő啝�Ɏ�̉����܂����B�U����: {attackDamage}, �ړ����x: {moveSpeed}");
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 80;
        moveSpeed = originalSpeed;
        agent.speed = moveSpeed;
        seasonEffectApplied = false;
    }
}
