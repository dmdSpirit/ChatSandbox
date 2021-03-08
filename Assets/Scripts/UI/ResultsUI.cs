using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class ResultsUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text winner;
        [SerializeField] private TMP_Text greenStone;
        [SerializeField] private TMP_Text greenWood;
        [SerializeField] private TMP_Text greenTotal;
        [SerializeField] private TMP_Text redStone;
        [SerializeField] private TMP_Text redWood;
        [SerializeField] private TMP_Text redTotal;

        public void Show()
        {
            // TODO: Rework.
            gameObject.SetActive(true);
            // greenStone.text = greenStoneValue.ToString();
            // greenWood.text = greenWoodValue.ToString();
            // redStone.text = redStoneValue.ToString();
            // redWood.text = redWoodValue.ToString();
            // var greenTotalValue = greenStoneValue + greenWoodValue;
            // greenTotal.text = greenTotalValue.ToString();
            // var redTotalValue = redStoneValue + redWoodValue;
            // redTotal.text = redTotalValue.ToString();
            // // HACK: Winner should be calculated in GameController or somewhere else, not here.
            // if (greenTotalValue == redTotalValue)
            //     winner.text = "Draw!";
            // else
            //     winner.text = redTotalValue > greenTotalValue ? "Red Team!" : "Greed Team!";
        }

        public void Hide() => gameObject.SetActive(false);
    }
}