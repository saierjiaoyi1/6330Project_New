using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("目标角色（摄像机默认跟随此目标）")]
    public Transform target;
    [Tooltip("跟随目标时的平滑时间")]
    public float followSmoothTime = 0.2f;

    [Header("自由平移")]
    [Tooltip("WASD移动速度")]
    public float moveSpeed = 10f;
    [Tooltip("自由移动时的平滑时间（惯性效果）")]
    public float moveSmoothTime = 0.2f;

    [Header("旋转设置")]
    [Tooltip("鼠标右键旋转灵敏度")]
    public float rotateSensitivity = 2f;

    [Header("缩放设置")]
    [Tooltip("鼠标滚轮缩放灵敏度")]
    public float zoomSensitivity = 5f;
    [Tooltip("正交摄像机最小尺寸")]
    public float minOrthoSize = 2f;
    [Tooltip("正交摄像机最大尺寸")]
    public float maxOrthoSize = 20f;

    [Header("摄像机参数")]
    [Tooltip("摄像机固定的俯仰角（X轴旋转角度），默认 45°")]
    public float pitch = 45f;

    // 内部变量
    private Vector3 pivot;             // 当前观察中心（旋转/平移的基准点）
    private Vector3 targetPivot;       // 目标中心点（用于平滑移动）
    private Vector3 pivotVelocity = Vector3.zero;  // SmoothDamp 使用的速度变量

    private float yaw;       // 水平旋转角度（围绕 Y 轴，单位：度）
    private float distance;  // 摄像机与中心点之间的距离（用于球面坐标计算）

    private bool isFollowing = true;   // 是否处于跟随目标状态

    void Start()
    {
        // 初始化中心点：如果有目标则以目标位置为中心，否则以摄像机前方一点为中心
        if (target != null)
        {
            pivot = target.position;
            targetPivot = target.position;
        }
        else
        {
            pivot = transform.position + transform.forward * 10f;
            targetPivot = pivot;
        }

        // 根据摄像机初始位置与中心点的偏移计算球面坐标参数：
        // 求出水平偏移，利用公式：horizontalDistance = distance * cos(pitch)
        Vector3 offset = transform.position - pivot;
        Vector3 horizontalOffset = new Vector3(offset.x, 0, offset.z);
        float horizontalDistance = horizontalOffset.magnitude;
        float pitchRad = pitch * Mathf.Deg2Rad;
        distance = horizontalDistance / Mathf.Cos(pitchRad);
        // yaw：由水平偏移的方向计算得出
        yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
    }

    void Update()
    {
        // 【1】按下空格键时恢复跟随模式
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (target != null)
            {
                isFollowing = true;
                targetPivot = target.position;
            }
        }

        // 【2】检测 WASD 键，进行自由平移
        float h = -Input.GetAxis("Horizontal"); // A/D 键
        float v = -Input.GetAxis("Vertical");   // W/S 键
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            // 一旦有 WASD 输入，取消跟随模式（若之前处于跟随状态）
            if (isFollowing)
            {
                isFollowing = false;
                // 保持当前中心点作为自由移动起点
                targetPivot = pivot;
            }

            // 这里采用以当前 yaw 为基准的平移方向（保证平移方向与摄像机绕中心旋转时保持一致）
            float yawRad = yaw * Mathf.Deg2Rad;
            // 定义“前方”与“右方”方向（仅在水平面上）
            Vector3 forward = new Vector3(Mathf.Sin(yawRad), 0, Mathf.Cos(yawRad));
            Vector3 right = new Vector3(Mathf.Cos(yawRad), 0, -Mathf.Sin(yawRad));
            // 计算移动量（可根据需要不归一化，使得同时按下多个键时速度增大）
            Vector3 moveDelta = (right * h + forward * v) * moveSpeed * Time.deltaTime;
            targetPivot += moveDelta;
        }

        // 如果处于跟随模式，则目标中心点跟随目标角色
        if (isFollowing && target != null)
        {
            targetPivot = target.position;
        }

        // 利用 SmoothDamp 平滑更新当前中心点（产生惯性效果）
        pivot = Vector3.SmoothDamp(pivot, targetPivot, ref pivotVelocity, moveSmoothTime);

        // 【3】按住鼠标右键拖动时进行旋转：左右拖动修改 yaw 角度
        if (Input.GetMouseButton(1))
        {
            float deltaX = Input.GetAxis("Mouse X");
            yaw += deltaX * rotateSensitivity;
        }

        // 【4】鼠标滚轮缩放视野（正交摄像机通过改变 orthographicSize 实现缩放）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSensitivity, minOrthoSize, maxOrthoSize);
            }
        }
    }

    void LateUpdate()
    {
        // 根据球面坐标公式计算摄像机相对于中心点的偏移：
        // offset.x = distance * sin(yaw) * cos(pitch)
        // offset.y = distance * sin(pitch)
        // offset.z = distance * cos(yaw) * cos(pitch)
        float pitchRad = pitch * Mathf.Deg2Rad;
        float yawRad = yaw * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            distance * Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            distance * Mathf.Sin(pitchRad),
            distance * Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        );

        // 更新摄像机位置，并始终让摄像机看向中心点
        transform.position = pivot + offset;
        transform.LookAt(pivot);
    }
}
