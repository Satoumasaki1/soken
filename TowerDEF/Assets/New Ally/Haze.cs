using UnityEngine;
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

    // **�V�����ǉ�����G�t�F�N�g�֘A�̃t�B�[���h**
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject attackEffectPrefab; // �G�t�F�N�g�̃v���n�u (damage ef)
    public Transform effectSpawnPoint;   // �G�t�F�N�g�𐶐�����ʒu

    public void OnApplicationQuit() //�ǉ�
    {
        SaveState();
    }

    public void Upgrade(int additionalHp, int additionalDamage, int additionaRadius) //�ǉ�
    {
        health += additionalHp;
        attackDamage += additionalDamage;
        detectionRadius += additionaRadius;
        Debug.Log(gameObject.name + " upgraded! HP: " + health + ", Damage: " + attackDamage + ", Radius: " + detectionRadius);
    }

    public void SaveState() //�ǉ�
    {
        PlayerPrefs.SetInt($"{gameObject.name}_HP", health);
        PlayerPrefs.SetInt($"{gameObject.name}_Damage", attackDamage);
        Debug.Log($"{gameObject.name} state saved!");
    }

    public void LoadState() //�ǉ�
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
        LoadState(); //�ǉ�

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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyBuffFromUdeppo();
        ApplyIrukaBuff();
        AttackOn();
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

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
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
        // �U���A�j���[�V�������Đ�
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // �G�t�F�N�g�̍Đ��i�V�K�ǉ��j
        PlayAttackEffect();

        // �͈͍U���܂��͒P�̍U��
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
                    Debug.Log($"{collider.name} �� {attackDamage} �̃_���[�W��^���܂����B");
                }
            }
        }
        else if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
            Debug.Log($"{target.name} �� {attackDamage} �̃_���[�W��^���܂����B");
        }
    }

    // **�V�K�ǉ�: �U���G�t�F�N�g���Đ����郁�\�b�h**
    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            // �G�t�F�N�g�𐶐�
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);

            // ��莞�Ԍ�ɍ폜�i2�b��j
            Destroy(effect, 2.0f);

            Debug.Log("�U���G�t�F�N�g�𐶐����܂����I");
        }
        else
        {
            Debug.LogWarning("attackEffectPrefab �܂��� effectSpawnPoint ���ݒ肳��Ă��܂���I");
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
        Debug.Log("�߂��ɂ���e�b�|�E�G�r�����m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            Udeppo udeppo = collider.GetComponent<Udeppo>();
            if (udeppo != null && udeppo.gameObject != gameObject)
            {
                isUdeppoNearby = true;
                Debug.Log($"�e�b�|�E�G�r�����m���܂���: {udeppo.name}");
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

    // �C���J�̃o�t���L�����m�F���ēK�p���鏈��
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
                    Debug.Log($"{name} �̍U���͂���������܂���: {attackDamage}");
                }
                break;
            }
        }

        if (!irukaNearby && isBuffActive)
        {
            isBuffActive = false;
            attackDamage = originalAttackDamage;
            Debug.Log($"{name} �̍U���͋������I�����܂����B���̍U���͂ɖ߂�܂���: {attackDamage}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // �V�[�Y���̌��ʂ�K�p���郁�\�b�h
    public void ApplySeasonEffect(GameManager.Season currentSeason)
    {
        if (seasonEffectApplied) return;

        switch (currentSeason)
        {
            case GameManager.Season.Spring:
                maxHealth += 10;
                health = Mathf.Min(health + 10, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.1f);
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
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 1.2f);
                Debug.Log("�H�̃o�t���K�p����܂���: �̗͂ƍU���͂��㏸");
                break;
            case GameManager.Season.Winter:
                maxHealth -= 10;
                health = Mathf.Min(health, maxHealth);
                attackDamage = Mathf.RoundToInt(originalAttackDamage * 0.9f);
                Debug.Log("�~�̃f�o�t���K�p����܂���: �̗͂ƍU���͂�����");
                break;
        }

        seasonEffectApplied = true;
    }

    // �V�[�Y���̌��ʂ����Z�b�g���郁�\�b�h
    public void ResetSeasonEffect()
    {
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
