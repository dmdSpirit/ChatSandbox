using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dmdspirit
{
    public enum TeamTag
    {
        None,
        red,
        green
    }

    public class GameController : MonoSingleton<GameController>
    {
        [SerializeField] private int joinTime = 30;
        [SerializeField] private int sessionTime = 300;
        [SerializeField] private int maxTeamSize = 4;
        [SerializeField] private JoinUI joinUI;
        [SerializeField] private SessionTimerUI sessionTimerUI;
        [SerializeField] private ResultsUI resultsUI;

        // HACK: Hardcoded number of teams for now.
        [SerializeField] private Team greenTeam;
        [SerializeField] private Team redTeam;
        private List<string> redTeamPlayers;
        private List<string> greenTeamPlayers;
        private Dictionary<string, Unit> playerUnits;

        private Coroutine joinTimerCoroutine;
        private bool isSessionRunning = false;

        public List<BuildingType> canBeBuild = new List<BuildingType>() {BuildingType.Tower};

        public void StartGame()
        {
            redTeamPlayers = new List<string>();
            greenTeamPlayers = new List<string>();
            playerUnits = new Dictionary<string, Unit>();
            // Start gathering users who want to !join.
            ChatParser.Instance.OnUserJoin += UserJoinHandler;
            ChatParser.Instance.OnGatherCommand += GatherCommandHandler;
            ChatParser.Instance.OnBuildCommand += BuildCommandHandler;
            joinTimerCoroutine = StartCoroutine(JoinTimer());
            sessionTimerUI.Hide();
            resultsUI.Hide();
            redTeam.HideUI();
            greenTeam.HideUI();
            isSessionRunning = false;
            Map.Instance.StartGame();
        }

        private void BuildCommandHandler(string userName, BuildingType buildingType, MapPosition mapPosition)
        {
            if (playerUnits.ContainsKey(userName) == false) return;
            playerUnits[userName].Build(buildingType, mapPosition);
        }


        private void GatherCommandHandler(string userName, ResourceType resourceType)
        {
            if (playerUnits.ContainsKey(userName) == false) return;
            playerUnits[userName].GatherResource(resourceType);
        }

        private void UserJoinHandler(string userName, TeamTag teamTag)
        {
            if (isSessionRunning == false && teamTag != TeamTag.None)
            {
                if (greenTeamPlayers.Contains(userName) && teamTag == TeamTag.red)
                {
                    greenTeamPlayers.Remove(userName);
                    redTeamPlayers.Add(userName);
                    joinUI.UpdatePlayers(greenTeamPlayers, redTeamPlayers);
                    return;
                }

                if (redTeamPlayers.Contains(userName) && teamTag == TeamTag.green)
                {
                    redTeamPlayers.Remove(userName);
                    greenTeamPlayers.Add(userName);
                    joinUI.UpdatePlayers(greenTeamPlayers, redTeamPlayers);
                    return;
                }
            }

            if (greenTeamPlayers.Contains(userName) || redTeamPlayers.Contains(userName) || (greenTeamPlayers.Count == maxTeamSize && redTeamPlayers.Count == maxTeamSize))
                return;
            Debug.Log($"User joining {userName} to {teamTag.ToString()}.");
            if (teamTag == TeamTag.None)
            {
                if (greenTeamPlayers.Count > redTeamPlayers.Count)
                    teamTag = TeamTag.red;
                else if (greenTeamPlayers.Count == redTeamPlayers.Count)
                {
                    var rnd = Random.Range(0, 2);
                    teamTag = rnd == 1 ? TeamTag.green : TeamTag.red;
                }
                else
                    teamTag = TeamTag.green;
            }

            var playerTeam = teamTag == TeamTag.green ? greenTeam : redTeam;
            if (teamTag == TeamTag.green)
                greenTeamPlayers.Add(userName);
            else
                redTeamPlayers.Add(userName);
            if (isSessionRunning)
            {
                var unit = playerTeam.SwapBotForPlayer(userName);
                if (unit != null)
                    playerUnits.Add(userName, unit);
            }
            else
                joinUI.UpdatePlayers(greenTeamPlayers, redTeamPlayers);
        }

        private void StartSession()
        {
            isSessionRunning = true;
            Debug.Log($"Start game session.");
            StartCoroutine(SessionTimer());
            greenTeam.Initialize(greenTeamPlayers, maxTeamSize);
            redTeam.Initialize(redTeamPlayers, maxTeamSize);
        }

        private void ShowResults()
        {
            Debug.Log("Game Ended. Show results.");
            resultsUI.Show(greenTeam.storedResources.stone, greenTeam.storedResources.wood, redTeam.storedResources.stone, redTeam.storedResources.wood);
        }

        private IEnumerator JoinTimer()
        {
            var timer = joinTime;
            joinUI.Show(timer, maxTeamSize);
            joinUI.UpdatePlayers(greenTeamPlayers, redTeamPlayers);
            while (timer > 0)
            {
                yield return new WaitForSeconds(1);
                timer -= 1;
                joinUI.UpdateTimer(timer);
            }

            joinUI.Hide();
            StartSession();
        }

        private IEnumerator SessionTimer()
        {
            sessionTimerUI.Show();
            var timer = sessionTime;
            sessionTimerUI.UpdateTimer(timer);
            while (timer > 0)
            {
                yield return new WaitForSeconds(1);
                timer -= 1;
                sessionTimerUI.UpdateTimer(timer);
            }

            ShowResults();
        }

        public void AddPlayer(TeamTag teamTag = TeamTag.None)
        {
            UserJoinHandler(string.Concat("testUser", Random.Range(0, 100).ToString()), teamTag);
        }

        public void PlayerUnitCreated(string player, Unit unit)
        {
            playerUnits.Add(player, unit);
        }
    }
}