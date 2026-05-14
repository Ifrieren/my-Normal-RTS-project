using UnityEngine;
using RTS.EventSystem;
using RTS.UI.Components;
using System.Collections.Generic;
using RTS.Units;
using System;
using RTS.UI.Containers;
namespace RTS.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [Header("组件")]
        [SerializeField] private BuildingsBuildingUI buildingsBuildingUI;
        [SerializeField] private CommandsUI CommandsUI;
        [SerializeField] private HashSet<BaseCommandable> SelectedCommandables = new(20);

        void Start()
        {
            CommandsUI.Disable();
            buildingsBuildingUI.Disable(); 
        }
        void OnEnable()
        {
            EventBus.Subscribe<UnitDeSelectEvent>(HandleUnitDeSelected);
            EventBus.Subscribe<UnitSelectEvent>(HandleUnitSelected);
        }
        void OnDisable()
        {
            EventBus.Subscribe<UnitDeSelectEvent>(HandleUnitDeSelected);
            EventBus.Subscribe<UnitSelectEvent>(HandleUnitSelected);
        }
        private void HandleUnitSelected(UnitSelectEvent evt)
        {
            if (evt.Unit is BaseCommandable Commandable)
            {
                SelectedCommandables.Add(Commandable);
                CommandsUI.EnableFor(SelectedCommandables);
            }
            if( evt.Unit is CommandPost commandPost && SelectedCommandables.Count == 1)
            {
                buildingsBuildingUI.EnableFor(commandPost);
            }
        }
        private void HandleUnitDeSelected(UnitDeSelectEvent evt)
        {
            if (evt.Unit is BaseCommandable Commandable)
            {
                SelectedCommandables.Remove(Commandable);
                if (SelectedCommandables.Count > 0)
                {
                    CommandsUI.EnableFor(SelectedCommandables);
                    
                    if(evt.Unit is CommandPost  commandPost&& SelectedCommandables.Count == 1)
                    {
                        buildingsBuildingUI.EnableFor(commandPost);
                    }
                    else
                    {
                        buildingsBuildingUI.Disable();
                    }
                }
                else
                {
                    CommandsUI.Disable();
                    buildingsBuildingUI.Disable();
                }
                
            }
        }
    }
}