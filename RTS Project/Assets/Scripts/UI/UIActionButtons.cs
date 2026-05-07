
using UnityEngine;
using UnityEngine.UI;
namespace RTS.UI
{
    public class UIActionButtons : MonoBehaviour
    {
        [SerializeField] private Image icon;
        public void SetIcon(Sprite icon)
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