using System.Collections;
using UnityEngine;

/// <summary>
/// Level_2 专用：人类与狐狸形态切换系统（使用New Input System）
/// 与 PlayerSwitcher（镜像关卡）独立共存
/// </summary>
public class FormSwitcher : MonoBehaviour
{
    [Header("Form Instances")]
    public Player humanForm;
    public Player foxForm;
    
    [Header("Switch Settings")]
    [SerializeField] private float switchCooldown = 0.3f;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraFollow; // 可选：如果需要手动控制相机跟随
    [Tooltip("狐狸正常模式的相机大小（OrthographicSize，值越大视野越宽）")]
    [SerializeField] private float foxCameraDistance = 12f;
    [Tooltip("狐狸正常模式的相机偏移（向前看的提前量）")]
    [SerializeField] private Vector3 foxCameraOffset = new Vector3(3.5f, 1f, 0);
    [Tooltip("狐狸远景模式的相机大小（按F切换，建议是正常模式的2倍以上）")]
    [SerializeField] private float foxCameraDistanceFar = 24f;
    [Tooltip("狐狸远景模式的相机偏移")]
    [SerializeField] private Vector3 foxCameraOffsetFar = new Vector3(5f, 2f, 0);
    [Tooltip("狐狸切换远近镜头的按键")]
    [SerializeField] private KeyCode foxZoomToggleKey = KeyCode.F;
    [Tooltip("人类相机大小（OrthographicSize）")]
    [SerializeField] private float humanCameraDistance = 5f;
    [Tooltip("人类相机偏移")]
    [SerializeField] private Vector3 humanCameraOffset = Vector3.zero;
    
    [Header("Space Check")]
    [Tooltip("狐狸身上的 CeilingCheck 子对象（放在狐狸头顶位置）")]
    [SerializeField] private Transform ceilingCheck;
    [Tooltip("向上检测的距离（人类比狐狸高出的距离）")]
    [SerializeField] private float ceilingCheckDistance = 1f;
    [Tooltip("检测的层（必须包含 Ground）")]
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Audio")]
    [SerializeField] private string switchSFX = "form_switch";
    [SerializeField] private string switchFailedSFX = "switch_failed"; // 切换失败音效
    
    [Header("VFX (Optional)")]
    [SerializeField] private GameObject switchVFXPrefab;
    
    public FormSwitchSet input { get; private set; }
    
    private Player currentForm;
    private bool canSwitch = true;
    private bool isHumanForm = true;
    private bool isFoxZoomedOut = false; // 狐狸是否处于远景模式

    private void Awake()
    {
        input = new FormSwitchSet();
    }

    private void Start()
    {
        // 初始化为人类形态
        InitializeForms();
        
        // 首次初始化：直接设置状态，不使用SwitchToHuman()避免位置计算错误
        humanForm.gameObject.SetActive(true);
        humanForm.isControlled = true;
        humanForm.SwitchInputState();
        
        foxForm.gameObject.SetActive(false);
        foxForm.isControlled = false;
        
        currentForm = humanForm;
        isHumanForm = true;
        
        // 初始化相机跟随
        UpdateCameraTarget(humanForm.transform);
    }

    private void Update()
    {
        // 检测形态切换输入（Q或Left Ctrl）
        if (canSwitch && input.FormSwitch.Switch.WasPressedThisFrame())
        {
            ToggleForm();
        }
        
        // 狐狸形态：按F切换远近镜头
        if (!isHumanForm && Input.GetKeyDown(foxZoomToggleKey))
        {
            ToggleFoxZoom();
        }
    }

    private void InitializeForms()
    {
        if (humanForm == null || foxForm == null)
        {
            Debug.LogError("FormSwitcher: 请在Inspector中指定人类和狐狸的Player实例！");
            enabled = false;
            return;
        }

        // 确保两个形态共享相同的respawn位置
        Vector3 spawnPos = humanForm.transform.position;
        humanForm.respawnLoc = spawnPos;
        foxForm.respawnLoc = spawnPos;
        
        // 将 Fox 移到和 Human 相同位置（避免场景中放置位置导致的问题）
        foxForm.transform.position = humanForm.transform.position;
        
        // 确保 Fox 初始是禁用的（避免两个碰撞体冲突）
        foxForm.gameObject.SetActive(false);
        
        Debug.Log($"FormSwitcher 初始化完成: Human at {humanForm.transform.position}");
    }

    private void ToggleForm()
    {
        Debug.Log($"🔄 尝试切换形态: 当前是 {(isHumanForm ? "人类" : "狐狸")}");
        
        if (isHumanForm)
        {
            SwitchToFox();
        }
        else
        {
            Debug.Log("🦊➡️👤 从狐狸切换到人类，开始空间检测...");
            SwitchToHuman();
        }
    }
    
    /// <summary>
    /// 切换狐狸相机远近模式（仅狐狸形态可用）
    /// </summary>
    private void ToggleFoxZoom()
    {
        if (isHumanForm) return; // 人类形态不允许
        
        isFoxZoomedOut = !isFoxZoomedOut;
        
        // 更新相机
        UpdateCameraTarget(foxForm.transform);
        
        Debug.Log($"狐狸相机模式切换: {(isFoxZoomedOut ? "远景模式" : "正常模式")}");
    }

    public void SwitchToHuman()
    {
        if (isHumanForm && currentForm != null) return;
        
        // 获取当前位置
        Vector3 currentPosition = currentForm != null ? currentForm.transform.position : foxForm.transform.position;
        Vector2 currentVelocity = currentForm != null 
            ? currentForm.GetComponent<Rigidbody2D>().velocity 
            : Vector2.zero;

        // 检查切换到人类是否有足够空间（使用 CeilingCheck 检测）
        if (!CanSwitchToHuman(Vector3.zero))
        {
            Debug.LogWarning("❌ 头顶空间不足，无法变回人类形态");
            
            // 播放切换失败音效
            if (!string.IsNullOrEmpty(switchFailedSFX) && AudioManager.instance != null)
            {
                AudioManager.instance.PlayGlobalSFX(switchFailedSFX);
            }
            
            return;
        }

        // 先激活人类（确保Awake已执行，input已初始化）
        humanForm.gameObject.SetActive(true);
        humanForm.transform.position = currentPosition;
        
        // 设置控制状态
        humanForm.isControlled = true;
        humanForm.SwitchInputState();

        // 继承部分速度（让切换更流畅）
        Rigidbody2D rb = humanForm.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(currentVelocity.x * 0.7f, currentVelocity.y * 0.5f);
        }

        // 停用狐狸（放在最后，避免两个Player同时激活导致问题）
        if (foxForm != null && foxForm.gameObject.activeSelf)
        {
            foxForm.isControlled = false;
            foxForm.SwitchInputState();
            foxForm.gameObject.SetActive(false);
        }

        currentForm = humanForm;
        isHumanForm = true;
        isFoxZoomedOut = false; // 切换回人类时重置狐狸相机模式

        // 反馈
        UpdateCameraTarget(humanForm.transform);
        PlaySwitchFeedback(currentPosition);
        StartCoroutine(SwitchCooldownCo());
    }

    public void SwitchToFox()
    {
        if (!isHumanForm && currentForm != null) return;
        
        // 获取当前位置
        Vector3 currentPosition = currentForm != null ? currentForm.transform.position : humanForm.transform.position;
        Vector2 currentVelocity = currentForm != null 
            ? currentForm.GetComponent<Rigidbody2D>().velocity 
            : Vector2.zero;

        // 先激活狐狸（确保Awake已执行，input已初始化）
        foxForm.gameObject.SetActive(true);
        foxForm.transform.position = currentPosition;
        
        // 设置控制状态
        foxForm.isControlled = true;
        foxForm.SwitchInputState();

        // 继承部分速度
        Rigidbody2D rb = foxForm.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(currentVelocity.x * 0.7f, currentVelocity.y * 0.5f);
        }

        // 停用人类（放在最后）
        if (humanForm != null && humanForm.gameObject.activeSelf)
        {
            humanForm.isControlled = false;
            humanForm.SwitchInputState();
            humanForm.gameObject.SetActive(false);
        }

        currentForm = foxForm;
        isHumanForm = false;
        isFoxZoomedOut = false; // 切换到狐狸时默认使用正常视角

        // 反馈
        UpdateCameraTarget(foxForm.transform);
        PlaySwitchFeedback(currentPosition);
        StartCoroutine(SwitchCooldownCo());
    }

    private void UpdateCameraTarget(Transform target)
    {
        bool isFox = (target == foxForm.transform);
        
        // 方法1: Cinemachine (优先，最常用)
        var vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = target;
            vcam.LookAt = target;
            
            // 狐狸形态：根据远近模式调整相机
            if (isFox)
            {
                // 根据当前模式选择参数
                float distance = isFoxZoomedOut ? foxCameraDistanceFar : foxCameraDistance;
                Vector3 offset = isFoxZoomedOut ? foxCameraOffsetFar : foxCameraOffset;
                
                // 调整正交相机的视野大小（2D游戏最关键的参数）
                vcam.m_Lens.OrthographicSize = distance;
                Debug.Log($"🎥 狐狸相机 Orthographic Size 设置为: {distance}, 模式={(isFoxZoomedOut ? "远景" : "正常")}");
                
                var transposer = vcam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>();
                if (transposer != null)
                {
                    transposer.m_CameraDistance = distance;
                    transposer.m_TrackedObjectOffset = offset;
                    Debug.Log($"狐狸相机（3D）：距离={distance}, 偏移={offset}");
                }
                else
                {
                    // 如果用的是2D Transposer（最常见）
                    var transposer2D = vcam.GetCinemachineComponent<Cinemachine.CinemachineTransposer>();
                    if (transposer2D != null)
                    {
                        transposer2D.m_FollowOffset = new Vector3(offset.x, offset.y, -10f);
                        Debug.Log($"狐狸相机（2D）：偏移={offset}");
                    }
                }
            }
            else
            {
                // 人类形态：使用配置的设置
                vcam.m_Lens.OrthographicSize = humanCameraDistance;
                Debug.Log($"🎥 人类相机 Orthographic Size 设置为: {humanCameraDistance}");
                
                var transposer = vcam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>();
                if (transposer != null)
                {
                    transposer.m_CameraDistance = humanCameraDistance;
                    transposer.m_TrackedObjectOffset = humanCameraOffset;
                    Debug.Log($"人类相机（3D）：距离={humanCameraDistance}, 偏移={humanCameraOffset}");
                }
                else
                {
                    var transposer2D = vcam.GetCinemachineComponent<Cinemachine.CinemachineTransposer>();
                    if (transposer2D != null)
                    {
                        transposer2D.m_FollowOffset = new Vector3(humanCameraOffset.x, humanCameraOffset.y, -10f);
                        Debug.Log($"人类相机（2D）：偏移={humanCameraOffset}");
                    }
                }
            }
            
            Debug.Log($"Cinemachine 相机已更新跟随目标: {target.name}");
            return;
        }
        
        // 方法2: 如果Inspector中指定了cameraFollow（手动跟随）
        if (cameraFollow != null)
        {
            Vector3 offset = isFox ? 
                (isFoxZoomedOut ? foxCameraOffsetFar : foxCameraOffset) : 
                humanCameraOffset;
            cameraFollow.position = new Vector3(
                target.position.x + offset.x, 
                target.position.y + offset.y, 
                cameraFollow.position.z
            );
            Debug.Log($"Manual Camera Follow 已更新: {target.name}, 偏移={offset}");
            return;
        }
        
        // 方法3: 查找场景中的主相机并直接跟随（简单但不推荐）
        if (Camera.main != null)
        {
            Vector3 offset = isFox ? 
                (isFoxZoomedOut ? foxCameraOffsetFar : foxCameraOffset) : 
                humanCameraOffset;
            float distance = isFox ? 
                (isFoxZoomedOut ? foxCameraDistanceFar : foxCameraDistance) : 
                humanCameraDistance;
            
            Camera.main.transform.position = new Vector3(
                target.position.x + offset.x, 
                target.position.y + offset.y, 
                Camera.main.transform.position.z
            );
            
            // 调整相机大小（正交相机）
            if (Camera.main.orthographic)
            {
                Camera.main.orthographicSize = distance;
            }
            
            Debug.Log($"Main Camera 已更新: {target.name}, 大小={distance}, 模式={(isFox && isFoxZoomedOut ? "远景" : "正常")}");
            return;
        }
        
        Debug.LogWarning("未找到相机跟随系统！请检查场景中是否有 Cinemachine Virtual Camera 或在 Inspector 中设置 Camera Follow。");
    }

    private void PlaySwitchFeedback(Vector3 position)
    {
        // 播放音效
        if (!string.IsNullOrEmpty(switchSFX) && AudioManager.instance != null)
        {
            AudioManager.instance.PlayGlobalSFX(switchSFX);
        }

        // 生成特效
        if (switchVFXPrefab != null)
        {
            GameObject vfx = Instantiate(switchVFXPrefab, position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    private IEnumerator SwitchCooldownCo()
    {
        canSwitch = false;
        yield return new WaitForSeconds(switchCooldown);
        canSwitch = true;
    }

    private void OnEnable()
    {
        input.Enable();
        Player.onPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        input.Disable();
        Player.onPlayerDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        // 死亡后重置为人类形态
        //StartCoroutine(ResetToHumanAfterDeath());
    }

    private IEnumerator ResetToHumanAfterDeath()
    {
        yield return new WaitForSeconds(0.5f);
        if (!isHumanForm)
        {
            SwitchToHuman();
        }
    }

    /// <summary>
    /// 公共API：检查当前是否为人类形态
    /// </summary>
    public bool IsHumanForm() => isHumanForm;
    
    /// <summary>
    /// 公共API：检查当前是否为狐狸形态
    /// </summary>
    public bool IsFoxForm() => !isHumanForm;
    
    /// <summary>
    /// 公共API：获取当前激活的形态
    /// </summary>
    public Player GetCurrentForm() => currentForm;
    
    /// <summary>
    /// 公共API：锁定切换（用于教程或特定场景）
    /// </summary>
    public void LockSwitch() => canSwitch = false;
    
    /// <summary>
    /// 公共API：解锁切换
    /// </summary>
    public void UnlockSwitch() => canSwitch = true;

    // ==================== 空间检测方法 ====================
    
    /// <summary>
    /// 简单检测：使用 CeilingCheck 向上射线检测是否有天花板
    /// </summary>
    private bool CanSwitchToHuman(Vector3 footPosition)
    {
        // 如果没有配置 CeilingCheck，默认允许切换
        if (ceilingCheck == null)
        {
            Debug.LogWarning("⚠️ CeilingCheck 未配置，默认允许切换！请在 Fox 对象下添加 CeilingCheck 子对象。");
            return true;
        }
        
        // 使用配置的 Layer（如果没设置就用 Ground）
        LayerMask layerToCheck = groundLayer.value != 0 
            ? groundLayer 
            : LayerMask.GetMask("Ground");
        
        // 从 CeilingCheck 位置向上发射射线
        RaycastHit2D hit = Physics2D.Raycast(
            ceilingCheck.position, 
            Vector2.up, 
            ceilingCheckDistance, 
            layerToCheck
        );
        
        if (hit.collider != null)
        {
            Debug.LogWarning($"❌ 无法切换：头顶有障碍物 {hit.collider.gameObject.name}，距离={hit.distance:F2}");
            return false;
        }
        
        Debug.Log("✅ 头顶空间足够，可以切换");
        return true;
    }

#if UNITY_EDITOR
    // Scene视图中可视化调试（简化版：显示 CeilingCheck 射线）
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || currentForm == null) return;
        
        // 如果是狐狸形态，显示 CeilingCheck 检测射线
        if (!isHumanForm && ceilingCheck != null)
        {
            bool canSwitch = CanSwitchToHuman(Vector3.zero); // 仅用于获取检测结果
            
            // 绘制 CeilingCheck 位置
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ceilingCheck.position, 0.15f);
            
            // 绘制向上检测射线
            Gizmos.color = canSwitch ? Color.green : Color.red;
            Vector3 endPoint = ceilingCheck.position + Vector3.up * ceilingCheckDistance;
            Gizmos.DrawLine(ceilingCheck.position, endPoint);
            Gizmos.DrawWireSphere(endPoint, 0.1f);
        }
        
        // 绘制相机偏移（狐狸形态）
        if (!isHumanForm && foxForm != null)
        {
            // 根据当前模式选择偏移
            Vector3 offset = isFoxZoomedOut ? foxCameraOffsetFar : foxCameraOffset;
            Gizmos.color = isFoxZoomedOut ? Color.magenta : Color.cyan;
            Vector3 cameraTargetPos = foxForm.transform.position + offset;
            Gizmos.DrawLine(foxForm.transform.position, cameraTargetPos);
            Gizmos.DrawWireSphere(cameraTargetPos, isFoxZoomedOut ? 0.5f : 0.3f);
        }
    }
    
    // 编辑器调试显示
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, 100, 300, 30), $"当前形态: {(isHumanForm ? "人类 👤" : "狐狸 🦊")}");
            GUI.Label(new Rect(10, 130, 300, 30), $"切换冷却: {(canSwitch ? "就绪" : "冷却中")}");
            GUI.Label(new Rect(10, 160, 300, 30), $"按 Q 或 Left Ctrl 切换形态");
            
            // 狐狸形态时显示额外信息
            if (!isHumanForm && currentForm != null)
            {
                // 显示能否切换回人类
                bool canSwitchBack = CanSwitchToHuman(Vector3.zero); // 简单调用
                GUI.color = canSwitchBack ? Color.green : Color.red;
                GUI.Label(new Rect(10, 190, 300, 30), $"可切换回人类: {(canSwitchBack ? "是 ✓" : "否 ✗")}");
                if (ceilingCheck == null)
                {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(10, 220, 400, 30), $"警告: CeilingCheck 未配置！");
                }
                
                // 显示相机模式
                GUI.color = isFoxZoomedOut ? Color.magenta : Color.cyan;
                GUI.Label(new Rect(10, 220, 300, 30), $"相机模式: {(isFoxZoomedOut ? "远景 📷" : "正常 📹")}");
                GUI.Label(new Rect(10, 250, 300, 30), $"按 F 切换远近镜头");
            }
        }
    }
#endif
}
