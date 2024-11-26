using UnityEngine;

public class Manuta : MonoBehaviour, IDamageable
{
    // Manuta�̗̑�
    public int health = 20;
    public int maxHealth = 20;
    private bool isBuffApplied = false;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 5;
    public int attackDamage = 5;
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��

    // Manuta�̍U���͂ƍU���֘A�̐ݒ�
    public float detectionRadius = 10f;     // �G�����m����͈�
    public float attackCooldown = 1.0f;     // �U���̃N�[���_�E������

    private Transform target;               // �U���Ώۂ̓G
    private float lastAttackTime;           // �Ō�ɍU����������

    // GameManager�̎Q�Ƃ��C���X�y�N�^�[����ݒ�ł���悤�ɂ���
    [SerializeField]
    private GameManager gm;

    void Start()
    {
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }
    }

    void Update()
    {
        if (gm != null && gm.isPaused) return;
        ApplyBuffFromKobanuzame();
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
        if (target != null)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
            Debug.Log($"{target.name} �� {attackDamage} �̃_���[�W��^���܂����B");
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

    private void ApplyBuffFromKobanuzame()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool kobanuzameNearby = false;
        Debug.Log("�߂��ɂ���Kobanuzame�����m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            Kobanuzame kobanuzame = collider.GetComponent<Kobanuzame>();
            if (kobanuzame != null && kobanuzame.gameObject != gameObject)
            {
                kobanuzameNearby = true;
                Debug.Log($"Kobanuzame�����m���܂���: {kobanuzame.name}");
                break;
            }
        }

        if (kobanuzameNearby && !isBuffApplied)
        {
            maxHealth *= 3; // �̗͂�3�{�ɂ���
            health = Mathf.Min(health * 3, maxHealth); // ���݂̗̑͂�3�{�ɂ��A�ő�̗͂𒴂��Ȃ��悤�ɂ���
            attackDamage += 5; // �U���͂�5����
            isBuffApplied = true;  // �o�t���K�p���ꂽ���Ƃ��L�^
        }
        else if (!kobanuzameNearby && isBuffApplied)
        {
            maxHealth /= 3; // �̗͂����ɖ߂�
            health = Mathf.Min(health, maxHealth); // ���݂̗̑͂��ő�̗͂ɍ��킹��
            attackDamage -= 5; // �U���͂����ɖ߂�
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
}
