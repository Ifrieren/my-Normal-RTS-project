using UnityEngine;
namespace RTS.Player
{
    [System.Serializable]
    public class CameraConfig
    {
        [field: Header("相机移动参数")]
        [field: SerializeField] public float keyboardPanSpeed { get; private set; } = 20;

        [field: SerializeField] public bool enableEdgePan { get; private set; } = true;

        [field: SerializeField] public float mousePanRange { get; private set; } = 50;

        [field: SerializeField] public float minMousePanSpeed { get; private set; } = 20;

        [field: SerializeField] public float maxMousePanSpeed { get; private set; } = 60;

        [field: SerializeField] public float timeToAcceleration { get; private set; } = 2f;

        [field: Header("相机缩放参数")]
        [field: SerializeField] public float mouseScrollSpeed { get; private set; } = 100;
        [field: SerializeField] public float minZoomDistance { get; private set; } = 5;
        [field: SerializeField] public float maxZoomDistance { get; private set; } = 20;

        [field: Header("相机球面旋转参数")]
        [field: SerializeField] public float orbitSensitivity { get; private set; } = 0.2f;
        [field: SerializeField] public float minPitch { get; private set; } = 5f;   // 防止钻入地下
        [field: SerializeField] public float maxPitch { get; private set; } = 60f;  // 防止翻转

        // 记录当前的球面坐标状态
        [field: Header("当前球面状态")]
        [field: SerializeField] public float currentDistance { get; set; }
        [field: SerializeField] public float currentYaw { get; set; }
        [field: SerializeField] public float currentPitch { get; set; }

        [field: Header("地图边界限制")]
        [field: SerializeField] public bool enableMapLimit { get; private set; } = true;

        [field: Tooltip("在场景中放一个带Collider的空物体,勾选IsTrigger,拖到这里即可作为摄像机的活动范围")]
        [field: SerializeField] public Collider mapBounds { get; private set; }
    }


}