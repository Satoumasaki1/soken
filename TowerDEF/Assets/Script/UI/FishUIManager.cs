using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishUIManager : MonoBehaviour
{

    // 餌の在庫を確認するためにGameManagerのインスタンスを参照
    private GameManager gameManager;

    // UIボタンのリスト（餌のボタン）
    [SerializeField] private Button[] fishButtons;

    // Start is called before the first frame update
    void Start()
    {
        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;

        // ボタンにクリックリスナーを追加
        fishButtons[0].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Kani));
        fishButtons[1].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Tyoutyou));
        fishButtons[2].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Kaisou));
        fishButtons[3].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Syako));
        fishButtons[4].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Koban));
        fishButtons[5].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Teppou));
        fishButtons[6].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Manta));
        fishButtons[7].onClick.AddListener(() => SetFishType(GameManager.ResourceFishType.Uni));
        // 最初に在庫の更新を行う
        UpdateButtonStates();
    }

    // 餌の種類を選択し、GameManagerに通知するメソッド
    public void SetFishType(GameManager.ResourceFishType fishType)
    {
        // GameManagerに選ばれた餌タイプを通知する
        GameManager.Instance.SetSelectedFishType(fishType);
        Debug.Log("選択された餌: " + fishType);
    }

    // 餌のボタンの状態（有効/無効）を在庫に基づいて更新
    private void UpdateButtonStates()
    {
        for (int i = 0; i < fishButtons.Length; i++)
        {
            GameManager.ResourceFishType fishType = (GameManager.ResourceFishType)i;
            fishButtons[i].interactable = gameManager.finventory[fishType] > 0; // inventoryを使用してボタンの有効/無効を設定
        }
    }

}
