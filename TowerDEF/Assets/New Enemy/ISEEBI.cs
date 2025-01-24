using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class ISEEBI : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string targetTag = "Ally"; // �U���Ώۂ̃^�O��ݒ�
    public string fallbackTag = "Base"; // �^�[�Q�b�g��������Ȃ������ꍇ�̑�փ^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 15; // ISEEBI�̗̑�
    public int maxHealth = 15; // �ő�̗�
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

    // **�̓�����U���̐ݒ�**
    [Header("�̓�����U���ݒ�")]
    public float dashDistance = 3.0f;      // �O�i����
    public float dashDuration = 0.5f;      // �O�i����
    public float returnDuration = 0.3f;    // ��ގ���
    private bool isDashing = false;        // �̓����蒆���ǂ���

    // **�U���G�t�F�N�g���T�E���h**
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject attackEffectPrefab;  // �U���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint;     // �G�t�F�N�g�̐����ʒu
    public AudioClip attackSound;          // �U���T�E���h
    private AudioSource audioSource;       // �T�E���h�Đ��p

    // **�w���X�o�[**
    [Header("�w���X�o�[�ݒ�")]
    public GameObject healthBarPrefab;     // �w���X�o�[�̃v���n�u
    private GameObject healthBarInstance;  // ���ۂɐ������ꂽ�w���X�o�[
    private Slider healthSlider;           // �w���X�o�[�̃X���C�_�[
    private Transform cameraTransform;     // ���C���J������Transform

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;

        // AudioSource��������
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // �w���X�o�[��ݒ�
        cameraTransform = Camera.main.transform;
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2.0f, 0); // ����ɔz�u
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = health;
            }
        }
        else
        {
            Debug.LogError("�w���X�o�[�v���n�u���ݒ肳��Ă��܂���I");
        }

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
                agent.isStopped = true;

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    if (!isDashing) StartCoroutine(PerformDashAttack());
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        // �w���X�o�[�̒l���X�V
        healthSlider.value = health;

        // �w���X�o�[���J�����Ɍ�����
        healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - cameraTransform.position);

        // �w���X�o�[�̕\��/��\��
        healthBarInstance.SetActive(health < maxHealth);
    }

    private void OnDestroy()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    void FindTarget()
    {
        GameObject allyTarget = GameObject.FindGameObjectWithTag(targetTag);
        if (allyTarget != null)
        {
            target = allyTarget.transform;
            return;
        }

        GameObject baseTarget = GameObject.FindGameObjectWithTag(fallbackTag);
        if (baseTarget != null)
        {
            target = baseTarget.transform;
        }
    }

    private IEnumerator PerformDashAttack()
    {
        isDashing = true;

        // **�O�i�t�F�[�Y**
        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // **�U���t�F�[�Y**
        if (target != null)
        {
            AttackTarget();
        }

        PlayAttackEffect();

        // **��ރt�F�[�Y**
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(dashPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void PlayAttackEffect()
    {
        // �U���G�t�F�N�g
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
        }

        // �U���T�E���h
        if (attackSound != null && audioSource != null)
        {
            audioSource.clip = attackSound;
            audioSource.Play();
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
            agent.speed = originalSpeed * slowEffect;
            attackCooldown = originalAttackCooldown * 2;
            poisonEffectApplied = true;
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.isStopped = true;
    }

    private void RemoveStunEffect()
    {
        isStunned = false;
        agent.isStopped = false;
    }

    private void RemovePoisonEffect()
    {
        isPoisoned = false;
        agent.speed = originalSpeed;
        attackCooldown = originalAttackCooldown;
        poisonEffectApplied = false;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.85f);
                health = Mathf.Max(health - 2, 1);
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                health += 5;
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.75f);
                health = Mathf.Max(health - 4, 1);
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health += 8;
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        attackDamage = 5;
        health = maxHealth;
        seasonEffectApplied = false;
    }
}