using UnityEngine;
using UnityEngine.UI;

public class LevelUpItem : MonoBehaviour
{
    public int itemCount = 3; // 持っているアイテムの数
    public GameObject targetCharacter; // レベルアップさせたいキャラクター（Uniなど）
    public GameObject targetCharacter2;
    public GameObject targetCharacter3;
    public GameObject targetCharacter4;
    public GameObject targetCharacter5;
    public GameObject targetCharacter6;

    private void Update()
    {
        // 右クリックでレベルアップ
        if (Input.GetMouseButtonDown(1)) // 右クリック
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

    // レベルアップを試みる
    private void TryLevelUp(Uni character)
    {
        int currentLevel = character.level;

        // レベル3以上にはできない
        if (currentLevel >= character.maxLevel)
        {
            Debug.Log("Already at max level.");
            return;
        }

        // レベルアップに必要なアイテム数を確認
        int requiredItems = (currentLevel == 1) ? 1 : 2;

        if (itemCount >= requiredItems)
        {
            itemCount -= requiredItems; // 必要な数のアイテムを消費
            character.LevelUp(); // レベルアップ
            Debug.Log("Level up! Remaining items: " + itemCount);
        }
        else
        {
            Debug.Log("Not enough items to level up.");
        }
    }
}
