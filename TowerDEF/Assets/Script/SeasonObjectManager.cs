using UnityEngine;

public class SeasonObjectManager : MonoBehaviour
{
    // 対応するタグ名
    private readonly string[] seasonTags = { "Spring", "Summer", "Autumn", "Winter" };

    private void Update()
    {
        // GameManager のインスタンスが存在するか確認
        if (GameManager.Instance != null)
        {
            // 現在のシーズンを取得
            GameManager.Season currentSeason = GameManager.Instance.currentSeason;

            // 全てのタグについて処理を行う
            foreach (string tag in seasonTags)
            {
                // 現在のシーズンと一致するタグは有効化、一致しないタグは無効化
                bool shouldActivate = tag == currentSeason.ToString();
                SetObjectsActiveByTag(tag, shouldActivate);
            }
        }
    }

    /// <summary>
    /// 指定されたタグを持つオブジェクトを有効化または無効化する
    /// </summary>
    /// <param name="tag">操作対象のタグ</param>
    /// <param name="isActive">有効化する場合は true、無効化する場合は false</param>
    private void SetObjectsActiveByTag(string tag, bool isActive)
    {
        // 指定されたタグを持つすべてのオブジェクトを取得
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        // 各オブジェクトの状態を設定
        foreach (GameObject obj in objects)
        {
            obj.SetActive(isActive);
        }
    }
}
