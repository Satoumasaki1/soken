using UnityEngine;
using System.Collections;

public class Haze : MonoBehaviour, IDamageable
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
    public float attackCooldown = 1.0f;

    private Transform target;
    private float lastAttackTime;

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
}
