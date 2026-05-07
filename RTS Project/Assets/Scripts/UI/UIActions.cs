using UnityEngine;
using RTS.EventSystem;
using System.Collections.Generic;
using RTS.Units;
using Unity.VisualScripting;
using RTS.Commands;
using Unity.AppUI.UI;
using System.Linq;
namespace RTS.UI
{
    public class UIActions : MonoBehaviour
    {
        [SerializeField] private UIActionButtons[] actionButtons;
        private HashSet<BaseCommandable> selectedUnit = new(20);

        void Start()
        {
            foreach (UIActionButtons button in actionButtons)
            {
                button.SetIcon(null);
            }
        }
        void OnEnable()
        {
            EventSystem.EventBus.Subscribe<UnitSelectEvent>(OnUnitSelected);
            EventSystem.EventBus.Subscribe<UnitDeSelectEvent>(OnUnitDeSelected);

        }
        void OnDisable()
        {
            EventSystem.EventBus.UnSubscribe<UnitSelectEvent>(OnUnitSelected);
            EventSystem.EventBus.UnSubscribe<UnitDeSelectEvent>(OnUnitDeSelected);
        }
        // Update is called once per frame
        private void OnUnitSelected(UnitSelectEvent evt)
        {
            if (evt.Unit is BaseCommandable commandable)
            {
                selectedUnit.Add(commandable);
                RefreshButtons();
            }
        }

        private void OnUnitDeSelected(UnitDeSelectEvent evt)
        {
            if (evt.Unit is BaseCommandable commandable)
            {
                selectedUnit.Remove(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<BaseCommand> availableCommands = new(9);
            foreach (BaseCommandable commandable in selectedUnit)
            {
                availableCommands.UnionWith(commandable.availableCommands);
            }

            for (int i = 0; i < actionButtons.Length; i++)
            {
                BaseCommand actionForSlot = availableCommands.Where(action => action.slot == i).FirstOrDefault();
                if (actionForSlot != null)
                {
                    actionButtons[i].SetIcon(actionForSlot.Icon);
                }
                else
                {
                    actionButtons[i].SetIcon(null);
                }
            }
        }
    }

}
