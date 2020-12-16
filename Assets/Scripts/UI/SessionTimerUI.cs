using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class SessionTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timer;

        private void Start()
        {
            timer.SetText("0");
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void UpdateTimer(int timerValue)
        {
            timer.SetText(timerValue.ToString());
        }
    }
}