using UnityEngine;

public class CharacterUpgrade : MonoBehaviour
{
    public string characterName;
    public int newHP;
    public int newAttackPower;
    public int newSearchRange; // 捜索範囲 (特定キャラ用)

    public void ApplyUpgrade()
    {
        // キャラクターごとに適切なステータスを設定
        Debug.Log($"{characterName} has been upgraded!");

        if (newHP > 0) GetComponent<CharacterStats>().HP = newHP;
        if (newAttackPower > 0) GetComponent<CharacterStats>().attackPower = newAttackPower;
        if (newSearchRange > 0) GetComponent<CharacterStats>().searchRange = newSearchRange;
    }
}
