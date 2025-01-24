using UnityEngine;
using UnityEngine.UI;

public class Udeppo : MonoBehaviour, IDamageable, IUpgradable
{
    // Udeppo�̗̑�
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 10; // ���̍U����

    // �U���֘A�̐ݒ�
    public int attackDamage = 10;
    public float detectionRadius = 20f;     // �G�����m����͈́i�˒��������j
    public float attackCooldown = 3.0f;     // �U���̃N�[���_�E�����ԁi�U���p�x�͒x���j

    // **���������U���̓����֘A**
    public float slamDistance = 2.0f;      // ���������U���őO�i���鋗��
    public float slamDuration = 0.3f;      // �O�i�ɂ����鎞��
    public float returnDuration = 0.3f;    // ��ނɂ����鎞��
    private bool isAttacking = false;      // �U�������ǂ����𔻒肷��t���O

    // **�U���G�t�F�N�g���T�E���h�ݒ�**
    [Header("�U���G�t�F�N�g�ݒ�")]
    public GameObject slamEffectPrefab;    // ���������U���G�t�F�N�g�̃v���n�u
    public Transform effectSpawnPoint;     // �G�t�F�N�g�𐶐�����ʒu
    public AudioClip slamSound;            // ���������U�����̌��ʉ�
    private AudioSource audioSource;       // ���ʉ����Đ����邽�߂�AudioSource

    // **�w���X�o�[�֘A**
    [Header("�w���X�o�[�ݒ�")]
    public GameObject healthBarPrefab;     // �w���X�o�[�̃v���n�u
    private GameObject healthBarInstance;  // ���ۂɐ������ꂽ�w���X�o�[
    private Slider healthSlider;           // �w���X�o�[�̃X���C�_�[�R���|�[�l���g
    private Transform cameraTransform;     // ���C���J������Transform

    // GameManager�̎Q��
    [SerializeField]
    private GameManager gm;

    private float lastAttackTime;          // �Ō�ɍU����������
    private Transform target;              // �U���Ώۂ̓G

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

        ApplyBuffFromHaze();
        ApplyBuffFromIruka();
        AttackOn();
        UpdateHealthBar();
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

    public void PlaySlamEffect()
    {
        if (slamEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(slamEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(effect, 2.0f);
            Debug.Log("���������U���G�t�F�N�g�𐶐����܂����I");
        }

        if (slamSound != null && audioSource != null)
        {
            audioSource.clip = slamSound;
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
            if (!isAttacking) StartCoroutine(PerformSlamAttack());
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

    private System.Collections.IEnumerator PerformSlamAttack()
    {
        isAttacking = true;

        // **�O�i�t�F�[�Y**
        Vector3 startPosition = transform.position;
        Vector3 slamPosition = transform.position + transform.forward * slamDistance;
        float elapsedTime = 0f;

        while (elapsedTime < slamDuration)
        {
            transform.position = Vector3.Lerp(startPosition, slamPosition, elapsedTime / slamDuration);
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
            }
        }

        // **�G�t�F�N�g���T�E���h�̍Đ�**
        PlaySlamEffect();

        // **��ރt�F�[�Y**
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(slamPosition, startPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
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

    private void ApplyBuffFromHaze()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool isHazeNearby = false;
        foreach (Collider collider in colliders)
        {
            Haze haze = collider.GetComponent<Haze>();
            if (haze != null && haze.gameObject != gameObject)
            {
                isHazeNearby = true;
                break;
            }
        }

        if (isHazeNearby && !maxHealthBuffApplied)
        {
            maxHealth += 20;
            health = Mathf.Min(health + 20, maxHealth);
            detectionRadius *= 2;
            attackDamage *= 2;
            maxHealthBuffApplied = true;
        }
        else if (!isHazeNearby && maxHealthBuffApplied)
        {
            maxHealth -= 20;
            health = Mathf.Min(health, maxHealth);
            detectionRadius /= 2;
            attackDamage /= 2;
            maxHealthBuffApplied = false;
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
                    attackDamage = Mathf.RoundToInt(attackDamage * 1.5f);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}