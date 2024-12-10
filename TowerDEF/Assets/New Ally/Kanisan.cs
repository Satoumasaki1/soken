using UnityEngine;
using System.Collections;

public class Kanisan : MonoBehaviour, IDamageable, ISeasonEffect
{
    // Kanisan�̗̑�
    public int health = 10;
    public int maxHealth = 10;
    private bool maxHealthBuffApplied = false;
    public bool isBuffActive = false; // �o�t���L�����ǂ����̃t���O
    private int originalAttackDamage = 3;
    public int attackDamage = 3;
    public float buffMultiplier = 1.5f; // �U���o�t�̔{��

    // Kanisan�̍U���͂ƍU���֘A�̐ݒ�
    public float detectionRadius = 10f;     // �G�����m����͈�
    public float attackRange = 5f;          // �U���͈�
    public float attackCooldown = 1.5f;     // �U���̃N�[���_�E������

    private Transform target;               // �U���Ώۂ̓G
    private float lastAttackTime;           // �Ō�ɍU����������

    // GameManager�̎Q�Ƃ��C���X�y�N�^�[����ݒ�ł���悤�ɂ���
    [SerializeField]
    private GameManager gm;

    private bool seasonEffectApplied = false;

    void Start()
    {
        // GameManager�̎Q�Ƃ��C���X�y�N�^�[�Őݒ肳��Ă��Ȃ��ꍇ�A�����I�Ɏ擾
        if (gm == null)
        {
            gm = GameManager.Instance != null ? GameManager.Instance : GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // GameManager�̎Q�Ƃ��擾�ł��Ȃ������ꍇ�A�G���[���b�Z�[�W��\��
        if (gm == null)
        {
            Debug.LogError("GameManager�̎Q�Ƃ�������܂���BGameManager���������ݒ肳��Ă��邩�m�F���Ă��������B");
        }
    }

    void Update()
    {
        // �ꎞ��~����AttackOn()�����s�����ɖ߂�
        if (gm != null && gm.isPaused)
        {
            return;
        }

        // �ꎞ��~����Ă��Ȃ��ꍇ�A�U�����������s
        AttackOn();

        // �߂��̃J�j����̐��ɉ����čU���͂Ƒ̗͂�����
        BuffKanisan();

        // �C���J����̃o�t��K�p
        ApplyIrukaBuff();
    }

    private void OnMouseDown()
    {
        // �����N���b�N���ꂽ�Ƃ��A�񕜂����݂�
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
                        health += 2;
                        health = Mathf.Min(health, maxHealth);
                        gm.inventory[selectedFeed]--;
                        gm.UpdateResourceUI();
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
                Debug.Log("�����L�ł͉񕜂ł��܂���B");
            }
        }
        else
        {
            Debug.Log("�L���I������Ă��܂���B");
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
                target = null; // �^�[�Q�b�g���͈͊O�ɏo���ꍇ���Z�b�g
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
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
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage);
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
        Debug.Log("�����̃J�j��������m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            Kanisan kanisan = collider.GetComponent<Kanisan>();
            if (kanisan != null && kanisan.gameObject != gameObject)
            {
                nearbyKanisanCount++;
                Debug.Log($"�����̃J�j��������m���܂���: {kanisan.name}");
                if (nearbyKanisanCount >= 5)
                {
                    nearbyKanisanCount = 5;
                    break;
                }
            }
        }

        attackDamage = 3 + nearbyKanisanCount * 5;
        maxHealth = 10 + nearbyKanisanCount * 5;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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
        maxHealth = 10;
        health = Mathf.Min(health, maxHealth);
        attackDamage = originalAttackDamage;
        seasonEffectApplied = false;
        Debug.Log("�V�[�Y�����ʂ����Z�b�g����܂����B");
    }
}
