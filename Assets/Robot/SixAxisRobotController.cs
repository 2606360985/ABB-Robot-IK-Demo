using UnityEngine;
using System.Collections;

public class SixAxisRobotController : MonoBehaviour
{
    [System.Serializable]
    public class JointSettings
    {
        public Transform jointTransform;      // �ؽڵ�Transform
        public Vector3 rotationAxis = Vector3.up; // �ؽ���ת�ᣨ�ֲ�����ϵ�£�
        public float minAngle = -180f;          // ��С����Ƕȣ�����ڳ�ʼ��̬��
        public float maxAngle = 180f;           // �������Ƕȣ�����ڳ�ʼ��̬��

        [HideInInspector]
        public Quaternion initialLocalRotation; // �ؽڵĳ�ʼ�ֲ���ת����Start�м�¼��
        [HideInInspector]
        public float currentAngle;              // ��ǰ�ۻ�����ת�Ƕȣ���ʼΪ0��
    }

    [Header("�ؽ�����")]
    public JointSettings[] joints = new JointSettings[6]; // J1-J6

    [Header("IK����")]
    public Transform endEffector; // ĩ��ִ����������ĩ�ˣ�
    public Transform target;      // IKĿ��λ��
    public Transform hint;        // �ⲿ��ʾ����ʾ����δʹ�ã��ɸ�����Ҫ��չ��
    [Range(0, 1)] public float ikWeight = 1f;       // IKλ��Ȩ�أ������ֶΣ�
    [Range(0, 1)] public float rotationWeight = 1f;   // IK��תȨ�أ������ֶΣ�

    [Header("�˶�����")]
    public Transform pickupPoint;  // ʰȡ��
    public Transform placePoint;   // ���õ�
    public float movementSpeed = 0.5f; // Ŀ���ƶ��ٶ�

    private bool isMoving = false;
    private Vector3 currentTargetPosition;

    void Start()
    {
        // ��¼ÿ���ؽڵĳ�ʼ�ֲ���ת������ʼ����ǰ�Ƕ�Ϊ0
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
        // ���ո������ȡ�Ż��˶���Ŀ����pickupPoint��placePoint֮���ƶ���
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(MoveToTarget());
        }

        // ÿ֡�����Զ���� IK ��������������ؽ�ʹĩ��ִ�����ƽ�Ŀ��
        if (target != null && endEffector != null)
        {
            SolveIK();
        }
    }

    /// <summary>
    /// Э�̣���ʰȡ��ͷ��õ�֮��ƽ���ƶ�Ŀ��
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
    /// CCD IK ���������ĩ��ִ������ʼ�������������������ؽڵ���ת
    /// </summary>
    void SolveIK()
    {
        // �����������ɸ�����Ҫ�����������⾫��
        int iterations = 10;
        // ��ĩ����Ŀ�����С�ڴ���ֵʱ��Ϊ�Ѿ�����
        float threshold = 0.01f;

        for (int iter = 0; iter < iterations; iter++)
        {
            float distanceToTarget = Vector3.Distance(endEffector.position, target.position);
            if (distanceToTarget < threshold)
                break;

            // ��ĩ�˹ؽڿ�ʼ��������ǰ������CCD�㷨ͨ������β��ʼ������
            for (int i = joints.Length - 1; i >= 0; i--)
            {
                JointSettings joint = joints[i];
                if (joint.jointTransform == null)
                    continue;

                Vector3 jointPos = joint.jointTransform.position;
                // �������ӵ�ǰ�ؽ�ָ��ĩ��ִ����
                Vector3 toEffector = endEffector.position - jointPos;
                // �������ӵ�ǰ�ؽ�ָ��Ŀ��
                Vector3 toTarget = target.position - jointPos;

                // ���ؽڵľֲ���ת��ת��Ϊ��������ϵ�µķ���
                Vector3 worldAxis = joint.jointTransform.TransformDirection(joint.rotationAxis).normalized;

                // ������ͶӰ����ֱ����ת���ƽ����
                Vector3 toEffectorProj = Vector3.ProjectOnPlane(toEffector, worldAxis);
                Vector3 toTargetProj = Vector3.ProjectOnPlane(toTarget, worldAxis);

                // ���ͶӰ���ȹ��̣�����������ֹ��ֵ���⣩
                if (toEffectorProj.sqrMagnitude < 0.0001f || toTargetProj.sqrMagnitude < 0.0001f)
                    continue;

                // ����ͶӰ����֮��Ĵ����żнǣ���������Ϊ�ο���
                float angleDelta = Vector3.SignedAngle(toEffectorProj, toTargetProj, worldAxis);

                // ���㲢���Ƹùؽڵ����ۻ���ת�Ƕ�
                float newAngle = joint.currentAngle + angleDelta;
                newAngle = Mathf.Clamp(newAngle, joint.minAngle, joint.maxAngle);
                // ��¼����ʵ�ʸ��µĽǶ�
                float clampedDelta = newAngle - joint.currentAngle;
                joint.currentAngle = newAngle;

                // ���¹ؽھֲ���ת�����ڳ�ʼ��̬����ת��ǰ�Ƕ�
                joint.jointTransform.localRotation = joint.initialLocalRotation *
                                                       Quaternion.AngleAxis(joint.currentAngle, joint.rotationAxis);
            }
        }
    }

    /// <summary>
    /// �ڱ༭���л��Ƶ��Ը���ͼ�Σ�Ŀ��λ�ú͸��ؽڵ���ת��
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
                // ����������ϵ�»��ƹؽ���ת��ķ���
                Vector3 axisWorld = joints[i].jointTransform.TransformDirection(joints[i].rotationAxis).normalized;
                Gizmos.DrawLine(joints[i].jointTransform.position,
                                joints[i].jointTransform.position + axisWorld * 0.5f);
            }
        }
    }
}
