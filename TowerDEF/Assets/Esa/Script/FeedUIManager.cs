using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedUIManager : MonoBehaviour
{
    // UIテキストまたはImageコンポーネント（餌の種類を表示）
    [SerializeField]
    public TextMeshProUGUI feedTypeText;

    // 餌の在庫を確認するためにGameManagerのインスタンスを参照
    private GameManager gameManager;

    // UIボタンのリスト（餌のボタン）
    [SerializeField] private Button[] feedButtons;

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
        UpdateButtonStates();
    }

    // 餌の種類を選択し、GameManagerに通知するメソッド
    public void SetFeedType(GameManager.ResourceType feedType)
    {
        // GameManagerに選ばれた餌タイプを通知する
        GameManager.Instance.SetSelectedFeedType(feedType);
        Debug.Log("選択された餌: " + feedType);
        UpdateFeedTypeUI(); // 餌タイプが変更された際にUIを更新
    }

    // 餌の種類を保持するメソッド（ボタンから呼び出される）
    /*public void SelectFeed(int feedTypeIndex)
    {
        GameManager.ResourceType selectedFeed = (GameManager.ResourceType)feedTypeIndex;

        // 在庫がある場合のみ餌を保持
        if (gameManager.inventory[selectedFeed] > 0) // inventoryを使用
        {
            GameManager.Instance.SelectedFeedType = selectedFeed; // GameManagerのSelectedFeedTypeを更新
            UpdateFeedTypeUI();
        }
        else
        {
            Debug.Log($"{selectedFeed} の在庫がありません");
        }
    }*/

    // UI表示を更新するメソッド
    private void UpdateFeedTypeUI()
    {
        // GameManagerのSelectedFeedTypeを参照してUIを更新
        feedTypeText.text = GameManager.Instance.SelectedFeedType.HasValue
            ? GameManager.Instance.SelectedFeedType.Value.ToString() // 選択された餌の名前を表示
            : "No Feed Selected";
    }

    // 餌のボタンの状態（有効/無効）を在庫に基づいて更新
    private void UpdateButtonStates()
    {
        for (int i = 0; i < feedButtons.Length; i++)
        {
            GameManager.ResourceType feedType = (GameManager.ResourceType)i;
            feedButtons[i].interactable = gameManager.inventory[feedType] > 0; // inventoryを使用してボタンの有効/無効を設定
        }
    }

    // 魚をクリックしたときに呼び出されるメソッド
    /*public void TryFeedFish(Fish fish)
    {
        if (SelectedFeedType.HasValue && gameManager.inventory[(int)SelectedFeedType.Value] > 0)
        {
            gameManager.GiveFoodToFish(fish, SelectedFeedType.Value); // 魚に餌を与える
            UpdateButtonStates(); // 在庫が減ったのでボタンを更新
            SelectedFeedType = null; // 餌をリセット
            UpdateFeedTypeUI();
        }
    }*/
}
