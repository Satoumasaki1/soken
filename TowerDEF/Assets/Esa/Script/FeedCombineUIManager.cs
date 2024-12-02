using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedCombineUIManager : MonoBehaviour
{
    [SerializeField] private GameObject popupMenu;           // 合成ポップアップメニュー
    [SerializeField] private Button feedAButton;             // コマセボタン
    [SerializeField] private Button feedBButton;             // ミックスペレットボタン
    [SerializeField] private Button feedCButton;             // バイオフィードボタン
    [SerializeField] private TextMeshProUGUI resultText;     // 合成結果を表示するUI
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

        // ポップアップを最初は非表示にする
        popupMenu.SetActive(false);
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
        // ゲームを一時停止
        //GameManager.Instance.PauseGame();

        popupMenu.SetActive(true);
        resultText.text = "作りたい餌を選んでください";
    }

    // ポップアップを閉じるメソッド
    public void HidePopup()
    {
        // ゲームの一時停止を解除
        //GameManager.Instance.ResumeGame();

        popupMenu.SetActive(false);
    }
}
