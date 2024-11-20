using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 3f; // �G�̈ړ����x
    public int damage = 10; // �G���^����_���[�W
    public Transform[] waypoints; // �E�F�C�|�C���g�z��

    private int currentWaypointIndex = 0; // ���݂̃E�F�C�|�C���g�̃C���f�b�N�X

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[currentWaypointIndex].position;
        }
    }

    void Update()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        MoveTowardsWaypoint();
    }

    void MoveTowardsWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // �E�F�C�|�C���g�ɓ��B�����ꍇ
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;

            // �Ō�̃E�F�C�|�C���g�ɓ��B������
            if (currentWaypointIndex >= waypoints.Length)
            {
                ReachBase();
            }
        }
    }

    void ReachBase()
    {
        // ����ɓ��B�����Ƃ��̏����i��: �_���[�W��^����Ȃǁj
        Debug.Log("Enemy has reached the base!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �^�[�Q�b�g�ɐڐG������_���[�W��^����
        if (collision.gameObject.CompareTag("Ally"))
        {
            Ally ally = collision.gameObject.GetComponent<Ally>();
            if (ally != null)
            {
                ally.TakeDamage(damage);
            }
        }
    }
}

public class Ally : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Ally health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // ���������S�����ۂ̏���
        Debug.Log("Ally has been defeated!");
        Destroy(gameObject);
    }
}
