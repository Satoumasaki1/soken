using UnityEngine;
using UnityEngine.UI;

public class Onagazame : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Onagazame�̗̑�
    public int health = 25;
    public int maxHealth = 25;

    // Onagazame�̍U���͂ƍU���֘A�̐ݒ�
    public int attackDamage = 10;
    public float detectionRadius = 15f;     // �G�����m����͈�
    public float attackCooldown = 1.5f;    // �U���̃N�[���_�E������
    public float stunDuration = 12.0f;     // �X�^�����ʂ̎�������

    // �����ۍU���̓����֘A
    public float tailSwingDistance = 2.0f; // �����ۍU���őO�i���鋗��
    public float tailSwingDuration = 0.3f; // �O�i�ɂ����鎞��
    public float tailReturnDuration = 0.3f; // ��ނɂ����鎞��
    private bool isAttacking = false;      // ���ݍU�������ǂ����𔻒肷��t���O

    private Transform target;               // �U���Ώۂ̓G
    private float lastAttackTime;           // �Ō�ɍU����������

    // **�U���G�t�F�N�g���T�E���h�ݒ�**
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject tailAttackEffectPrefab; // �����ۍU���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint;        // �G�t�F�N�g�𐶐�����ʒu
    public AudioClip tailAttackSound;         // �����ۍU�����̌��ʉ�
    private AudioSource audioSource;          // ���ʉ����Đ����邽�߂�AudioSource

    // **�w���X�o�[�֘A**
    [Header("�w���X�o�[�ݒ�")]
    public GameObject healthBarPrefab;        // �w���X�o�[�̃v���n�u
    private GameObject healthBarInstance;     // ���ۂɐ������ꂽ�w���X�o�[
    private Slider healthSlider;              // �w���X�o�[�̃X���C�_�[�R���|�[�l���g
    private Transform cameraTransform;        // ���C���J������Transform

    // GameManager�̎Q�Ƃ��C���X�y�N�^�[����ݒ�ł���悤�ɂ���
    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    public void OnApplicationQuit()
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius)
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        detectionRadius += additionaRadius;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage + ", Radius: " + detectionRadius);
    }

    public void SaveState()
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()
    {
        if (PlayerPrefs.HasKey($"{gameObject.name}_HP"))
        {
            health = PlayerPrefs.GetInt($"{gameObject.name}_HP");
        }

        if (PlayerPrefs.HasKey($"{gameObject.name}_Damage"))
        {
            attackDamage = PlayerPrefs.GetInt($"{gameObject.name}_Damage");
        }

        Debug.Log($"{gameObject.name} state loaded! HP: {health}, Damage: {attackDamage}");
    }

    void Start()
    {
        LoadState();

        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }

        // **AudioSource�̏�����**
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // **�J�����̎Q�Ƃ��擾**
        cameraTransform = Camera.main.transform;

        // **�w���X�o�[�̏�����**
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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;

        AttackOn();        // �U������
        UpdateHealthBar(); // �w���X�o�[�̍X�V
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        // �w���X�o�[�̃X���C�_�[�l���X�V
        healthSlider.value = health;

        // �w���X�o�[���J�����Ɍ����ĉ�]
        healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - cameraTransform.position);

        // �w���X�o�[�̕\��/��\��
        healthBarInstance.SetActive(health < maxHealth);
    }

    private void OnDestroy()
    {
        // �w���X�o�[�̃C���X�^���X���폜
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    private void OnMouseDown()
    {
        TryHeal();
    }

    public void TryHeal()
    {
        if (!gm.SelectedFeedType.HasValue) { Debug.Log("�a���I������Ă��܂���B"); return; }

        var selectedFeed = gm.SelectedFeedType.Value;
        if (selectedFeed == GameManager.ResourceType.OkiaMi ||
            selectedFeed == GameManager.ResourceType.Benthos ||
            selectedFeed == GameManager.ResourceType.Plankton)
        {
            if (gm.inventory[selectedFeed] > 0 && health < maxHealth)
            {
                health = Mathf.Min(health + 2, maxHealth);
                gm.inventory[selectedFeed]--;
                gm.UpdateResourceUI();
                Debug.Log($"{selectedFeed} �ő̗͂��񕜂��܂����B�c��݌�: {gm.inventory[selectedFeed]}");
            }
            else
            {
                Debug.Log(health >= maxHealth ? "�̗͂͊��ɍő�ł��B" : $"{selectedFeed} �̍݌ɂ��s�����Ă��܂��B");
            }
        }
        else
        {
            Debug.Log("���̉a�ł͉񕜂ł��܂���B");
        }
    }

    public void PlayTailAttackEffect()
    {
        // **�G�t�F�N�g�̐���**
        if (tailAttackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(tailAttackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
            Debug.Log("�����ۍU���G�t�F�N�g�𐶐����܂����I");
        }

        // **���ʉ��̍Đ�**
        if (tailAttackSound != null && audioSource != null)
        {
            audioSource.clip = tailAttackSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("�U���G�t�F�N�g�܂��̓T�E���h���ݒ肳��Ă��܂���I");
        }
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
            if (!isAttacking) StartCoroutine(PerformTailAttack());
            lastAttackTime = Time.time;
        }
    }

    void DetectEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Debug.Log("�G�����m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                target = collider.transform;
                Debug.Log($"�G�����m���܂���: {target.name}");
                break;
            }
        }
    }

    private System.Collections.IEnumerator PerformTailAttack()
    {
        isAttacking = true;

        // **�O�i�t�F�[�Y**
        Vector3 startPosition = transform.position;
        Vector3 attackPosition = transform.position + transform.forward * tailSwingDistance;
        float elapsedTime = 0f;

        while (elapsedTime < tailSwingDuration)
        {
            transform.position = Vector3.Lerp(startPosition, attackPosition, elapsedTime / tailSwingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // **�U������**
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                damageable?.TakeDamage(attackDamage);

                IStunnable stunnable = collider.GetComponent<IStunnable>();
                stunnable?.Stun(stunDuration);
            }
        }

        // **�G�t�F�N�g���T�E���h�̍Đ�**
        PlayTailAttackEffect();

        // **��ރt�F�[�Y**
        elapsedTime = 0f;
        while (elapsedTime < tailReturnDuration)
        {
            transform.position = Vector3.Lerp(attackPosition, startPosition, elapsedTime / tailReturnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ���|��܂����I");
        Destroy(gameObject);
    }

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                Debug.Log("�t�̃o�t���K�p����܂���: �̗͂ƍU���͂������㏸");
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                Debug.Log("�Ẵf�o�t���K�p����܂���: �̗͂�����");
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                Debug.Log("�H�̃o�t���K�p����܂���: �̗͂ƍU���͂��㏸");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                Debug.Log("�~�̃f�o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        maxHealth = 25;
        health = Mathf.Min(health, maxHealth);
        attackDamage = 10;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}