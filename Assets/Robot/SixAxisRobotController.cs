using UnityEngine;
using System.Collections;

public class SixAxisRobotController : MonoBehaviour
{
    [System.Serializable]
    public class JointSettings
    {
        public Transform jointTransform;      // 关节的Transform
        public Vector3 rotationAxis = Vector3.up; // 关节旋转轴（局部坐标系下）
        public float minAngle = -180f;          // 最小允许角度（相对于初始姿态）
        public float maxAngle = 180f;           // 最大允许角度（相对于初始姿态）

        [HideInInspector]
        public Quaternion initialLocalRotation; // 关节的初始局部旋转（在Start中记录）
        [HideInInspector]
        public float currentAngle;              // 当前累积的旋转角度（初始为0）
    }

    [Header("关节配置")]
    public JointSettings[] joints = new JointSettings[6]; // J1-J6

    [Header("IK设置")]
    public Transform endEffector; // 末端执行器（工具末端）
    public Transform target;      // IK目标位置
    public Transform hint;        // 肘部提示（本示例中未使用，可根据需要扩展）
    [Range(0, 1)] public float ikWeight = 1f;       // IK位置权重（保留字段）
    [Range(0, 1)] public float rotationWeight = 1f;   // IK旋转权重（保留字段）

    [Header("运动参数")]
    public Transform pickupPoint;  // 拾取点
    public Transform placePoint;   // 放置点
    public float movementSpeed = 0.5f; // 目标移动速度

    private bool isMoving = false;
    private Vector3 currentTargetPosition;

    void Start()
    {
        // 记录每个关节的初始局部旋转，并初始化当前角度为0
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].jointTransform != null)
            {
                joints[i].initialLocalRotation = joints[i].jointTransform.localRotation;
                joints[i].currentAngle = 0f;
            }
        }
        currentTargetPosition = pickupPoint.position;
        if (target != null)
            target.position = currentTargetPosition;
    }

    void Update()
    {
        // 按空格键触发取放货运动（目标在pickupPoint和placePoint之间移动）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(MoveToTarget());
        }

        // 每帧调用自定义的 IK 求解器，调整各关节使末端执行器逼近目标
        if (target != null && endEffector != null)
        {
            SolveIK();
        }
    }

    /// <summary>
    /// 协程：在拾取点和放置点之间平滑移动目标
    /// </summary>
    System.Collections.IEnumerator MoveToTarget()
    {
        if (isMoving || pickupPoint == null || placePoint == null) yield break;
        isMoving = true;

        Vector3 startPos = currentTargetPosition;
        Vector3 endPos = (currentTargetPosition == pickupPoint.position) ? placePoint.position : pickupPoint.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * movementSpeed;
            if (target != null)
                target.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        currentTargetPosition = endPos;
        isMoving = false;
    }

    /// <summary>
    /// CCD IK 求解器：从末端执行器开始向基座方向迭代调整各关节的旋转
    /// </summary>
    void SolveIK()
    {
        // 迭代次数，可根据需要调整以提高求解精度
        int iterations = 10;
        // 当末端与目标距离小于此阈值时认为已经到达
        float threshold = 0.01f;

        for (int iter = 0; iter < iterations; iter++)
        {
            float distanceToTarget = Vector3.Distance(endEffector.position, target.position);
            if (distanceToTarget < threshold)
                break;

            // 从末端关节开始，依次向前调整（CCD算法通常从链尾开始调整）
            for (int i = joints.Length - 1; i >= 0; i--)
            {
                JointSettings joint = joints[i];
                if (joint.jointTransform == null)
                    continue;

                Vector3 jointPos = joint.jointTransform.position;
                // 向量：从当前关节指向末端执行器
                Vector3 toEffector = endEffector.position - jointPos;
                // 向量：从当前关节指向目标
                Vector3 toTarget = target.position - jointPos;

                // 将关节的局部旋转轴转换为世界坐标系下的方向
                Vector3 worldAxis = joint.jointTransform.TransformDirection(joint.rotationAxis).normalized;

                // 将向量投影到垂直于旋转轴的平面上
                Vector3 toEffectorProj = Vector3.ProjectOnPlane(toEffector, worldAxis);
                Vector3 toTargetProj = Vector3.ProjectOnPlane(toTarget, worldAxis);

                // 如果投影长度过短，则跳过（防止数值问题）
                if (toEffectorProj.sqrMagnitude < 0.0001f || toTargetProj.sqrMagnitude < 0.0001f)
                    continue;

                // 计算投影向量之间的带符号夹角（以世界轴为参考）
                float angleDelta = Vector3.SignedAngle(toEffectorProj, toTargetProj, worldAxis);

                // 计算并限制该关节的新累积旋转角度
                float newAngle = joint.currentAngle + angleDelta;
                newAngle = Mathf.Clamp(newAngle, joint.minAngle, joint.maxAngle);
                // 记录本次实际更新的角度
                float clampedDelta = newAngle - joint.currentAngle;
                joint.currentAngle = newAngle;

                // 更新关节局部旋转：基于初始姿态再旋转当前角度
                joint.jointTransform.localRotation = joint.initialLocalRotation *
                                                       Quaternion.AngleAxis(joint.currentAngle, joint.rotationAxis);
            }
        }
    }

    /// <summary>
    /// 在编辑器中绘制调试辅助图形：目标位置和各关节的旋转轴
    /// </summary>
    void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(target.position, 0.05f);
        }

        if (joints != null)
        {
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i].jointTransform == null)
                    continue;
                Gizmos.color = Color.red;
                // 在世界坐标系下绘制关节旋转轴的方向
                Vector3 axisWorld = joints[i].jointTransform.TransformDirection(joints[i].rotationAxis).normalized;
                Gizmos.DrawLine(joints[i].jointTransform.position,
                                joints[i].jointTransform.position + axisWorld * 0.5f);
            }
        }
    }
}
