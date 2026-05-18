
using System;
using RTS.Commands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace RTS.UI.Containers
{
    [RequireComponent(typeof(Button))]
    public class UIActionButtons : MonoBehaviour, IUIElement<BaseCommand, UnityAction>
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            Disable();
        }

        public void EnableFor(BaseCommand command, UnityAction onClick)
        {
            SetIcon(command.Icon);
            button.interactable = true;
            button.onClick.AddListener(onClick);
        }

        public void Disable()
        {
            SetIcon(null);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }
        private void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                this.icon.enabled = false;
            }
            else
            {
                this.icon.sprite = icon;
                this.icon.enabled = true;
            }

        }

    }
}