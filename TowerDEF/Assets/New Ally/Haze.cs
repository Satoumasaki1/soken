using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Haze : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Haze�̗̑͂ƍő�̗�
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 5;
    public int attackDamage = 5;

    // �U���֘A�̐ݒ�
    public float detectionRadius = 10f;
    public float attackCooldown = 3.0f;

    private Transform target;
    private float lastAttackTime;

    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    private Animator animator; // �A�j���[�V�����𐧌䂷�邽�߂�Animator

    // �U���G�t�F�N�g�֘A
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject attackEffectPrefab; // �G�t�F�N�g�̃v���n�u (damage ef)
    public Transform effectSpawnPoint;   // �G�t�F�N�g�𐶐�����ʒu
    public AudioClip attackSound;           // ���ʉ���AudioClip
    private AudioSource audioSource;        // ���ʉ����Đ�����AudioSource

    // **�V�K�ǉ�: �̓�����̓����𐧌䂷�邽�߂̃t�B�[���h**
    [Header("�̓�����ݒ�")]
    public float dashDistance = 2.0f; // �O�i���鋗��
    public float dashDuration = 0.2f; // �O�i�ɂ����鎞��
    public float returnDuration = 0.2f; // ��ނɂ����鎞��

    private bool isDashing = false; // �̓����蒆���ǂ����𔻒肷��t���O

    // **�̗̓o�[�֘A�̐ݒ�**
    [Header("�̗̓o�[�֘A")]
    public GameObject healthBarPrefab;      // �̗̓o�[�̃v���n�u
    private GameObject healthBarInstance;   // ���ۂɐ������ꂽ�̗̓o�[
    private Slider healthSlider;            // �̗̓o�[�̃X���C�_�[�R���|�[�l���g

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

        // GameManager�Q�Ƃ̎擾
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }

        animator = GetComponent<Animator>(); // Animator�̎擾
        if (animator == null)
        {
            Debug.LogWarning("Animator���A�^�b�`����Ă��܂���I");
        }

        // **�̗̓o�[�̏�����**
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform); // �̗̓o�[�𐶐�
            healthBarInstance.transform.localPosition = new Vector3(0, 0.5f, 0); // �L�����N�^�[�̓���ɔz�u

            healthSlider = healthBarInstance.GetComponentInChildren<Slider>(); // Slider �R���|�[�l���g���擾

            if (healthSlider != null)
            {
                healthSlider.maxValue = 1; // �X���C�_�[�̍ő�l�� 1 �ɐݒ�
                healthSlider.value = (float)health / maxHealth; // ���݂̗̑͂ɉ����ăX���C�_�[�̒l��ݒ�
            }
        }
        else
        {
            Debug.LogError("HealthBarPrefab���ݒ肳��Ă��܂���I");
        }

        audioSource = gameObject.AddComponent<AudioSource>(); // AudioSource��ǉ�
        audioSource.playOnAwake = false; // �����Đ��𖳌���
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyBuffFromUdeppo();
        ApplyIrukaBuff();
        AttackOn();

        // **�̗̓o�[���X�V**
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || healthBarInstance == null) return;

        float healthPercentage = (float)health / maxHealth;
        healthSlider.value = healthPercentage;

        // �̗͂��ő�̏ꍇ�̗͑̓o�[���\��
        healthBarInstance.SetActive(health < maxHealth);

        // **�̗̓o�[�̉�]���J�����ɍ��킹��**
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

                // **�񕜎��ɑ̗̓o�[���X�V**
                UpdateHealthBar();

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

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
            PlayAttackEffect(); // �U���G�t�F�N�g�ƌ��ʉ��̍Đ�
            AttackTarget();
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

    void AttackTarget()
    {
        if (animator != null) animator.SetTrigger("Attack");

        if (!isDashing) StartCoroutine(PerformDash());

        PlayAttackEffect();

        if (ApplyBuffFromUdeppo())
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Debug.Log("�͈͍U�������s��...");
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    IDamageable damageable = collider.GetComponent<IDamageable>();
                    damageable?.TakeDamage(attackDamage);
                }
            }
        }
        else if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;

        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(dashPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
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
            Debug.Log("narase!!!!!!!!!");
        }
        else
        {
            Debug.LogWarning("�U�����ʉ����ݒ肳��Ă��܂���I");
        }

    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private bool ApplyBuffFromUdeppo()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool isUdeppoNearby = false;

        foreach (Collider collider in colliders)
        {
            Udeppo udeppo = collider.GetComponent<Udeppo>();
            if (udeppo != null && udeppo.gameObject != gameObject)
            {
                isUdeppoNearby = true;
                break;
            }
        }

        if (isUdeppoNearby && !maxHealthBuffApplied)
        {
            maxHealth += 20;
            health = Mathf.Min(health + 20, maxHealth);
            maxHealthBuffApplied = true;
        }
        else if (!isUdeppoNearby && maxHealthBuffApplied)
        {
            maxHealth -= 20;
            health = Mathf.Min(health, maxHealth);
            maxHealthBuffApplied = false;
        }

        return isUdeppoNearby;
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.1f);
                break;
            case GameManager.Season.Summer:
                maxHealth -= 5;
                health = Mathf.Min(health, maxHealth);
                break;
            case GameManager.Season.Autumn:
                maxHealth += 15;
                health = Mathf.Min(health + 15, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.2f);
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 0.9f);
                break;
        }

        seasonEffectApplied = true;
    }

    public void ResetSeasonEffect()
    {
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
