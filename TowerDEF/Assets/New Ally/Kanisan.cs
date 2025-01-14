using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Kanisan : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Kanisan�̊�{�v���p�e�B
    public int health = 10;
    public int maxHealth = 10; //�폜
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

    public void OnApplicationQuit()�@//�ǉ�
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage)//�ǉ�
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage);
    }

    public void SaveState()//�ǉ�
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState()//�ǉ�
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

        LoadState();//�ǉ�

        // GameManager�̎Q�Ƃ��擾
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }

        // ���݂̗̑͂�������
        health = maxHealth;

        // �̗̓o�[�𐶐����A�L�����N�^�[�̎q�I�u�W�F�N�g�Ƃ��Ĕz�u
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = new Vector3(0, 2, 0); // ����ɔz�u
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = 1;
                healthSlider.value = health;
            }
        }
        else
        {
            Debug.LogError("HealthBarPrefab���ݒ肳��Ă��܂���I");
        }
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

        float healthPercentage = (float)health / maxHealth;
        healthSlider.value = healthPercentage;

        // �̗͂��}�b�N�X�̏ꍇ�͔�\��
        healthBarInstance.SetActive(health < maxHealth);

        // �o�[�̐F���X�V
        Image fill = healthSlider.fillRect.GetComponent<Image>();
        if (fill != null)
        {
            fill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
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
                    if (health < maxHealth)
                    {
                        health += 2; // �񕜗�
                        health = Mathf.Min(health, maxHealth);
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

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackTarget();
                lastAttackTime = Time.time;
            }
            else if (distanceToTarget > detectionRadius)
            {
                target = null;
            }
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

    private void AttackTarget()
    {
        if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
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

    private void Die()
    {
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
        maxHealth += nearbyKanisanCount * 5;
        health = Mathf.Min(health + nearbyKanisanCount * 5, maxHealth);

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
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.1f);
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 1.2f);
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(attackDamage * 0.9f);
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        maxHealth = 10;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
    }
}
