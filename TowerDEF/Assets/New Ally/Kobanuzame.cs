using UnityEngine;

public class Kobanuzame : MonoBehaviour, IDamageable
{
    // Kobanuzame�̗̑�
    public int health = 20;
    public int maxHealth = 20;
    private bool isBuffApplied = false;

    // Kobanuzame�̍U���͂ƍU���֘A�̐ݒ�
    public int attackDamage = 5;
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

        ApplyBuffFromManuta();
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
        if (!isBuffApplied)
        {
            health -= damageAmount;
            if (health <= 0) Die();
        }
        else
        {
            Debug.Log("Manuta�̋������ʂɂ��_���[�W�𖳌������܂����B");
        }
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
