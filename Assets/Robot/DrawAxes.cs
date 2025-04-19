using UnityEngine;

public class DrawAxes : MonoBehaviour

{
    public float axisLength = 1f;
    public float lineWidth = 0.05f;

    private LineRenderer xAxis;
    private LineRenderer yAxis;
    private LineRenderer zAxis;

    void Start()
    {
        // ����������X���LineRenderer
        xAxis = CreateAxis(Color.red);
        // ����������Y���LineRenderer
        yAxis = CreateAxis(Color.green);
        // ����������Z���LineRenderer
        zAxis = CreateAxis(Color.blue);
    }

    void Update()
    {
        // ����X���λ��
        UpdateAxis(xAxis, transform.right);
        // ����Y���λ��
        UpdateAxis(yAxis, transform.up);
        // ����Z���λ��
        UpdateAxis(zAxis, transform.forward);
    }

    private LineRenderer CreateAxis(Color color)
    {
        // ����һ���µ�GameObject��Ϊ������
        GameObject axisObject = new GameObject("Axis");
        // ��������ĸ���������Ϊ��ǰ����
        axisObject.transform.parent = transform;
        // Ϊ������������LineRenderer���
        LineRenderer lineRenderer = axisObject.AddComponent<LineRenderer>();
        // ����LineRenderer�Ĳ���
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // ����LineRenderer����ɫ
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        // ����LineRenderer�Ŀ��
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        // ����LineRenderer�Ķ�������
        lineRenderer.positionCount = 2;
        return lineRenderer;
    }

    private void UpdateAxis(LineRenderer lineRenderer, Vector3 direction)
    {
        // ����LineRenderer����ʼ��Ϊ��ǰ�����λ��
        lineRenderer.SetPosition(0, transform.position);
        // ����LineRenderer�Ľ�����Ϊ��ǰ����λ�ü���ָ�����������ĳ���
        lineRenderer.SetPosition(1, transform.position + direction * axisLength);
    }
}