using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Kanisan : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Kanisan�̊�{�v���p�e�B
    public int health = 10;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 3;
    public int attackDamage = 3;
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��

    // �U���֘A�̐ݒ�
    public float detectionRadius = 10f;     // �G�����m����͈�
    public float attackRange = 5f;          // �U���͈�
    public float attackCooldown = 1.5f;     // �U���̃N�[���_�E������
    private Transform target;               // �U���Ώۂ̓G
    private float lastAttackTime;           // �Ō�ɍU����������

    // GameManager�̎Q��
    [SerializeField]
    private GameManager gm;

    // �̗̓o�[�֘A
    public GameObject healthBarPrefab;      // �̗̓o�[�̃v���n�u
    private GameObject healthBarInstance;   // ���ۂɐ������ꂽ�̗̓o�[
    private Slider healthSlider;            // �̗̓o�[�̃X���C�_�[�R���|�[�l���g

    private bool seasonEffectApplied = false;

    // **�U���G�t�F�N�g�����ʉ��֘A**
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject attackEffectPrefab;   // �U���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint;      // �G�t�F�N�g�𐶐�����ʒu
    public AudioClip attackSound;           // ���ʉ���AudioClip
    private AudioSource audioSource;        // ���ʉ����Đ�����AudioSource

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

        // GameManager�̎Q�Ƃ��擾
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }

        // �̗̓o�[�𐶐����A�L�����N�^�[�̎q�I�u�W�F�N�g�Ƃ��Ĕz�u
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2, 0); // ����ɔz�u
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = 10;
                healthSlider.value = health;
            }
        }
        else
        {
            Debug.LogError("HealthBarPrefab���ݒ肳��Ă��܂���I");
        }

        // **AudioSource�̏�����**
        audioSource = gameObject.AddComponent<AudioSource>(); // AudioSource��ǉ�
        audioSource.playOnAwake = false; // �����Đ��𖳌���
    }

    void Update()
    {
        // �ꎞ��~���͏������X�L�b�v
        if (gm != null && gm.isPaused)
            return;

        // �U������
        AttackOn();

        // ���͂̃J�j����ɂ��o�t
        BuffKanisan();

        // �C���J����̃o�t�K�p
        ApplyIrukaBuff();

        // �̗̓o�[���X�V
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        float healthPercentage = (float)health / 10; // ���N��Ԃ� 0�`1 �ɐ��K��
        healthSlider.value = healthPercentage;

        // �̗͂��}�b�N�X�̏ꍇ�͔�\��
        healthBarInstance.SetActive(health < 10);

        // �̗̓o�[�̉�]���J�����ɍ��킹��
        if (Camera.main != null)
        {
            healthBarInstance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    private void OnDestroy()
    {
        // �L�����N�^�[���폜���ꂽ�ꍇ�A�̗̓o�[���폜
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    private void OnMouseDown()
    {
        // �a�ŉ񕜂��鏈��
        TryHeal();
    }

    public void TryHeal()
    {
        if (gm.SelectedFeedType.HasValue)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                if (gm.inventory[selectedFeed] > 0)
                {
                    if (health < 10)
                    {
                        health += 2; // �񕜗�
                        health = Mathf.Min(health, 10);
                        gm.inventory[selectedFeed]--;
                        gm.UpdateResourceUI();
                        // �̗̓o�[���X�V
                        UpdateHealthBar();
                        Debug.Log($"{selectedFeed} �ő̗͂��񕜂��܂����B�c��݌�: {gm.inventory[selectedFeed]}");
                    }
                    else
                    {
                        Debug.Log("�̗͂͊��ɍő�ł��B");
                    }
                }
                else
                {
                    Debug.Log($"{selectedFeed} �̍݌ɂ��s�����Ă��܂��B");
                }
            }
            else
            {
                Debug.Log("���̉a�ł͉񕜂ł��܂���B");
            }
        }
        else
        {
            Debug.Log("�a���I������Ă��܂���B");
        }
    }

    public void PlayAttackEffect()
    {
        // **�G�t�F�N�g�̐���**
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f); // �G�t�F�N�g����莞�Ԍ�ɍ폜

            Debug.Log("�U���G�t�F�N�g�𐶐����܂����I");
        }
        else
        {
            Debug.LogWarning("�U���G�t�F�N�g�̃v���n�u�܂��͐����ʒu���ݒ肳��Ă��܂���I");
        }

        // **���ʉ��̍Đ�**
        if (attackSound != null && audioSource != null)
        {
            audioSource.clip = attackSound; // ���ʉ���ݒ�
            audioSource.Play(); // ���ʉ����Đ�
        }
        else
        {
            Debug.LogWarning("�U�����ʉ����ݒ肳��Ă��܂���I");
        }
    }

    public void PerformSmashAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        StartCoroutine(SmashAttack());
    }

    private IEnumerator SmashAttack()
    {
        // �㏸���[�V����
        Vector3 originalPosition = transform.position;
        Vector3 raisedPosition = originalPosition + Vector3.up * 2f;
        float elapsedTime = 0f;

        while (elapsedTime < 0.2f)
        {
            transform.position = Vector3.Lerp(originalPosition, raisedPosition, elapsedTime / 0.2f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // **�U���G�t�F�N�g�ƌ��ʉ����Đ�**
        PlayAttackEffect();

        // �����������[�V����
        elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            transform.position = Vector3.Lerp(raisedPosition, originalPosition, elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �U����K�p
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
        }

        lastAttackTime = Time.time;
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            PerformSmashAttack(); // **���������U�������s**
        }
    }

    private void DetectEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                target = collider.transform;
                break;
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        UpdateHealthBar();

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ���|��܂����I");
        Destroy(gameObject);
    }

    private void BuffKanisan()
    {
        if (maxHealthBuffApplied) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        int nearbyKanisanCount = 0;
        foreach (Collider collider in colliders)
        {
            Kanisan kanisan = collider.GetComponent<Kanisan>();
            if (kanisan != null && kanisan != this)
            {
                nearbyKanisanCount++;
                if (nearbyKanisanCount >= 5) break;
            }
        }

        attackDamage += nearbyKanisanCount * 5;
        health = Mathf.Min(health + nearbyKanisanCount * 5, 10);

        maxHealthBuffApplied = true;
    }

    private void ApplyIrukaBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool irukaNearby = false;
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Iruka iruka))
            {
                irukaNearby = true;
                if (!isBuffActive)
                {
                    isBuffActive = true;
                    attackDamage = Mathf.RoundToInt(originalAttackDamage * buffMultiplier);
                }
                break;
            }
        }

        if (!irukaNearby && isBuffActive)
        {
            isBuffActive = false;
            attackDamage = originalAttackDamage;
        }
    }

    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                health = Mathf.Min(health + 10, 10);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                break;
            case GameManager.Season.Summer:
                health = Mathf.Min(health, 10);
                break;
            case GameManager.Season.Autumn:
                health = Mathf.Min(health + 15, 10);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                break;
            case GameManager.Season.Winter:
                health = Mathf.Min(health, 10);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        health = Mathf.Min(health, 10);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
    }
}
