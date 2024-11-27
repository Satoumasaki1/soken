using UnityEngine;

public class Onagazame : MonoBehaviour, IDamageable
{
    // Onagazame�̗̑�
    public int health = 25;
    public int maxHealth = 25;

    // Onagazame�̍U���͂ƍU���֘A�̐ݒ�
    public int attackDamage = 10;
    public float detectionRadius = 15f;     // �G�����m����͈�
    public float attackCooldown = 1.5f;     // �U���̃N�[���_�E������
    public float stunDuration = 12.0f;       // �X�^�����ʂ̎�������

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
            PerformStunAttack();
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

    void PerformStunAttack()
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

                IStunnable stunnable = collider.GetComponent<IStunnable>();
                if (stunnable != null)
                {
                    stunnable.Stun(stunDuration);
                    Debug.Log($"{collider.name} �ɃX�^�����ʂ�^���܂����B��������: {stunDuration}�b");
                }
            }
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
