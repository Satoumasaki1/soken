using UnityEngine;
using UnityEngine.AI;

public class KAZIKI : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 60; // KAZIKI�̗̑�
    public int attackDamage = 40; // �ːi�U���̈З͂��グ��
    public float attackRange = 6f; // �U���͈�
    public float attackCooldown = 3f; // �U���N�[���_�E������
    public float moveSpeed = 8f; // �ʏ�̈ړ����x������

    private float lastAttackTime;
    private NavMeshAgent agent;
    public float dashChargeTime = 2.0f; // �ːi�U���̃`���[�W����
    public float dashSpeed = 20f; // �ːi�U���̑��x
    public float dashDistance = 15f; // �ːi�U���̋���

    private bool isCharging = false;
    private bool isDashing = false;

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

        if (isCharging || isDashing)
        {
            return; // �`���[�W���܂��͓ːi���͓���𐧌䂷��
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
                StartCoroutine(ChargeAndDashAttack());
                lastAttackTime = Time.time;
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

    System.Collections.IEnumerator ChargeAndDashAttack()
    {
        isCharging = true;
        agent.isStopped = true; // �`���[�W���͈ړ����~����
        yield return new WaitForSeconds(dashChargeTime);

        isCharging = false;
        isDashing = true;
        agent.isStopped = false;

        Vector3 dashDirection = transform.forward; // �ːi�����͌��݂̑O��
        float dashStartTime = Time.time;

        while (Time.time < dashStartTime + (dashDistance / dashSpeed))
        {
            agent.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // �U�������i�ђʍU���j
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1.0f, dashDirection, dashDistance);
        foreach (RaycastHit hit in hits)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null && (hit.collider.CompareTag(primaryTargetTag) || hit.collider.CompareTag(fallbackTag)))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log("KAZIKI���ːi�U�����s���܂���: " + hit.collider.name);
            }
        }

        // �ːi��̔�����HP��10%����
        TakeDamage((int)(health * 0.1f));
        if (health <= 0)
        {
            Die();
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
        // KAZIKI���|�ꂽ�ۂ̏����i�Ⴆ�Δj��Ȃǁj
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
                moveSpeed = originalSpeed * 0.9f;
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                Debug.Log("�t�̃f�o�t���K�p����܂���: �ړ����x�ƍU���͂�����");
                break;
            case GameManager.Season.Summer:
                health += 10;
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                Debug.Log("�Ẵo�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
            case GameManager.Season.Autumn:
                moveSpeed = originalSpeed * 0.8f;
                attackDamage = Mathf.RoundToInt(attackDamage * 0.8f);
                health -= 10;
                Debug.Log("�H�̃f�o�t���K�p����܂���: �ړ����x�ƍU���͂�����");
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health += 15;
                Debug.Log("�~�̃o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
        }

        seasonEffectApplied = true;
    }

    // �V�[�Y�����ʂ̃��Z�b�g
    public void ResetSeasonEffect()
    {
        moveSpeed = originalSpeed;
        attackDamage = 40; // ���̍U���͂ɖ߂�
        health = originalHealth; // ���̗̑͂ɖ߂�
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
