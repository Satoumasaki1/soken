using UnityEngine;

public class Kaisou : MonoBehaviour
{
    public float slowDownFactor = 0.5f; // エネミーの速度をどれくらい遅くするか
    public float slowDuration = 2.0f; // 速度低下が続く時間

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

        enemy.moveSpeed = originalSpeed; // 一定時間後に元の速度に戻す
    }*/
}
