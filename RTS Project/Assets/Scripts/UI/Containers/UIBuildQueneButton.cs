using System;
using System.Collections.Generic;
using RTS.UI;
using RTS.Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RTS.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIBuildQueneButton : MonoBehaviour, IUIElement<UnitSO, UnityAction>
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            Disable();
        }
        public void Disable()
        {
            icon.gameObject.SetActive(false);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }

        public void EnableFor(UnitSO unitSO, UnityAction callback)
        {
            button.onClick.RemoveAllListeners();
            icon.gameObject.SetActive(true);
            icon.sprite = unitSO.UnitIcon;
            button.interactable = true;
            button.onClick.AddListener(callback);

        }


    }
}
