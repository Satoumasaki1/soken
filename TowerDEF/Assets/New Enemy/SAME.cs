using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class SAME : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 150; // SAME�̗̑�
    public int maxHealth = 150; // �ő�̗�
    public int attackDamage = 80; // SAME�̍U����
    public float attackRange = 4f; // �U���͈�
    public float attackCooldown = 5f; // �U���N�[���_�E������
    public float moveSpeed = 5f; // �ʏ�̈ړ����x
    public float dashDistance = 6f; // �̓�����U���̋���
    public float dashDuration = 0.5f; // �̓�����U���̎���

    private float lastAttackTime;
    private NavMeshAgent agent;
    private bool isDashing = false;

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

    // **�w���X�o�[�ݒ�**
    [Header("�w���X�o�[�ݒ�")]
    public GameObject healthBarPrefab; // �w���X�o�[�̃v���n�u
    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Transform cameraTransform;

    // **�G�t�F�N�g���T�E���h�ݒ�**
    [Header("�U���G�t�F�N�g���T�E���h�ݒ�")]
    public GameObject attackEffectPrefab; // �U���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint; // �G�t�F�N�g�����ʒu
    public AudioClip dashSound; // �̓�����T�E���h
    private AudioSource audioSource;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed; // �ړ����x��ݒ�
        originalAttackCooldown = attackCooldown;
        originalSpeed = agent.speed;
        originalHealth = health;

        // �w���X�o�[�𐶐�
        cameraTransform = Camera.main.transform;
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2.5f, 0); // ����ɔz�u
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

        // AudioSource��ݒ�
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

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
            if (Time.time > poisonEndTime)
            {
                RemovePoisonEffect();
            }
        }

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
        {
            FindTarget();
        }

        if (target != null && !isDashing)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                agent.isStopped = true;

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    StartCoroutine(PerformDashAttack());
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

        healthSlider.value = health;
        healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - cameraTransform.position);
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
        GameObject allyTarget = GameObject.FindGameObjectWithTag(primaryTargetTag);
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

        // �̓�����U���̏���
        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        PlayAttackEffect(); // �G�t�F�N�g���T�E���h�Đ�

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �U������
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(primaryTargetTag) || collider.CompareTag(fallbackTag))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                    Debug.Log($"SAME�� {collider.name} �ɍU�����s���܂����B�_���[�W: {attackDamage}");
                }
            }
        }

        isDashing = false;
    }

    private void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
        }

        if (dashSound != null && audioSource != null)
        {
            audioSource.clip = dashSound;
            audioSource.Play();
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
        if (seasonEffectApplied && this.currentSeason == currentSeason) return;

        ResetSeasonEffect();
        this.currentSeason = currentSeason;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                moveSpeed = originalSpeed * 1.2f;
                agent.speed = moveSpeed;
                break;
            case GameManager.Season.Summer:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
                moveSpeed = originalSpeed * 1.4f;
                agent.speed = moveSpeed;
                break;
            case GameManager.Season.Autumn:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                moveSpeed = originalSpeed * 0.9f;
                agent.speed = moveSpeed;
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 0.7f);
                moveSpeed = originalSpeed * 0.6f;
                agent.speed = moveSpeed;
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