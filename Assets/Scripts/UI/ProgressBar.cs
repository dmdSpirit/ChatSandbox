using UnityEngine;
using UnityEngine.UI;

namespace dmdspirit
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Image bar;

        public void SetProgress(float progress) => bar.fillAmount = progress;
    }
}