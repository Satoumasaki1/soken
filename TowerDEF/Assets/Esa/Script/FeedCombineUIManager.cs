using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FeedCombineUIManager : MonoBehaviour
{
    [SerializeField] private GameObject popupMenu;           // 合成ポップアップメニュー
    [SerializeField] private Button feedAButton;             // コマセボタン
    [SerializeField] private Button feedBButton;             // ミックスペレットボタン
    [SerializeField] private Button feedCButton;             // バイオフィードボタン
    [SerializeField] private TextMeshProUGUI resultText;     // 合成結果を表示するUI
    [SerializeField] private TextMeshProUGUI descriptionText; // 餌の説明を表示するUI
    [SerializeField] private Button closeButton;             // ポップアップを閉じるボタン

    public GameManager gm;
    private GameManager.ResourceType selectedFeedType;       // 選択された合成餌のタイプ

    private void Start()
    {
        // ボタンにクリックイベントを設定
        feedAButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedA));
        feedBButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedB));
        feedCButton.onClick.AddListener(() => SelectFeedToCreate(GameManager.ResourceType.FeedC));
        closeButton.onClick.AddListener(HidePopup);

        gm = GameManager.Instance;

        // ボタンにホバーイベントを追加（スクリプト内で説明文を設定）
        AddHoverEvents(feedAButton, "コマセ: 小魚用の餌です。オキアミとプランクトンを使って作成します。");
        AddHoverEvents(feedBButton, "ミックスペレット: バランスの取れた餌。オキアミとベントスを使って作成します。");
        AddHoverEvents(feedCButton, "バイオフィード: 特殊な餌。プランクトンとベントスを使って作成します。");

        // ポップアップを最初は非表示にする
        //popupMenu.SetActive(false);
    }

    // 合成餌を選択するメソッド
    private void SelectFeedToCreate(GameManager.ResourceType feedType)
    {
        selectedFeedType = feedType;
        CreateFeed(selectedFeedType);
    }

    // 合成餌を作成するメソッド
    private void CreateFeed(GameManager.ResourceType feedType)
    {
        var inventory = GameManager.Instance.inventory;

        switch (feedType)
        {
            case GameManager.ResourceType.FeedA:
                if (inventory[GameManager.ResourceType.OkiaMi] > 0 && inventory[GameManager.ResourceType.Plankton] > 0)
                {
                    inventory[GameManager.ResourceType.OkiaMi]--;
                    inventory[GameManager.ResourceType.Plankton]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "合成成功: コマセが作成されました！";
                }
                else
                {
                    resultText.text = "合成失敗: 在庫が足りません";
                }
                break;

            case GameManager.ResourceType.FeedB:
                if (inventory[GameManager.ResourceType.OkiaMi] > 0 && inventory[GameManager.ResourceType.Benthos] > 0)
                {
                    inventory[GameManager.ResourceType.OkiaMi]--;
                    inventory[GameManager.ResourceType.Benthos]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "合成成功: ミックスペレットが作成されました！";
                }
                else
                {
                    resultText.text = "合成失敗: 在庫が足りません";
                }
                break;

            case GameManager.ResourceType.FeedC:
                if (inventory[GameManager.ResourceType.Plankton] > 0 && inventory[GameManager.ResourceType.Benthos] > 0)
                {
                    inventory[GameManager.ResourceType.Plankton]--;
                    inventory[GameManager.ResourceType.Benthos]--;
                    inventory[feedType]++;
                    gm.UpdateResourceUI();
                    resultText.text = "合成成功: バイオフィードが作成されました！";
                }
                else
                {
                    resultText.text = "合成失敗: 在庫が足りません";
                }
                break;

            default:
                resultText.text = "無効な餌タイプ";
                break;
        }
    }

    // ポップアップを表示するメソッド
    public void ShowPopup()
    {
        popupMenu.SetActive(true);
        resultText.text = "作りたい餌を選んでください";
    }

    // ポップアップを閉じるメソッド
    public void HidePopup()
    {
        popupMenu.SetActive(false);
        descriptionText.text = ""; // 説明をクリア
    }

    // ボタンにホバーイベントを追加するメソッド
    private void AddHoverEvents(Button button, string description)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // ホバー時に説明を表示
        EventTrigger.Entry entryEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entryEnter.callback.AddListener((data) => { descriptionText.text = description; });
        trigger.triggers.Add(entryEnter);

        // ホバーが外れたときに説明を消す
        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((data) => { descriptionText.text = ""; });
        trigger.triggers.Add(entryExit);
    }
}
