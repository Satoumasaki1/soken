using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HanaminoKasago : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // HanaminoKasago�̗̑͂ƍő�̗�
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��
    public int health = 20;
    public int maxHealth = 20;

    // ��დŊ֘A�̐ݒ�
    public float detectionRadius = 15f; // ��დł͈̔�
    public float poisonDamage = 1.0f;   // �p���_���[�W
    public float effectInterval = 1.0f; // �p���_���[�W�̊Ԋu
    private float lastEffectTime;

    // �U���֘A
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 10;
    public int attackDamage = 10; // �U����

    // �̓�����֘A�̐ݒ�
    [Header("�̓�����ݒ�")]
    public float dashDistance = 2.0f; // �O�i���鋗��
    public float dashDuration = 0.2f; // �O�i�ɂ����鎞��
    public float returnDuration = 0.2f; // ��ނɂ����鎞��
    private bool isDashing = false; // �̓����蒆���ǂ����𔻒肷��t���O

    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    // **�̗̓o�[�֘A**
    [Header("�̗̓o�[�֘A")]
    public GameObject healthBarPrefab;      // �̗̓o�[�̃v���n�u
    private GameObject healthBarInstance;   // ���ۂɐ������ꂽ�̗̓o�[
    private Slider healthSlider;            // �̗̓o�[�̃X���C�_�[�R���|�[�l���g

    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject attackEffectPrefab;   // �U���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint;      // �G�t�F�N�g�𐶐�����ʒu

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

        // **�̗̓o�[�̏�����**
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform); // �̗̓o�[�𐶐�
            healthBarInstance.transform.localPosition = new Vector3(0, 0.5f, 0); // �L�����N�^�[�̏�i���� 0.5f�j�ɔz�u

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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;

        ApplyParalyticPoisonEffect();
        ApplyIrukaBuff();

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

    public void ApplyParalyticPoisonEffect()
    {
        if (Time.time > lastEffectTime + effectInterval)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    IDamageable enemy = collider.GetComponent<IDamageable>();
                    enemy?.TakeDamage((int)poisonDamage);
                }
            }
            lastEffectTime = Time.time;
        }
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

    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            // **�G�t�F�N�g�𐶐�**
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);

            // **��莞�Ԍ�ɃG�t�F�N�g���폜**
            Destroy(effect, 2.0f);

            Debug.Log("�U���G�t�F�N�g�𐶐����܂����I");
        }
        else
        {
            Debug.LogWarning("�U���G�t�F�N�g�̃v���n�u�܂��͐����ʒu���ݒ肳��Ă��܂���I");
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

    private IEnumerator PerformDash()
    {
        if (isDashing) yield break;

        isDashing = true;

        // �O�i
        Vector3 startPosition = transform.position;
        Vector3 dashPosition = transform.position + transform.forward * dashDistance;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, dashPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        // ���
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(dashPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
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
}
