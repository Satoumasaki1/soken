using UnityEngine;

public class BreakableObject1 : MonoBehaviour
{
    public int hp = 5; // ヒットポイント

    // IsBrokenプロパティの追加
    public bool IsBroken
    {
        get { return hp <= 0; } // hpが0以下なら破壊済みと判定
    }

    public void TakeDamage()
    {
        hp--; // ダメージを受けるとhpを減少
        if (hp <= 0)
        {
            Debug.Log($"{gameObject.name} が破壊されました！");
            Destroy(gameObject); // オブジェクトを破壊
        }
    }
}