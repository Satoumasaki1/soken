using UnityEngine;
using UnityEngine.EventSystems;

public class DragCamera : MonoBehaviour
{
    public float dragSpeed = 2.0f;       // �h���b�O���̃J�����̈ړ����x
    public float zoomSpeed = 5.0f;       // �Y�[�����̃J�����̑��x
    public float minZoomDistance = 150.0f; // �Y�[���̍ŏ������i�����j
    public float maxZoomDistance = 265.0f; // �Y�[���̍ő勗���i����j

    private Vector3 dragOrigin;
    private bool isDraggingFromUI = false; // �h���b�O�J�n��UI�ォ�ǂ������L�^

    void Update()
    {
        // ���N���b�N�����������Ƀh���b�O�J�n�ʒu���擾
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;

            // UI��ŃN���b�N���J�n���ꂽ�����L�^
            isDraggingFromUI = IsPointerOverUI();
            return;
        }

        // ���N���b�N�����������Ă���ԁAUI����̃h���b�O�łȂ��ꍇ�̂݃J�����ړ����s��
        if (Input.GetMouseButton(0) && !isDraggingFromUI)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

            // X���i���E�j��Z���i���s���j�̈ړ��ʂ��v�Z
            Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

            // �J�����̈ʒu��ύX�i���s���ƍ��E�̈ړ��j
            transform.Translate(-move, Space.World);

            // �h���b�O�̊J�n�ʒu���X�V
            dragOrigin = Input.mousePosition;
        }

        // �}�E�X�z�C�[���ŃY�[���C���E�A�E�g
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            // ���݂̃J�����ʒu���玟�̃Y�[���ʒu���v�Z
            Vector3 zoom = transform.forward * scroll * zoomSpeed;
            Vector3 nextPosition = transform.position + zoom;

            // �J�����̋������v�Z
            float distance = Vector3.Distance(nextPosition, Vector3.zero);

            // �Y�[�������������Ə���͈͓̔��Ɏ��܂�悤�ɐ���
            if (distance >= minZoomDistance && distance <= maxZoomDistance)
            {
                transform.Translate(zoom, Space.World);
            }
        }
    }

    /// <summary>
    /// �}�E�X��UI�v�f�̏�ɂ��邩�ǂ������`�F�b�N����
    /// </summary>
    /// <returns>UI�̏�ɂ���ꍇ��true�A�����łȂ��ꍇ��false</returns>
    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
