using UnityEngine;

public class Kaisou : MonoBehaviour
{
    public float slowDownFactor = 0.5f; // �G�l�~�[�̑��x���ǂꂭ�炢�x�����邩
    public float slowDuration = 2.0f; // ���x�ቺ����������

    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyMovement enemyMovement = other.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                StartCoroutine(SlowDownEnemy(enemyMovement));
            }
        }
    }

    private IEnumerator SlowDownEnemy(EnemyMovement enemy)
    {
        float originalSpeed = enemy.moveSpeed;
        enemy.moveSpeed *= slowDownFactor;

        yield return new WaitForSeconds(slowDuration);

        enemy.moveSpeed = originalSpeed; // ��莞�Ԍ�Ɍ��̑��x�ɖ߂�
    }*/
}
