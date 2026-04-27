using UnityEngine;
using RTS.EventSystem;
using UnityEngine.Rendering.Universal;
using System.ComponentModel.Design;
namespace RTS.Units
{
    public class BaseBuilding : MonoBehaviour, ISelectable
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("组件")]
        [SerializeField] private DecalProjector DecalProjector;

        [Header("状态")]
        [SerializeField] private bool isSelected;

        [field:Header("数据")]
        [field: SerializeField] public float health{ get; private set; }
        void OnEnable()
        {
            EventSystem.EventBus.Subscribe<UnitSelectEvent>(OnSelected);
            EventSystem.EventBus.Subscribe<UnitDeSelectEvent>(OndeSelected);
            //EventSystem.EventBus.Subscribe<UnitMoveToEvent>(MoveTo);
        }

        void OnDisable()
        {
            EventSystem.EventBus.UnSubscribe<UnitSelectEvent>(OnSelected);
            EventSystem.EventBus.UnSubscribe<UnitDeSelectEvent>(OndeSelected);
            //EventSystem.EventBus.UnSubscribe<UnitMoveToEvent>(MoveTo);
        }
        public void OnSelected(UnitSelectEvent evt)
        {
            Debug.Log($"building.OnSelected 被调用, evt.Unit: {evt.Unit}, this: {(ISelectable)this}");
            if (evt.Unit == (ISelectable)this)
            {
                Debug.Log("选中当前building，显示DecalProjector");
                DecalProjector?.gameObject.SetActive(true);
                isSelected = true;
                //TODO
            }
        }

        public void OndeSelected(UnitDeSelectEvent evt)
        {
            Debug.Log($"building.OndeSelected 被调用, evt.Unit: {evt.Unit}, this: {(ISelectable)this}");
            if (evt.Unit == (ISelectable)this)
            {
                Debug.Log("取消选中当前building，隐藏DecalProjector");
                DecalProjector?.gameObject.SetActive(false);
                isSelected = false;
                //TODO
            }
        }
    }

}
