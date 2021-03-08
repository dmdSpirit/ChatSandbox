using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public int maxTeamSize = 4;
        
        [SerializeField] private int joinTime = 30;
        [SerializeField] private int sessionTime = 300;
        [SerializeField] private JoinUI joinUI;
        [SerializeField] private SessionTimerUI sessionTimerUI;
        [SerializeField] private ResultsUI resultsUI;

        // HACK: Hardcoded number of teams for now.
        // TODO: Rewrite using team tags and indexes.
        [SerializeField] private Team greenTeam;
        [SerializeField] private Team redTeam;
        private List<string> redTeamPlayers;
        private List<string> greenTeamPlayers;
        private Dictionary<string, Unit> playerUnits;

        private Coroutine joinTimerCoroutine;
        private bool isSessionRunning = false;

        public List<BuildingType> CanBeBuild { get; private set; }
        
        // TODO: Different logging settings.

        public void StartGame()
        {
            CanBeBuild = new List<BuildingType>() {BuildingType.Tower, BuildingType.Barracks};
            redTeamPlayers = new List<string>();
            greenTeamPlayers = new List<string>();
            playerUnits = new Dictionary<string, Unit>();
            ChatParser.Instance.OnCommand += ChatCommandHandler;
            joinTimerCoroutine = StartCoroutine(JoinTimer());
            sessionTimerUI.Hide();
            resultsUI.Hide();
            redTeam.HideUI();
            greenTeam.HideUI();
            isSessionRunning = false;
            Map.Instance.StartGame();
        }

        // IMPROVE: Not sure this is the best way.
        public Team GetTeam(TeamTag tag) => tag == TeamTag.red ? redTeam : greenTeam;

        private void ChatCommandHandler(ChatParser.Command command)
        {
            Unit unit = null;
            if (command.commandType != ChatParser.ChatCommands.Join)
            {
                if (command.isBotCommand)
                {
                    var team = command.teamTag == TeamTag.green ? greenTeam : redTeam;
                    if (team.Units.Count <= command.botIndex) return;
                    unit = team.Units[command.botIndex];
                    if (unit.IsPlayer) return;
                }
                else if (playerUnits.ContainsKey(command.user))
                    unit = playerUnits[command.user];
                else
                    return;
            }

            switch (command.commandType)
            {
                case ChatParser.ChatCommands.Join:
                    JoinUser(command.user, command.teamTag);
                    break;
                case ChatParser.ChatCommands.Gather:
                case ChatParser.ChatCommands.Build:
                case ChatParser.ChatCommands.Job:
                case ChatParser.ChatCommands.Patrol:
                case ChatParser.ChatCommands.Move:
                case ChatParser.ChatCommands.Kill:
                    unit.Command(command);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Team GetEnemyTeam(Team team) => team == redTeam ? greenTeam : redTeam;

        private void JoinUser(string userName, TeamTag teamTag)
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
            Debug.Log($"User joining {userName} to {teamTag.ToString()}.");
            if (teamTag == TeamTag.green)
                greenTeamPlayers.Add(userName);
            else
                redTeamPlayers.Add(userName);
            if (isSessionRunning)
                playerTeam.SwapBotForPlayer(userName);
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
            resultsUI.Show();
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

        public void RegisterPlayerUnit(string player, Unit unit)
        {
            playerUnits.Add(player, unit);
        }
    }
}