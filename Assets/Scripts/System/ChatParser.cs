using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace dmdspirit
{
    public class ChatParser : MonoSingleton<ChatParser>
    {
        public enum ChatCommands
        {
            None,
            Join,
            Help,
            Bot,
            Gather, // !g
            Build, // !b
            Job, // !j
            Patrol, // !p
            Move, // !m
        }

        public struct Command
        {
            public string user;
            public ChatCommands commandType;
            public TeamTag teamTag;
            public BuildingType buildingType;
            public MapPosition position;
            public MapPosition? secondPosition;
            public TileDirection direction;
            public UnitJobType jobType;
            public int botIndex;
            public ResourceType resourceType;
        }
        
        public event Action<Command> OnCommand;

        private struct Message
        {
            public string userName;
            public string messageText;
        }

        private TwitchClient client;
        private List<Command> commandList;

        private List<ChatCommands> excludedCommands = new List<ChatCommands>() {ChatCommands.None};

        private void Start()
        {
            commandList = new List<Command>();
            var credentials = new ConnectionCredentials(Credentials.bot_user_name, Credentials.bot_access_token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 100,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, "doomedspirit");
            client.OnConnected += ConnectedHandler;
            client.OnConnectionError += ConnectionErrorHandler;
            client.OnNoPermissionError += NoPermissionErrorHandler;
            client.OnChatCommandReceived += ChatCommandReceivedHandler;
            client.Connect();
        }

        private void Update()
        {
            foreach (var command in commandList)
                OnCommand?.Invoke(command);
            commandList.Clear();
        }

        private bool TryParseChatCommand(string command, out ChatCommands chatCommand)
        {
            // IMPROVE: Ugly.
            switch (command.ToLower().Trim())
            {
                case "g":
                    command = "gather";
                    break;
                case "b":
                    command = "build";
                    break;
                case "j":
                    command = "job";
                    break;
                case "p":
                    command = "patrol";
                    break;
                case "m":
                    command = "move";
                    break;
            }

            return Enum.TryParse<ChatCommands>(command, true, out chatCommand);
        }

        private void ChatCommandReceivedHandler(object sender, OnChatCommandReceivedArgs e)
        {
            var userName = e.Command.ChatMessage.DisplayName;
            if (TryParseChatCommand(e.Command.CommandText, out var command) == false)
            {
                Debug.LogError($"Could not parse chat command {userName}: {e.Command.CommandText}");
                return;
            }

            var args = e.Command.ArgumentsAsList;
            switch (command)
            {
                case ChatCommands.Join:
                    if (TryParseJoinCommand(userName, args, out var joinCommand))
                        commandList.Add(joinCommand);
                    break;
                case ChatCommands.Gather:
                    if (TryParseGatherCommand(userName, args, out var gatherCommand))
                        commandList.Add(gatherCommand);
                    break;
                case ChatCommands.Help:
                    ParseHelpCommand(userName, args, e.Command.ChatMessage.Channel);
                    break;
                case ChatCommands.Build:
                    if (TryParseBuildCommand(userName, args, out var buildCommand))
                        commandList.Add(buildCommand);
                    break;
                case ChatCommands.Job:
                    if (TryParseJobCommand(userName, args, out var jobCommand))
                        commandList.Add(jobCommand);
                    break;
                case ChatCommands.Patrol:
                    if (TryParsePatrolCommand(userName, args, out var patrolCommand))
                        commandList.Add(patrolCommand);
                    break;
                case ChatCommands.Move:
                    if (TryParseMoveCommand(userName, args, out var moveCommand))
                        commandList.Add(moveCommand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ParseHelpCommand(string userName, List<string> args, string channel)
        {
            // TODO: Improve help command (!help build)
            client.SendMessage(channel, $"List of commands: {GetCommandList()}");
        }

        // !join red, !join
        private bool TryParseJoinCommand(string userName, List<string> args, out Command command)
        {
            command = new Command() {user = userName, commandType = ChatCommands.Gather, teamTag = TeamTag.None};
            if (args.Count > 0 && Enum.TryParse<TeamTag>(args[0], true, out var teamTag))
                command.teamTag = teamTag;
            return true;
        }

        // !gather wood
        private bool TryParseGatherCommand(string userName, List<string> args, out Command command)
        {
            command = new Command() {user = userName, commandType = ChatCommands.Gather};
            return args.Count >= 1 && Enum.TryParse<ResourceType>(args[0], true, out command.resourceType);
        }

        // !build a2 tower left, !build a2 tower, !build a2
        private bool TryParseBuildCommand(string userName, List<string> args, out Command command)
        {
            command = new Command() {user = userName, commandType = ChatCommands.Build};
            if (args.Count < 1 || MapPosition.TryParse(args[0], out command.position) == false) return false;
            if (args.Count > 1 && Enum.TryParse<BuildingType>(args[1], true, out var buildingType))
                command.buildingType = buildingType;
            if (args.Count > 2 && Enum.TryParse<TileDirection>(args[2], true, out var direction))
                command.direction = direction;
            return true;
        }

        // !job warrior
        private bool TryParseJobCommand(string userName, List<string> args, out Command command)
        {
            command = new Command() {user = userName, commandType = ChatCommands.Job};
            return args.Count >= 1 && Enum.TryParse<UnitJobType>(args[0], true, out command.jobType);
        }

        // !patrol a2 b2, !patrol a2
        private bool TryParsePatrolCommand(string userName, List<string> args, out Command command)
        {
            command = new Command() {user = userName, commandType = ChatCommands.Patrol};
            // 1 or 2 args. if only one => patrol between current position and target
            if (args.Count < 1 || MapPosition.TryParse(args[0], out var firstPosition) == false) return false;
            command.position = firstPosition;
            if (args.Count >= 2 && MapPosition.TryParse(args[1], out var secondPosition))
                command.secondPosition = secondPosition;
            return true;
        }

        // !move a2
        private bool TryParseMoveCommand(string userName, List<string> args, out Command command)
        {
            command = new Command() {user = userName, commandType = ChatCommands.Move};
            return args.Count >= 1 && MapPosition.TryParse(args[0], out command.position);
        }

        private string GetCommandList() => string.Join(", ", ((from ChatCommands command in Enum.GetValues(typeof(ChatCommands)) where excludedCommands.Contains(command) == false select string.Concat("!", command.ToString().ToLower()))));

        private void NoPermissionErrorHandler(object sender, EventArgs e)
        {
            Debug.LogError($"No permission error.");
        }

        private void ConnectionErrorHandler(object sender, OnConnectionErrorArgs e)
        {
            Debug.LogError($"Connection error: {e.Error.Message}.");
        }

        private void OnDestroy() => client.Disconnect();

        private void ConnectedHandler(object sender, OnConnectedArgs e) => Debug.Log($"Game connected to {client.JoinedChannels[0].Channel}");

        private void DisconnectedHandler(object sender, OnDisconnectedArgs e) => Debug.Log($"Game disconnected.");
    }
}