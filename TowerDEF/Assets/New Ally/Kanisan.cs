using UnityEngine;

public class Kanisan : MonoBehaviour, IDamageable
{
    // Kanisan�̗̑�
    public int health = 10;
    public int maxHealth = 10;
    private bool maxHealthBuffApplied = false;

    // Kanisan�̍U���͂ƍU���֘A�̐ݒ�
    public int attackDamage = 3;
    public float detectionRadius = 10f;     // �G�����m����͈�
    public float attackCooldown = 1.5f;     // �U���̃N�[���_�E������

    private Transform target;               // �U���Ώۂ̓G
    private float lastAttackTime;           // �Ō�ɍU����������

    // GameManager�̎Q�Ƃ��C���X�y�N�^�[����ݒ�ł���悤�ɂ���
    [SerializeField]
    private GameManager gm;

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
    }

    private void OnMouseDown()
    {
        // �����N���b�N���ꂽ�Ƃ��A�񕜂����݂�
        TryHeal();
    }

    // �����N���b�N���ꂽ�Ƃ��ɌĂ΂��񕜃��\�b�h
    public void TryHeal()
    {
        // GameManager�őI������Ă����L������ꍇ
        if (gm.SelectedFeedType.HasValue)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            // �L���I�L�A�~�A�x���g�X�A�܂��̓v�����N�g���̏ꍇ�̂݉񕜉\
            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                // �L�̍݌ɂ�����ꍇ�̂݉񕜏������s��
                if (gm.inventory[selectedFeed] > 0)
                {
                    // �̗͂��ő�l�ɒB���Ă��Ȃ��ꍇ�A��
                    if (health < maxHealth)
                    {
                        health += 2; // �񕜗ʂ�ݒ�
                        health = Mathf.Min(health, maxHealth); // �ő�̗͂𒴂��Ȃ��悤�ɐ���
                        gm.inventory[selectedFeed]--; // �݌ɂ����炷
                        gm.UpdateResourceUI(); // ���\�[�XUI���X�V
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

    // �G�����m���čU�����郁�\�b�h
    public void AttackOn()
    {
        // �^�[�Q�b�g���ݒ肳��Ă��Ȃ��ꍇ�A�G�����m
        if (target == null)
        {
            DetectEnemy();
        }
        else
        {
            // �^�[�Q�b�g���͈͓��ɂ���A�U���N�[���_�E�����I�����Ă���ꍇ�ɍU��
            if (Vector3.Distance(transform.position, target.position) <= detectionRadius && Time.time > lastAttackTime + attackCooldown)
            {
                AttackTarget();
                lastAttackTime = Time.time; // �U�����Ԃ��X�V
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ���m�͈͂ɓ������I�u�W�F�N�g��Enemy�^�O�����ꍇ�A�^�[�Q�b�g�ɐݒ�
        if (other.CompareTag("Enemy") && target == null)
        {
            target = other.transform;
        }
    }

    // �͈͓��̓G�����m���ă^�[�Q�b�g�ɐݒ肷�郁�\�b�h
    void DetectEnemy()
    {
        // ���m�͈͓��ɂ��邷�ׂĂ�Collider���擾
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Debug.Log("�G�����m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            // �^�O��"Enemy"�ƈ�v����I�u�W�F�N�g���^�[�Q�b�g�Ƃ��Đݒ�
            if (collider.CompareTag("Enemy"))
            {
                target = collider.transform;
                Debug.Log($"�G�����m���܂���: {target.name}");
                break;
            }
        }
    }

    // �^�[�Q�b�g���U�����郁�\�b�h
    void AttackTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // �^�[�Q�b�g��IDamageable�����ꍇ�A�_���[�W��^����
            damageable.TakeDamage(attackDamage);
        }
    }

    // �_���[�W���󂯂��Ƃ��̏���
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    // Kanisan���|�ꂽ�Ƃ��̏���
    private void Die()
    {
        Destroy(gameObject); // �I�u�W�F�N�g��j��
    }

    // �߂��̃J�j����̐��ɉ����čU���͂Ƒ̗͂���������
    private void BuffKanisan()
    {
        if (maxHealthBuffApplied) return;

        // �߂��ɂ���J�j���������
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

        // �U���͂Ƒ̗͂�ݒ肷��
        attackDamage = 3 + nearbyKanisanCount * 5; // ��{�U���͂ɋ߂��̃J�j���񐔂ɉ�����5�����Z�i�ő�25�j
        maxHealth = 10 + nearbyKanisanCount * 5; // ��{�̗͂ɋ߂��̃J�j���񐔂ɉ�����5�����Z�i�ő�25�j
        health = Mathf.Min(health + nearbyKanisanCount * 5, maxHealth); // �ʏ�̗̑͂ɂ��߂��̃J�j���񐔂ɉ�����5�����Z

        maxHealthBuffApplied = true; // �o�t����x�����K�p�����悤�ɂ���
    }

    void OnDrawGizmosSelected()
    {
        // �V�[���r���[�Ō��m�͈͂�����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
