using Unity.Mathematics;
using UnityEngine;
namespace RTS.UI.Components
{
    public class ProgressBar : MonoBehaviour //, IUIElement<>
    {
        [SerializeField] private Vector2 Padding = new Vector2(10, 0);
        [SerializeField] private RectTransform mask;
        private RectTransform maskParentRectTransform;
#if UNITY_EDITOR
        [Header("测试字段")]
        [Range(0, 1)][SerializeField] private float progress;
#endif

        private void Update()
        {
#if UNITY_EDITOR
            SetProgress(progress);
#endif
        }

        private void Awake()
        {
            maskParentRectTransform = mask.parent.GetComponent<RectTransform>();
        }

        public void SetProgress(float progress)
        {
            Vector2 ParentSize = maskParentRectTransform.sizeDelta;
            Vector2 TargetSize = ParentSize - Padding * 2;

            TargetSize.x *= Mathf.Clamp01(progress);

            mask.offsetMin = Padding;
            mask.offsetMax = new Vector2(TargetSize.x - ParentSize.x + Padding.x, -Padding.y);

        }

    }

}