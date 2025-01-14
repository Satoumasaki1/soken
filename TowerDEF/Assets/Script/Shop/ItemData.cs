using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName; // �A�C�e����
    public Sprite icon; // �A�C�e���̃A�C�R��
    public GameObject targetCharacter; // �����Ώۂ̃L�����N�^�[
    public int cost; // �A�C�e���̉��i
    public int additionalHp; // ���������HP�̗�
    public int additionalDamage; // ���������_���[�W�̗�

    public void ApplyEffect()
    {
        if (targetCharacter != null)
        {
            IUpgradable upgradable = targetCharacter.GetComponent<IUpgradable>();
            if (upgradable != null)
            {
                upgradable.Upgrade(additionalHp, additionalDamage);

                // �L�����N�^�[�̏�Ԃ𑦍��ɕۑ�
                if (targetCharacter.TryGetComponent(out Kanisan kanisan))
                {
                    kanisan.SaveState();
                }
                /*else if (targetCharacter.TryGetComponent(out Syako syako))
                {
                    syako.SaveState();
                }*/
                // ���̃L�����N�^�[�������ɒǉ�
            }
            else
            {
                Debug.LogWarning("Target character does not implement IUpgradable: " + targetCharacter.name);
            }
        }
    }
}

