using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class JoinUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timer;
        [SerializeField] private TMP_Text[] greenTeam;
        [SerializeField] private TMP_Text[] redTeam;

        private List<string> greenPlayers;
        private List<string> redPlayers;

        public void Show(int timerValue, int maxPlayersNumber)
        {
            gameObject.SetActive(true);
            if (maxPlayersNumber > greenTeam.Length || maxPlayersNumber > redTeam.Length)
                Debug.LogWarning($"{name} does not have enough slots for all players.");
            UpdateTimer(timerValue);
        }

        public void UpdatePlayers(List<string> greenPlayers, List<string> redPlayers)
        {
            for (var i = 0; i < greenTeam.Length; i++)
                greenTeam[i].text = i < greenPlayers.Count ? greenPlayers[i] : string.Empty;
            for (var i = 0; i < redTeam.Length; i++)
                redTeam[i].text = i < redPlayers.Count ? redPlayers[i] : string.Empty;
        }

        public void UpdateTimer(int timerValue) => timer.SetText(timerValue.ToString());
        public void Hide() => gameObject.SetActive(false);
    }
}