using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName; // アイテム名
    public Sprite icon; // アイテムのアイコン
    public GameObject targetCharacter; // 強化対象のキャラクター
    public int cost; // アイテムの価格
    public int additionalHp; // 強化されるHPの量
    public int additionalDamage; // 強化されるダメージの量
    public int additionaRadius; //強化される回復量

    public void ApplyEffect()
    {
        if (targetCharacter != null)
        {
            IUpgradable upgradable = targetCharacter.GetComponent<IUpgradable>();
            if (upgradable != null)
            {
                upgradable.Upgrade(additionalHp, additionalDamage, additionaRadius);

                // キャラクターの状態を即座に保存
                //カニ
                if (targetCharacter.TryGetComponent(out Kanisan kanisan))
                {
                    kanisan.SaveState();
                }
                //カサゴ
                else if (targetCharacter.TryGetComponent(out HanaminoKasago hanaminoKasago))
                {
                    hanaminoKasago.SaveState();
                }
                // 他のキャラクターもここに追加
                //ハゼ
                else if (targetCharacter.TryGetComponent(out Haze haze))
                {
                    haze.SaveState();
                }
                //コバンザメ
                else if (targetCharacter.TryGetComponent(out Kobanuzame kobanuzame))
                {
                    kobanuzame.SaveState();
                }
                //マンタ
                else if (targetCharacter.TryGetComponent(out Manuta manuta))
                {
                    manuta.SaveState();
                }
                //サメ
                else if (targetCharacter.TryGetComponent(out Onagazame onagazame))
                {
                    onagazame.SaveState();
                }
                //
                else if (targetCharacter.TryGetComponent(out Udeppo udeppo))
                {
                    udeppo.SaveState();
                }
                //ウニ
                else if (targetCharacter.TryGetComponent(out Uni1 uni1))
                {
                    uni1.SaveState();
                }
                //イルカ
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

