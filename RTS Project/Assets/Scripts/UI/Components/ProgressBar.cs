using Unity.Mathematics;
using UnityEngine;
namespace RTS.UI.Components
{
    public class ProgressBar : MonoBehaviour //, IUIElement<>
    {
        [SerializeField] private Vector2 Padding = new Vector2(10, 9);
        [SerializeField] private RectTransform mask;
        private RectTransform maskParentRectTransform;

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