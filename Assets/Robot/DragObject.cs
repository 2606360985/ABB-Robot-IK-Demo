using UnityEngine;

public class DragObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    private void OnMouseDown()
    {
        // 当鼠标按下时，开始拖拽
        isDragging = true;
        // 计算鼠标位置与物体位置的偏移量
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // 计算新的位置
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));
            // 应用偏移量
            transform.position = newPosition + offset;
        }
    }

    private void OnMouseUp()
    {
        // 当鼠标松开时，停止拖拽
        isDragging = false;
    }
}