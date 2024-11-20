using UnityEngine;

public class IkaiKa : MonoBehaviour
{
    public int attackPower = 5;
    public float maxHealth = 10f;
    private float currentHealth;
    public float moveSpeed = 2f;
    public float attackRange = 6f;
    private float targetSearchInterval = 0.5f;
    private float nextTargetSearchTime = 0f;
    private Collider[] nearbyTargets = new Collider[10];
    private Transform currentTarget;
    public Transform baseTarget;  // �����̋��_���w�肷��^�[�Q�b�g
    public Transform[] waypoints; // �o���ʒu���狒�_�܂ł̃��[�g�������E�F�C�|�C���g
    private int currentWaypointIndex = 0;

    // ������
    void Start()
    {
        currentHealth = maxHealth;
        if (waypoints.Length > 0)
        {
            currentTarget = waypoints[currentWaypointIndex];  // �����̃^�[�Q�b�g���ŏ��̃E�F�C�|�C���g�ɐݒ�
        }
        else
        {
            currentTarget = baseTarget;  // �E�F�C�|�C���g���Ȃ��ꍇ�͋��_���^�[�Q�b�g�ɐݒ�
        }
        Debug.Log(gameObject.name + " initialized with max health: " + maxHealth);
    }

    void Update()
    {
        Debug.Log(gameObject.name + " Update called. Current target: " + (currentTarget != null ? currentTarget.name : "None"));
        if (currentTarget != null)
        {
            MoveTowardsTarget();
        }
        else if (Time.time >= nextTargetSearchTime)
        {
            Debug.Log(gameObject.name + " Searching for new target.");
            FindNewTarget();
            nextTargetSearchTime = Time.time + targetSearchInterval;
        }
    }

    // �_���[�W���󂯂����̏���
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took damage: " + damageAmount + ", current health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ���j�b�g�����S�����ۂ̏���
    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated.");
        Destroy(gameObject);
    }

    // �V�����U���Ώۂ�T���i�D�揇��: Ally�j
    private void FindNewTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, 15f, nearbyTargets);
        Debug.Log(gameObject.name + " found " + hitCount + " potential targets.");
        Transform bestTarget = baseTarget;  // �f�t�H���g�ŋ��_���^�[�Q�b�g�ɂ���

        for (int i = 0; i < hitCount; i++)
        {
            Collider target = nearbyTargets[i];
            if (target.CompareTag("Ally"))
            {
                Debug.Log(gameObject.name + " evaluating target: " + target.name);
                bestTarget = target.transform;
                break;  // Ally�^�O�����������炻�̃^�[�Q�b�g�ɂ���
            }
        }

        currentTarget = bestTarget;
        Debug.Log(gameObject.name + " new target set to: " + (currentTarget != null ? currentTarget.name : "None"));
    }

    // �U���ΏۂɌ������Ĉړ�����
    private void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            Vector3 targetPosition = currentTarget.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            Debug.Log(gameObject.name + " moving towards target: " + currentTarget.name);

            // �E�F�C�|�C���g�ɓ��B�������ǂ������`�F�b�N
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                if (currentTarget == baseTarget)
                {
                    return;  // ���݂̃^�[�Q�b�g�����_�ł���΁A���̂܂܍U���𑱂���
                }
                if (currentWaypointIndex < waypoints.Length - 1)
                {
                    currentWaypointIndex++;
                    currentTarget = waypoints[currentWaypointIndex];
                    Debug.Log(gameObject.name + " moving to next waypoint: " + currentTarget.name);
                }
                else
                {
                    currentTarget = baseTarget;
                    Debug.Log(gameObject.name + " moving to base target: " + currentTarget.name);
                }
            }
        }
    }

    // �U���͈͂ɓ������ΏۂɍU������
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " triggered with: " + other.name);
        if (other.transform == currentTarget && Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackPower);
                Debug.Log(gameObject.name + " attacked " + other.gameObject.name + " for " + attackPower + " damage.");
            }
        }
    }

    // �U���͈͂��������邽�߂� Gizmos �`��
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
