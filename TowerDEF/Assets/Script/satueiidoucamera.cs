using UnityEngine;
using UnityEngine.EventSystems;

public class satueiidoucamera : MonoBehaviour
{
    public float dragSpeed = 2.0f;       // ドラッグ時のカメラの移動速度
    public float zoomSpeed = 5.0f;       // ズーム時のカメラの速度
    public float minZoomDistance = 150.0f; // ズームの最小距離（下限）
    public float maxZoomDistance = 265.0f; // ズームの最大距離（上限）

    private Vector3 dragOrigin;
    private bool isDraggingFromUI = false; // ドラッグ開始がUI上かどうかを記録

    void Update()
    {
        // 左クリックを押した時にドラッグ開始位置を取得
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;

            // UI上でクリックが開始されたかを記録
            isDraggingFromUI = IsPointerOverUI();
            return;
        }

        // 左クリックを押し続けている間、UIからのドラッグでない場合のみカメラ移動を行う
        if (Input.GetMouseButton(0) && !isDraggingFromUI)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

            // X軸（左右）とZ軸（奥行き）の移動量を計算
            Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

            // カメラの位置を変更（奥行きと左右の移動）
            transform.Translate(-move, Space.World);

            // ドラッグの開始位置を更新
            dragOrigin = Input.mousePosition;
        }

        // マウスホイールでズームイン・アウト
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            // 現在のカメラ位置から次のズーム位置を計算
            Vector3 zoom = transform.forward * scroll * zoomSpeed;
            Vector3 nextPosition = transform.position + zoom;

            // カメラの距離を計算
            float distance = Vector3.Distance(nextPosition, Vector3.zero);

            // ズーム距離が下限と上限の範囲内に収まるように制限
            if (distance >= minZoomDistance && distance <= maxZoomDistance)
            {
                transform.Translate(zoom, Space.World);
            }
        }
    }

    /// <summary>
    /// マウスがUI要素の上にあるかどうかをチェックする
    /// </summary>
    /// <returns>UIの上にある場合はtrue、そうでない場合はfalse</returns>
    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}