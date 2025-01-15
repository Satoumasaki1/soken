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
    public int additionaRadius; //���������񕜗�

    public void ApplyEffect()
    {
        if (targetCharacter != null)
        {
            IUpgradable upgradable = targetCharacter.GetComponent<IUpgradable>();
            if (upgradable != null)
            {
                upgradable.Upgrade(additionalHp, additionalDamage, additionaRadius);

                // �L�����N�^�[�̏�Ԃ𑦍��ɕۑ�
                //�J�j
                if (targetCharacter.TryGetComponent(out Kanisan kanisan))
                {
                    kanisan.SaveState();
                }
                //�J�T�S
                else if (targetCharacter.TryGetComponent(out HanaminoKasago hanaminoKasago))
                {
                    hanaminoKasago.SaveState();
                }
                // ���̃L�����N�^�[�������ɒǉ�
                //�n�[
                else if (targetCharacter.TryGetComponent(out Haze haze))
                {
                    haze.SaveState();
                }
                //�R�o���U��
                else if (targetCharacter.TryGetComponent(out Kobanuzame kobanuzame))
                {
                    kobanuzame.SaveState();
                }
                //�}���^
                else if (targetCharacter.TryGetComponent(out Manuta manuta))
                {
                    manuta.SaveState();
                }
                //�T��
                else if (targetCharacter.TryGetComponent(out Onagazame onagazame))
                {
                    onagazame.SaveState();
                }
                //
                else if (targetCharacter.TryGetComponent(out Udeppo udeppo))
                {
                    udeppo.SaveState();
                }
                //�E�j
                else if (targetCharacter.TryGetComponent(out Uni1 uni1))
                {
                    uni1.SaveState();
                }
                //�C���J
                else if (targetCharacter.TryGetComponent(out Iruka iruka))
                {
                    iruka.SaveState();
                }
            }
            else
            {
                Debug.LogWarning("Target character does not implement IUpgradable: " + targetCharacter.name);
            }
        }
    }
}

