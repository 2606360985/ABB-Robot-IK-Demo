using UnityEngine;

public class DragObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    private void OnMouseDown()
    {
        // ����갴��ʱ����ʼ��ק
        isDragging = true;
        // �������λ��������λ�õ�ƫ����
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // �����µ�λ��
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));
            // Ӧ��ƫ����
            transform.position = newPosition + offset;
        }
    }

    private void OnMouseUp()
    {
        // ������ɿ�ʱ��ֹͣ��ק
        isDragging = false;
    }
}