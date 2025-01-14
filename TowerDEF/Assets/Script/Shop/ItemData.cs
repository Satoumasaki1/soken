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

    public void ApplyEffect()
    {
        if (targetCharacter != null)
        {
            IUpgradable upgradable = targetCharacter.GetComponent<IUpgradable>();
            if (upgradable != null)
            {
                upgradable.Upgrade(additionalHp, additionalDamage);

                // キャラクターの状態を即座に保存
                if (targetCharacter.TryGetComponent(out Kanisan kanisan))
                {
                    kanisan.SaveState();
                }
                /*else if (targetCharacter.TryGetComponent(out Syako syako))
                {
                    syako.SaveState();
                }*/
                // 他のキャラクターもここに追加
            }
            else
            {
                Debug.LogWarning("Target character does not implement IUpgradable: " + targetCharacter.name);
            }
        }
    }
}

