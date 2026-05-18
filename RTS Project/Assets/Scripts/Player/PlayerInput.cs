using RTS.Commands;
using RTS.EventSystem;
using RTS.UI;
using RTS.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngineEventSystem = UnityEngine.EventSystems;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("引用组件")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CinemachineFollow cinemachineFollow;
        [SerializeField] private CinemachineRotationComposer cinemachineRotationComposer;
        [SerializeField] private new Camera camera;

        [SerializeField] private RectTransform SelectBoxUI;
        [SerializeField] private LayerMask FloorMask;

        [SerializeField] private LayerMask CommandableMask;

        [Header("参数设置")]
        [SerializeField] private CameraConfig CameraConfig;

        [Header("计时器")]
        [SerializeField] private float mousePanTimer = 0f;

        [SerializeField] private Vector2 DragStartPos;
        [Header("状态")]
        [SerializeField] private bool inEdgeZone;
        [SerializeField] private bool isDragging;

        [SerializeField] private bool isMouseDownOnUI;


        private BaseCommand ActiveCommand = null;
        private List<BaseCommandable> CommandableSelectedList = new(20);
        private List<BaseMobileUnit> baseMobileUnits = new(20);
        private HashSet<BaseCommandable> units = new(200); // 这是场景中总共可容纳的单位


        void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("where is your cinemachineFollow?");
            }
            if (!cinemachineCamera.TryGetComponent(out cinemachineRotationComposer))
            {
                Debug.LogError("where is your cinemachineRotationComposer?");
            }

            InitCinemachine();
        }

        void OnEnable()
        {
            EventSystem.EventBus.Subscribe<UnitSpawnEvent>(InitAliveUnits);
            EventSystem.EventBus.Subscribe<CommandSelectedEvent>(HandleCommandSelected);
        }

        void OnDisable()
        {
            EventSystem.EventBus.UnSubscribe<UnitSpawnEvent>(InitAliveUnits);
            EventSystem.EventBus.UnSubscribe<CommandSelectedEvent>(HandleCommandSelected);
        }

        // Update is called once per frame
        void Update()
        {
            HandlePaning();    // 处理WASD平移
            HandleZooming();    // 处理中键缩放
            HandleOrbiting();    // 处理中键旋转
            HandleAllTimer();    // 处理所有计时器
            HandleSelectUnit();     // 处理左键（拖拽）选择
            HandleFindPath();       // 处理右键寻路
        }

        private void InitAliveUnits(UnitSpawnEvent evt)
        {
            units.Add(evt.unit);
        }

        private void HandleAllTimer()
        {
            if (inEdgeZone)
            {
                mousePanTimer += Time.deltaTime;
            }
            else
            {
                mousePanTimer = 0f;
            }
        }

        private void HandleCommandSelected(CommandSelectedEvent evt)
        {
            Debug.Log("触发UI命令选择");
            ActiveCommand = evt.Command;
            if (!ActiveCommand.RequireClickToActivate)
            {
                // 如果不需要再次点击来触发就给个空位置让他触发
                ActivateCommand(new RaycastHit());
            }
        }
        private void HandleCommandRingUI(Vector3 Pos)
        {
            // 命令环UI
            EventSystem.EventBus.Publish<MobileCommandRingSpawnEvent>(new
             MobileCommandRingSpawnEvent
            { Pos = Pos });
        }

        private void HandleFindPath()
        {
            //TODO检测非可移动单位
            if (CommandableSelectedList.Count <= 0)
                return;


            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                ActiveCommand = null; // 处理边界条件

                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, FloorMask))
                {
                    Debug.Log("已发射寻路射线检测到地面");
                    //EventSystem.EventBus.Publish<UnitMoveToEvent>(new UnitMoveToEvent { Pos = hit.point });
                    //Vector3 centerPos = GetSelectedUnitsCenter();
                    HandleCommandRingUI(hit.point);

                    for (int i = 0; i < CommandableSelectedList.Count; i++)
                    {
                        CommandContext commandContext = new CommandContext(CommandableSelectedList[i], hit, i);

                        foreach (ICommand command in CommandableSelectedList[i].availableCommands)
                        {
                            if (command.CanHandle(commandContext))
                            {
                                command.Handle(commandContext);
                                break;
                            }
                        }
                    }

                }
            }
        }
        //用来计算重心坐标
        // private Vector3 GetSelectedUnitsCenter()
        // {
        //     Vector3 PosSum = Vector3.zero;
        //     foreach (BaseUnit unit in UnitSelectedList)
        //     {
        //         PosSum += unit.transform.position;
        //     }
        //     return PosSum / UnitSelectedList.Count;
        // }

        private void HandleSelectUnit()
        {
            if (camera == null)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                isMouseDownOnUI = UnityEngineEventSystem.EventSystem.current.IsPointerOverGameObject();
                if (isMouseDownOnUI)
                    return;

                Debug.Log("鼠标左键按下，清空选择列表");
                if (ActiveCommand == null)
                {
                    ClearSelectedList();
                }
                isDragging = false;
                DragStartPos = Mouse.current.position.ReadValue();
            }

            if (isMouseDownOnUI)
                return;

            if (Mouse.current.leftButton.isPressed && !isDragging)
            {
                if (Vector2.Distance(DragStartPos, Mouse.current.position.ReadValue()) > 5f)
                {
                    isDragging = true;
                    SelectBoxUI.gameObject.SetActive(true);
                }
            }

            if (isDragging)
            {
                HandleMultSelect();
            }
            else
            {
                Debug.Log($"isDraing:{isDragging}");
                HandleSingleSelectOrCommand();
            }

        }

        private void HandleMultSelect()
        {
            Debug.Log("触发多单位选择");
            SelectBoxUI.position = DragStartPos;
            Vector2 currentPos = Mouse.current.position.ReadValue();
            Vector2 size = currentPos - DragStartPos;

            SelectBoxUI.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            SelectBoxUI.anchoredPosition = DragStartPos + size / 2;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Bounds selectBounds = new(SelectBoxUI.anchoredPosition, SelectBoxUI.sizeDelta);
                // 检测单位是否再框内
                foreach (BaseCommandable unit in units)
                {

                    Vector2 unitsPos = camera.WorldToScreenPoint(unit.transform.position);
                    if (selectBounds.Contains(unitsPos))
                    {

                        CommandableSelectedList.Add(unit);

                        if (unit is BaseMobileUnit mobileUnit)
                        {
                            baseMobileUnits.Add(mobileUnit);
                        }

                    }
                }

                if (baseMobileUnits.Count == 0) //说明框选的全是建筑
                {
                    foreach (BaseCommandable unit in CommandableSelectedList)
                    {
                        EventSystem.EventBus.Publish<UnitSelectEvent>(
                        new UnitSelectEvent { Unit = unit });
                    }
                }
                else
                {
                    foreach (BaseCommandable unit in baseMobileUnits)
                    {
                        EventSystem.EventBus.Publish<UnitSelectEvent>(
                        new UnitSelectEvent { Unit = unit });
                    }
                }

                SelectBoxUI.gameObject.SetActive(false);
                isDragging = false;
            }
        }

        private void HandleSingleSelectOrCommand() //左键单击
        {
            // 单击选中
            Debug.Log("触发单个单位选择");
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {

                Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                //Debug.Log($"射线起点: {cameraRay.origin}, 方向: {cameraRay.direction}");

                if (ActiveCommand == null
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, CommandableMask)
                && hit.collider.TryGetComponent(out RTS.Units.BaseCommandable unit))
                {
                    Debug.Log("鼠标左键释放，开始选择单位");
                    //Debug.Log($"命中物体: {hit.collider.gameObject.name}, 单位: {unit}");
                    EventSystem.EventBus.Publish<UnitSelectEvent>(
                        new UnitSelectEvent { Unit = unit });
                    CommandableSelectedList.Add(unit);
                    Debug.Log("选中单位，已添加到列表");
                }
                else if (ActiveCommand != null &&
                Physics.Raycast(cameraRay, out hit, float.MaxValue, FloorMask))
                {
                    ActivateCommand(hit);
                }
            }
        }

        private void ActivateCommand(RaycastHit hit)
        {
            Debug.Log("左键触发命令");
            List<BaseCommandable> Commandables = CommandableSelectedList.
            Where((unit) => unit is BaseCommandable).
            Cast<BaseCommandable>().ToList();

            HandleCommandRingUI(hit.point);

            for (int i = 0; i < Commandables.Count; i++)
            {
                CommandContext commandContext = new(Commandables[i], hit, i, CommandSource.UI);
                if (ActiveCommand.CanHandle(commandContext))
                {
                    ActiveCommand.Handle(commandContext);
                }

            }
            ActiveCommand = null;
        }

        private void ClearSelectedList()
        {
            Debug.Log($"清空单位选择列表，当前列表数量: {CommandableSelectedList.Count} , {baseMobileUnits.Count}");

            if (CommandableSelectedList.Count > 0)
            {
                for (int i = 0; i < CommandableSelectedList.Count; i++)
                {
                    Debug.Log($"取消选择单位: {CommandableSelectedList[i]}");
                    EventSystem.EventBus.Publish<UnitDeSelectEvent>(
                        new UnitDeSelectEvent { Unit = CommandableSelectedList[i] });
                }

                CommandableSelectedList.Clear();
                baseMobileUnits.Clear();
                Debug.Log("列表已清空");
            }

        }

        private void HandleZooming()
        {
            float scrollAmount = Mouse.current.scroll.ReadValue().y * CameraConfig.mouseScrollSpeed;
            if (Mathf.Abs(scrollAmount) > 0.01)
            {
                // 【修改点】：不再单独操作 y 和 z，而是统一改变"距离"
                CameraConfig.currentDistance -= scrollAmount * Time.deltaTime;
                CameraConfig.currentDistance = Mathf.Clamp(
                    CameraConfig.currentDistance, CameraConfig.minZoomDistance, CameraConfig.maxZoomDistance);

                ApplySphericalOffset(); // 应用计算
            }
        }

        private void InitCinemachine()
        {
            Vector3 initialOffset = cinemachineFollow.FollowOffset;
            CameraConfig.currentDistance = initialOffset.magnitude; // 圆心到圆上的点的距离
            CameraConfig.currentPitch = Mathf.Asin(initialOffset.normalized.y) * Mathf.Rad2Deg;
            CameraConfig.currentYaw = Mathf.Atan2(initialOffset.normalized.x, -initialOffset.normalized.z) * Mathf.Rad2Deg;
        }
        private void HandleOrbiting()
        {
            if (Mouse.current.middleButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();

                CameraConfig.currentYaw += delta.x * CameraConfig.orbitSensitivity;
                CameraConfig.currentPitch -= delta.y * CameraConfig.orbitSensitivity; // 鼠标往下拖，Pitch减小，摄像机下降

                CameraConfig.currentPitch = Mathf.Clamp(CameraConfig.currentPitch,
                CameraConfig.minPitch, CameraConfig.maxPitch);

                ApplySphericalOffset();
            }
        }

        private void ApplySphericalOffset()
        {
            // 根据当前的俯仰角(Pitch)和偏航角(Yaw)生成四元数
            Quaternion rotation = Quaternion.Euler(CameraConfig.currentPitch, CameraConfig.currentYaw, 0);

            // 把默认在正后方的向量进行旋转，并拉长到指定距离。这完美实现了“沿着圆下降”
            cinemachineFollow.FollowOffset = rotation * new Vector3(0, 0, -CameraConfig.currentDistance);
        }

        private void HandlePaning()
        {
            Vector2 movement = Vector2.zero;

            movement += GetKeyboardPan();
            movement += GetMousePan();
            //  算出原本要移动过去的目标位置
            Vector3 desiredPosition = cameraTarget.position + HandlePanRelaitive(movement);
            if (CameraConfig.enableMapLimit && CameraConfig.mapBounds != null)
            {

                // 它的作用是："如果目标点在Collider里面，原样返回目标点；
                // 如果目标点跑到了Collider外面，就返回Collider表面离目标点最近的那个点。"

                Vector3 clampedPosition = CameraConfig.mapBounds.ClosestPoint(desiredPosition);

                // 因为 ClosestPoint 可能会把高度贴在 Collider 的上下表面，
                // 而我们通常希望 Target 保持在地面的固定高度，所以强行把 Y 轴改回来。
                clampedPosition.y = desiredPosition.y;

                // 将限制后的安全位置赋值给 Target
                cameraTarget.position = clampedPosition;
            }
            else
            {
                // 如果没有开启限制，或者没拖入 Collider，就直接过去
                cameraTarget.position = desiredPosition;
            }
        }

        private Vector2 GetMousePan()
        {
            if (!CameraConfig.enableEdgePan || Mouse.current.middleButton.isPressed)// 鼠标中键按下旋转时停止鼠标边缘平移优化手感
            {
                inEdgeZone = false;
                return Vector2.zero;
            }
            float width = Screen.width;
            float height = Screen.height;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            //判断鼠标是否触发了任何一个边缘
            inEdgeZone = mousePos.x <= CameraConfig.mousePanRange ||
                              mousePos.x >= width - CameraConfig.mousePanRange ||
                              mousePos.y <= CameraConfig.mousePanRange ||
                              mousePos.y >= height - CameraConfig.mousePanRange;

            if (!inEdgeZone) return Vector2.zero;
            float currentMousePanSpeed = CameraConfig.minMousePanSpeed;
            // 长时间停留在区域内会加速
            currentMousePanSpeed = Mathf.SmoothStep(CameraConfig.minMousePanSpeed, CameraConfig.maxMousePanSpeed,
            mousePanTimer / CameraConfig.timeToAcceleration);

            // 将屏幕坐标映射为方向向量
            float xRatio = (mousePos.x / width) * 2f - 1f;
            float yRatio = (mousePos.y / height) * 2f - 1f;

            Debug.Log($"当前平移速度为{currentMousePanSpeed}");

            return new Vector2(xRatio, yRatio).normalized * currentMousePanSpeed;

        }

        private Vector2 GetKeyboardPan()
        {
            Vector2 movement = Vector2.zero;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                movement.y += 1f;
            }
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                movement.x -= 1f;
            }
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                movement.y -= 1f;
            }
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                movement.x += 1f;
            }
            return movement.normalized * CameraConfig.keyboardPanSpeed;
        }

        private Vector3 HandlePanRelaitive(Vector2 movement)
        {
            movement *= Time.deltaTime;
            // 1. 获取相机当前的水平偏航角 (只取 Y 轴欧拉角，忽略上下低头抬头，保证移动是水平的)
            float cameraYaw = cinemachineCamera.transform.eulerAngles.y;

            // 2. 将这个偏航角转换成四元数
            Quaternion cameraRotation = Quaternion.Euler(0, cameraYaw, 0);

            // 3. 重点：四元数 * 向量 = 将向量按照相机的朝向进行旋转！
            // 这样原本绝对的 (x, 0, y) 就变成了基于相机视角的 (Right, 0, Forward)
            Vector3 relativeMovement = cameraRotation * new Vector3(movement.x, 0, movement.y);

            // 4. 应用修改后的相对移动向量
            return relativeMovement;
        }
    }

}
