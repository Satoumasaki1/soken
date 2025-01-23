using UnityEngine;
using System.Collections;

public class Kobanuzame : MonoBehaviour, IDamageable, ISeasonEffect, IUpgradable
{
    // Kobanuzame�̗̑�
    public int health = 20;
    public int maxHealth = 20;
    private bool isBuffApplied = false;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 5;
    public int attackDamage = 5;
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��

    // Kobanuzame�̍U���͂ƍU���֘A�̐ݒ�
    public float detectionRadius = 10f;     // �G�����m����͈�
    public float attackCooldown = 1.0f;     // �U���̃N�[���_�E������

    private Transform target;               // �U���Ώۂ̓G
    private float lastAttackTime;           // �Ō�ɍU����������

    // **�̓����菈���֘A**
    [Header("�̓�����ݒ�")]
    public float dashDistance = 3.0f;       // �̓�����̑O�i����
    public float dashDuration = 0.2f;       // �O�i�ɂ����鎞��
    public float returnDuration = 0.2f;     // ��ނɂ����鎞��
    private bool isDashing = false;         // ���ݑ̓����蒆���ǂ���

    // **�U���G�t�F�N�g���T�E���h�ݒ�**
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject attackEffectPrefab;   // �U���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint;      // �G�t�F�N�g�𐶐�����ʒu
    public AudioClip attackSound;           // �U�����̌��ʉ�
    private AudioSource audioSource;        // ���ʉ����Đ����邽�߂�AudioSource

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
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;

        ApplyBuffFromManuta();
        ApplyBuffFromIruka();
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

    public void PlayAttackEffect()
    {
        // **�G�t�F�N�g�̐���**
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
            Debug.Log("�U���G�t�F�N�g�𐶐����܂����I");
        }

        // **���ʉ��̍Đ�**
        if (attackSound != null && audioSource != null)
        {
            audioSource.clip = attackSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("�U���G�t�F�N�g�܂��̓T�E���h���ݒ肳��Ă��܂���I");
        }
    }

    public void PerformDashAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
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

        // **�U���G�t�F�N�g�ƃT�E���h�̍Đ�**
        PlayAttackEffect();

        // �U������
        if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRadius)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
            Debug.Log($"{target.name} �� {attackDamage} �̃_���[�W��^���܂����B");
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
        lastAttackTime = Time.time;
    }

    public void AttackOn()
    {
        if (target == null)
        {
            DetectEnemy();
        }
        else if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
        {
            PerformDashAttack(); // **�̓�����U�������s**
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

    public void TakeDamage(int damageAmount)
    {
        if (!isBuffApplied)
        {
            health -= damageAmount;
        }
        else
        {
            int reducedDamage = Mathf.RoundToInt(damageAmount * 0.25f); // �_���[�W��75%�y��
            health -= reducedDamage;
            Debug.Log($"Manuta�̋������ʂɂ��_���[�W���y������܂����B�󂯂��_���[�W: {reducedDamage}");
        }

        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void ApplyBuffFromManuta()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool manutaNearby = false;
        Debug.Log("�߂��ɂ���Manuta�����m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            Manuta manuta = collider.GetComponent<Manuta>();
            if (manuta != null && manuta.gameObject != gameObject)
            {
                manutaNearby = true;
                Debug.Log($"Manuta�����m���܂���: {manuta.name}");
                break;
            }
        }

        if (manutaNearby && !isBuffApplied)
        {
            attackCooldown *= 0.5f; // �U���p�x���オ��i�N�[���_�E�����Ԃ𔼕��ɂ���j
            isBuffApplied = true;  // �o�t���K�p���ꂽ���Ƃ��L�^
        }
        else if (!manutaNearby && isBuffApplied)
        {
            attackCooldown *= 2.0f; // �U���p�x�����ɖ߂�
            isBuffApplied = false; // �o�t������
        }
    }

    private void ApplyBuffFromIruka()
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
        maxHealth = 20;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
