using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class KAZIKI : MonoBehaviour, IDamageable, IStunnable, ISeasonEffect
{
    public string primaryTargetTag = "Ally"; // �D��^�[�Q�b�g�̃^�O��ݒ�
    public string fallbackTag = "Base"; // �Ō�ɑ_���^�[�Q�b�g�̃^�O

    private Transform target; // �^�[�Q�b�g��Transform
    public int health = 60; // KAZIKI�̗̑�
    public int maxHealth = 60; // �ő�̗�
    public int attackDamage = 40; // �ːi�U���̈З�
    public float attackRange = 6f; // �U���͈�
    public float attackCooldown = 3f; // �U���N�[���_�E������
    public float moveSpeed = 8f; // �ʏ�̈ړ����x

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

    // **�w���X�o�[�ݒ�**
    [Header("�w���X�o�[�ݒ�")]
    public GameObject healthBarPrefab; // �w���X�o�[�̃v���n�u
    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Transform cameraTransform;

    // **�G�t�F�N�g���T�E���h�ݒ�**
    [Header("�U���G�t�F�N�g���T�E���h�ݒ�")]
    public GameObject attackEffectPrefab; // �ːi�U���̃G�t�F�N�g
    public Transform effectSpawnPoint; // �G�t�F�N�g�����ʒu
    public AudioClip dashSound; // �ːi�U�����̃T�E���h
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

        if (isCharging || isDashing) return;

        if (target == null || (!target.CompareTag(primaryTargetTag) && !target.CompareTag(fallbackTag)))
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
                    StartCoroutine(ChargeAndDashAttack());
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

    private IEnumerator ChargeAndDashAttack()
    {
        isCharging = true;
        agent.isStopped = true; // �`���[�W���͒�~
        yield return new WaitForSeconds(dashChargeTime);

        isCharging = false;
        isDashing = true;
        agent.isStopped = false;

        Vector3 dashDirection = transform.forward; // �ːi����
        float dashStartTime = Time.time;

        PlayAttackEffect(); // �G�t�F�N�g���T�E���h�Đ�

        while (Time.time < dashStartTime + (dashDistance / dashSpeed))
        {
            agent.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // �ːi�U������
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

        TakeDamage((int)(health * 0.1f)); // �ːi��̔�����HP��10%����
        if (health <= 0)
        {
            Die();
        }
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
                moveSpeed = originalSpeed * 0.9f;
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                break;
            case GameManager.Season.Summer:
                health += 10;
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                break;
            case GameManager.Season.Autumn:
                moveSpeed = originalSpeed * 0.8f;
                attackDamage = Mathf.RoundToInt(attackDamage * 0.8f);
                health -= 10;
                break;
            case GameManager.Season.Winter:
                attackDamage = Mathf.RoundToInt(attackDamage * 1.3f);
                health += 15;
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        moveSpeed = originalSpeed;
        attackDamage = 40;
        health = originalHealth;
        seasonEffectApplied = false;
    }
}