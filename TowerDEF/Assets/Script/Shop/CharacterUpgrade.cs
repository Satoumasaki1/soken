using UnityEngine;

public class CharacterUpgrade : MonoBehaviour
{
    public string characterName;
    public int newHP;
    public int newAttackPower;
    public int newSearchRange; // �{���͈� (����L�����p)

    public void ApplyUpgrade()
    {
        // �L�����N�^�[���ƂɓK�؂ȃX�e�[�^�X��ݒ�
        Debug.Log($"{characterName} has been upgraded!");

        if (newHP > 0) GetComponent<CharacterStats>().HP = newHP;
        if (newAttackPower > 0) GetComponent<CharacterStats>().attackPower = newAttackPower;
        if (newSearchRange > 0) GetComponent<CharacterStats>().searchRange = newSearchRange;
    }
}
