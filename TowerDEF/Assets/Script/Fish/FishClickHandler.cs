using UnityEngine;

public class FishClickHandler : MonoBehaviour
{
    [SerializeField] private GameManager.ResourceFishType fishType; // インスペクターで設定する魚のタイプ

    private void OnMouseDown()
    {
        AddFishToInventory(fishType);
    }

    private void AddFishToInventory(GameManager.ResourceFishType fishType)
    {
        switch (fishType)
        {
            case GameManager.ResourceFishType.Kani:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Tyoutyou:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Kaisou:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Syako:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Koban:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Teppou:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Manta:
                GameManager.Instance.finventory[fishType]++;
                break;
            case GameManager.ResourceFishType.Uni:
                GameManager.Instance.finventory[fishType]++;
                break;
            default:
                Debug.LogWarning("指定された魚タイプが見つかりません。");
                break;
        }

        // インベントリーを更新した後、UIをリフレッシュ
        GameManager.Instance.UpdateResourceUI();

        // 魚をクリック後に削除する場合
        Destroy(gameObject);
    }
}
