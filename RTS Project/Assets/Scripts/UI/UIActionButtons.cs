
using System;
using RTS.Commands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace RTS.UI
{
    [RequireComponent(typeof(Button))]
    public class UIActionButtons : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void CommandEnableFor(BaseCommand command, UnityAction onClick)
        {
            SetIcon(command.Icon);
            button.interactable = true;
            button.onClick.AddListener(onClick);
        }
        
        public void CommandDisable()
        {
            SetIcon(null);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }
        private void SetIcon(Sprite icon)
        {
            if(icon == null)
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