using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int health = 10;
    public int attackPower = 5;

    // 強化処理
    public void Upgrade()
    {
        health *= 2; // 体力を2倍に
        attackPower *= 2; // 攻撃力を2倍に
        Debug.Log($"{gameObject.name} has been upgraded! Health: {health}, Attack: {attackPower}");
    }
}
