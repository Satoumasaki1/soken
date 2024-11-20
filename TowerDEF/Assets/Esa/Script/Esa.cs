using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Esa : MonoBehaviour
{
    // ���ȂǕϊ���̃I�u�W�F�N�g
    public GameObject transformedObject;

    private void OnEnable()
    {
        // GameManager��WaveStarted�C�x���g�����b�X������
        GameManager.WaveStarted += OnWaveStart;
    }

    private void OnDisable()
    {
        // ���X�i�[�������i�s�v�ȃC�x���g�𕷂��Ȃ��悤�ɂ���j
        GameManager.WaveStarted -= OnWaveStart;
    }

    // �E�F�[�u���n�܂������ɌĂ΂��֐�
    private void OnWaveStart()
    {
        // �E�F�[�u���n�܂�����a��ʂ̃I�u�W�F�N�g�ɕϊ�
        TransformToFish();
    }

    // �a�����ɕϊ����鏈��
    private void TransformToFish()
    {
        // �ϊ���̃I�u�W�F�N�g���w�肳��Ă���ꍇ
        if (transformedObject != null)
        {

            Debug.Log("���݂̈ʒu: " + transform.position);
            Debug.Log("���݂̉�]: " + transform.rotation);

            // ���݂̈ʒu�Ɖ�]���ێ����ĕϊ���̃I�u�W�F�N�g�𐶐�
            Instantiate(transformedObject, transform.position, transform.rotation);

            // ���̉a�I�u�W�F�N�g���폜
            Destroy(gameObject);
        }
    }
}
