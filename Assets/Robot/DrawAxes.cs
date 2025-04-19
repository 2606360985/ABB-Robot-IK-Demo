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
        // 创建并设置X轴的LineRenderer
        xAxis = CreateAxis(Color.red);
        // 创建并设置Y轴的LineRenderer
        yAxis = CreateAxis(Color.green);
        // 创建并设置Z轴的LineRenderer
        zAxis = CreateAxis(Color.blue);
    }

    void Update()
    {
        // 更新X轴的位置
        UpdateAxis(xAxis, transform.right);
        // 更新Y轴的位置
        UpdateAxis(yAxis, transform.up);
        // 更新Z轴的位置
        UpdateAxis(zAxis, transform.forward);
    }

    private LineRenderer CreateAxis(Color color)
    {
        // 创建一个新的GameObject作为坐标轴
        GameObject axisObject = new GameObject("Axis");
        // 将坐标轴的父对象设置为当前物体
        axisObject.transform.parent = transform;
        // 为坐标轴对象添加LineRenderer组件
        LineRenderer lineRenderer = axisObject.AddComponent<LineRenderer>();
        // 设置LineRenderer的材质
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // 设置LineRenderer的颜色
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        // 设置LineRenderer的宽度
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        // 设置LineRenderer的顶点数量
        lineRenderer.positionCount = 2;
        return lineRenderer;
    }

    private void UpdateAxis(LineRenderer lineRenderer, Vector3 direction)
    {
        // 设置LineRenderer的起始点为当前物体的位置
        lineRenderer.SetPosition(0, transform.position);
        // 设置LineRenderer的结束点为当前物体位置加上指定方向乘以轴的长度
        lineRenderer.SetPosition(1, transform.position + direction * axisLength);
    }
}