using UnityEngine;

public class FishPanelController : MonoBehaviour
{
    // FishPanelのUI要素のRectTransformを参照する
    public RectTransform fishPanel;

    // ToggleButton（隣のボタン）のRectTransformを参照する
    public RectTransform toggleButton;

    // スライドアニメーションの所要時間（秒単位）
    public float slideDuration = 0.5f;

    // FishPanelが隠れる際の位置（画面外を想定）
    public Vector2 hiddenPosition;

    // FishPanelが表示される際の位置（画面内）
    public Vector2 visiblePosition;

    // ボタンのオフセット（パネルの右端からの距離）
    public Vector2 buttonOffset;

    // パネルが現在表示されているかを追跡するフラグ
    private bool isPanelVisible = true;

    void Start()
    {
        // 初期位置を設定
        fishPanel.anchoredPosition = visiblePosition;

        // ボタンの初期位置をFishPanelの右端に合わせて、オフセットを考慮
        toggleButton.anchoredPosition = new Vector2(
            visiblePosition.x + fishPanel.sizeDelta.x + buttonOffset.x,
            visiblePosition.y + buttonOffset.y
        );
    }

    // FishPanelとボタンをトグルする（表示/非表示を切り替える）
    public void TogglePanel()
    {
        // 他のアニメーションが進行中の場合はキャンセルして新たに開始する
        StopAllCoroutines();

        if (isPanelVisible)
        {
            // パネルを隠すアニメーションを開始
            StartCoroutine(SlidePanel(hiddenPosition));
        }
        else
        {
            // パネルを表示するアニメーションを開始
            StartCoroutine(SlidePanel(visiblePosition));
        }
        // 表示状態をトグル
        isPanelVisible = !isPanelVisible;
    }

    // FishPanelとToggleButtonをスライドさせるアニメーション処理
    private System.Collections.IEnumerator SlidePanel(Vector2 targetPosition)
    {
        // FishPanelの現在の位置を記録
        Vector2 startPanelPos = fishPanel.anchoredPosition;

        // ToggleButtonの現在の位置を記録
        Vector2 startButtonPos = toggleButton.anchoredPosition;

        // ボタンの最終的な目標位置を計算
        Vector2 targetButtonPos = new Vector2(
            targetPosition.x + fishPanel.sizeDelta.x + buttonOffset.x,
            targetPosition.y + buttonOffset.y
        );

        float elapsedTime = 0f; // アニメーション経過時間の初期化

        while (elapsedTime < slideDuration) // アニメーションが終了するまでループ
        {
            // 経過時間を更新
            elapsedTime += Time.deltaTime;

            // 現在の進行度（0〜1）を計算
            float t = Mathf.Clamp01(elapsedTime / slideDuration);

            // FishPanelをスライド
            fishPanel.anchoredPosition = Vector2.Lerp(startPanelPos, targetPosition, t);

            // ToggleButtonをスライド
            toggleButton.anchoredPosition = Vector2.Lerp(startButtonPos, targetButtonPos, t);

            yield return null; // 次のフレームまで待機
        }

        // 最終的な位置を正確に設定
        fishPanel.anchoredPosition = targetPosition;
        toggleButton.anchoredPosition = targetButtonPos;
    }
}
