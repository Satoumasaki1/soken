using UnityEngine;
using UnityEngine.UI;

public class LevelUpItem : MonoBehaviour
{
    public int itemCount = 3; // �����Ă���A�C�e���̐�
    public GameObject targetCharacter; // ���x���A�b�v���������L�����N�^�[�iUni�Ȃǁj
    public GameObject targetCharacter2;
    public GameObject targetCharacter3;
    public GameObject targetCharacter4;
    public GameObject targetCharacter5;
    public GameObject targetCharacter6;

    private void Update()
    {
        // �E�N���b�N�Ń��x���A�b�v
        if (Input.GetMouseButtonDown(1)) // �E�N���b�N
        {
            if (targetCharacter != null)
            {
                Uni uniScript = targetCharacter.GetComponent<Uni>();
                if (uniScript != null)
                {
                    TryLevelUp(uniScript);
                }
            }
        }
    }

    // ���x���A�b�v�����݂�
    private void TryLevelUp(Uni character)
    {
        int currentLevel = character.level;

        // ���x��3�ȏ�ɂ͂ł��Ȃ�
        if (currentLevel >= character.maxLevel)
        {
            Debug.Log("Already at max level.");
            return;
        }

        // ���x���A�b�v�ɕK�v�ȃA�C�e�������m�F
        int requiredItems = (currentLevel == 1) ? 1 : 2;

        if (itemCount >= requiredItems)
        {
            itemCount -= requiredItems; // �K�v�Ȑ��̃A�C�e��������
            character.LevelUp(); // ���x���A�b�v
            Debug.Log("Level up! Remaining items: " + itemCount);
        }
        else
        {
            Debug.Log("Not enough items to level up.");
        }
    }
}
