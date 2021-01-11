using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dmdspirit
{
    public class CarriedResourceUI : MonoBehaviour
    {
        [SerializeField] private Sprite rockIcon;
        [SerializeField] private Sprite woodIcon;
        [SerializeField] private Image resourceIcon;
        [SerializeField] private TMP_Text quantity;

        private Unit unit;

        public void Initilaize(Unit unit)
        {
            this.unit = unit;
            unit.OnCarriedResourceChanged += CarriedResourceChangedHandler;
            Hide();
        }

        private void CarriedResourceChangedHandler()
        {
            var resource = unit.carriedResource;
            if (resource.type == ResourceType.None || resource.value == 0)
            {
                Hide();
                return;
            }
            Show();

            quantity.text = resource.value.ToString();
            resourceIcon.sprite = resource.type == ResourceType.Wood ? woodIcon : rockIcon;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}