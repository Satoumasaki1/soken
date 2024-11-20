using UnityEngine;

public class Udeppo : MonoBehaviour, IDamageable
{
    // Udeppo�̗̑�
    public int health = 20;
    public int maxHealth = 20;
    private bool maxHealthBuffApplied = false;

    // TeppoEbi�̍U���͂ƍU���֘A�̐ݒ�
    public int attackDamage = 10;
    public float detectionRadius = 20f;     // �G�����m����͈́i�˒��������j
    public float attackCooldown = 3.0f;     // �U���̃N�[���_�E�����ԁi�U���p�x�͒x���j

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
        ApplyBuffFromHaze();
        AttackOn();
    }

    private void OnMouseDown()
    {
        // �e�b�|�E�G�r���N���b�N���ꂽ�Ƃ��A�񕜂����݂�
        TryHeal();
    }

    // �e�b�|�E�G�r���N���b�N���ꂽ�Ƃ��ɌĂ΂��񕜃��\�b�h
    public void TryHeal()
    {
        // GameManager�őI������Ă���a������ꍇ
        if (gm.SelectedFeedType.HasValue)
        {
            var selectedFeed = gm.SelectedFeedType.Value;

            // �a���I�L�A�~�A�x���g�X�A�܂��̓v�����N�g���̏ꍇ�̂݉񕜉\
            if (selectedFeed == GameManager.ResourceType.OkiaMi ||
                selectedFeed == GameManager.ResourceType.Benthos ||
                selectedFeed == GameManager.ResourceType.Plankton)
            {
                // �a�̍݌ɂ�����ꍇ�̂݉񕜏������s��
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
                Debug.Log("���̉a�ł͉񕜂ł��܂���B");
            }
        }
        else
        {
            Debug.Log("�a���I������Ă��܂���B");
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
            Debug.Log($"{target.name} �� {attackDamage} �̃_���[�W��^���܂����B");
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

    // TeppoEbi���|�ꂽ�Ƃ��̏���
    private void Die()
    {
        Destroy(gameObject); // �I�u�W�F�N�g��j��
    }

    // �߂��Ƀn�[������ꍇ�A�̗͂ƍő�̗͂𑝉�������ǉ�����
    private void ApplyBuffFromHaze()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        bool isHazeNearby = false;
        Debug.Log("�߂��ɂ���n�[�����m���Ă��܂�...");
        foreach (Collider collider in colliders)
        {
            Haze haze = collider.GetComponent<Haze>();
            if (haze != null && haze.gameObject != gameObject)
            {
                isHazeNearby = true;
                Debug.Log($"�n�[�����m���܂���: {haze.name}");
                break;
            }
        }

        // �̗͂ƍő�̗͂�ݒ肷��
        if (isHazeNearby && !maxHealthBuffApplied)
        {
            maxHealth += 20; // �n�[������ꍇ�A�ő�̗͂�20����
            health = Mathf.Min(health + 20, maxHealth); // ���݂̗̑͂�20�������A�ő�̗͂𒴂��Ȃ��悤�ɐ���
            detectionRadius *= 2; // �n�[������ꍇ�A���m�͈͂�2�{�ɂ���
            attackDamage *= 2; // �n�[������ꍇ�A�˒���2�{�ɂ���
            maxHealthBuffApplied = true; // �o�t���K�p���ꂽ���Ƃ��L�^
        }
        else if (!isHazeNearby && maxHealthBuffApplied)
        {
            maxHealth -= 20; // �n�[�����Ȃ��Ȃ����ꍇ�A�ő�̗͂����ɖ߂�
            health = Mathf.Min(health, maxHealth); // ���݂̗̑͂��ő�̗͂ɍ��킹��
            detectionRadius /= 2; // ���m�͈͂����ɖ߂�
            attackDamage /= 2; // �˒������ɖ߂�
            maxHealthBuffApplied = false; // �o�t������
        }
    }

    void OnDrawGizmosSelected()
    {
        // �V�[���r���[�Ō��m�͈͂�����
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
