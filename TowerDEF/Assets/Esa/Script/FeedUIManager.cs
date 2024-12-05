using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // 辞書のために追加

public class FeedUIManager : MonoBehaviour
{
    // UIテキスト（餌の種類を表示）
    [SerializeField]
    public TextMeshProUGUI feedTypeText;

    // 餌の在庫を確認するためにGameManagerのインスタンスを参照
    private GameManager gameManager;

    // UIボタンのリスト（餌のボタン）
    [SerializeField] private Button[] feedButtons;

    // 餌の名前を日本語で表示するための辞書
    private Dictionary<GameManager.ResourceType, string> feedTypeNames = new Dictionary<GameManager.ResourceType, string>
    {
        { GameManager.ResourceType.OkiaMi, "オキアミ" },
        { GameManager.ResourceType.Benthos, "ベントス" },
        { GameManager.ResourceType.Plankton, "プランクトン" },
        { GameManager.ResourceType.FeedA, "コマセ" },
        { GameManager.ResourceType.FeedB, "ミックスペレット" },
        { GameManager.ResourceType.FeedC, "バイオフィード" }
    };

    private void Start()
    {
        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;

        // ボタンにクリックリスナーを追加
        feedButtons[0].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.OkiaMi));
        feedButtons[1].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.Benthos));
        feedButtons[2].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.Plankton));
        feedButtons[3].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.FeedA));
        feedButtons[4].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.FeedB));
        feedButtons[5].onClick.AddListener(() => SetFeedType(GameManager.ResourceType.FeedC));

        // 最初に在庫の更新を行う
        //UpdateButtonStates();
        //最初に持っている餌の情報をGameManagerに通知する
        SetFeedType(GameManager.ResourceType.OkiaMi);
    }

    // 餌の種類を選択し、GameManagerに通知するメソッド
    public void SetFeedType(GameManager.ResourceType feedType)
    {
        // GameManagerに選ばれた餌タイプを通知する
        GameManager.Instance.SetSelectedFeedType(feedType);
        Debug.Log("選択された餌: " + feedType);
        UpdateFeedTypeUI(); // 餌タイプが変更された際にUIを更新
    }

    // UI表示を更新するメソッド
    public void UpdateFeedTypeUI()
    {
        if (GameManager.Instance.SelectedFeedType.HasValue)
        {
            GameManager.ResourceType selectedFeed = GameManager.Instance.SelectedFeedType.Value;

            // 日本語の餌名を取得して表示
            if (feedTypeNames.TryGetValue(selectedFeed, out string feedName))
            {
                feedTypeText.text = feedName;
            }
            else
            {
                feedTypeText.text = "不明な餌"; // 辞書にない場合
            }
        }
        else
        {
            feedTypeText.text = "餌が選択されていません";
        }
    }

    // 餌のボタンの状態（有効/無効）を在庫に基づいて更新
    public void UpdateButtonStates()
    {
        for (int i = 0; i < feedButtons.Length; i++)
        {
            GameManager.ResourceType feedType = (GameManager.ResourceType)i;
            feedButtons[i].interactable = gameManager.inventory[feedType] > 0; // inventoryを使用してボタンの有効/無効を設定
        }
    }
}
