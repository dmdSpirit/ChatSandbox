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
            public bool isBotCommand;
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
            if (TryParseChatCommand(e.Command.CommandText, out var commandType) == false)
            {
                Debug.LogError($"Could not parse chat command {userName}: {e.Command.CommandText}");
                return;
            }

            var command = new Command() {user = userName};
            var args = e.Command.ArgumentsAsList;
            switch (commandType)
            {
                case ChatCommands.Help:
                    ParseHelpCommand(args, e.Command.ChatMessage.Channel);
                    return;
                case ChatCommands.Bot:
                    if (TryParseBotCommand(args, ref command) == false)
                        return;
                    if (TryParseChatCommand(args[2], out commandType) == false)
                    {
                        Debug.LogError($"Could not parse chat command {userName}: {e.Command.CommandText}");
                        return;
                    }

                    args.RemoveRange(0, 3);
                    command.isBotCommand = true;
                    break;
            }

            if (TryParseCommand(commandType, args, ref command))
                commandList.Add(command);
        }

        private bool TryParseBotCommand(List<string> args, ref Command command) => args.Count > 3 && Enum.TryParse(args[0], true, out command.teamTag) && int.TryParse(args[1], out command.botIndex);

        private bool TryParseCommand(ChatCommands commandType, List<string> args, ref Command command)
        {
            command.commandType = commandType;
            switch (commandType)
            {
                case ChatCommands.Join:
                    return TryParseJoinCommand(args, ref command);
                case ChatCommands.Gather:
                    return TryParseGatherCommand(args, ref command);
                case ChatCommands.Build:
                    return TryParseBuildCommand(args, ref command);
                case ChatCommands.Job:
                    return TryParseJobCommand(args, ref command);
                case ChatCommands.Patrol:
                    return TryParsePatrolCommand(args, ref command);
                case ChatCommands.Move:
                    return TryParseMoveCommand(args, ref command);
            }

            return false;
        }

        private void ParseHelpCommand(List<string> args, string channel)
        {
            // TODO: Improve help command (!help build)
            client.SendMessage(channel, $"List of commands: {GetCommandList()}");
        }

        // !join red, !join
        private bool TryParseJoinCommand(List<string> args, ref Command command)
        {
            if (args.Count > 0 && Enum.TryParse<TeamTag>(args[0], true, out var teamTag))
                command.teamTag = teamTag;
            return true;
        }

        // !gather wood
        private bool TryParseGatherCommand(List<string> args, ref Command command) => args.Count >= 1 && Enum.TryParse<ResourceType>(args[0], true, out command.resourceType);

        // !build a2 tower left, !build a2 tower, !build a2
        private bool TryParseBuildCommand(List<string> args, ref Command command)
        {
            if (args.Count < 1 || MapPosition.TryParse(args[0], out command.position) == false) return false;
            if (args.Count > 1 && Enum.TryParse<BuildingType>(args[1], true, out var buildingType))
                command.buildingType = buildingType;
            if (args.Count > 2 && Enum.TryParse<TileDirection>(args[2], true, out var direction))
                command.direction = direction;
            return true;
        }

        // !job warrior
        private bool TryParseJobCommand(List<string> args, ref Command command) => args.Count >= 1 && Enum.TryParse<UnitJobType>(args[0], true, out command.jobType);

        // !patrol a2 b2, !patrol a2
        private bool TryParsePatrolCommand(List<string> args, ref Command command)
        {
            // 1 or 2 args. if only one => patrol between current position and target
            if (args.Count < 1 || MapPosition.TryParse(args[0], out var firstPosition) == false) return false;
            command.position = firstPosition;
            if (args.Count >= 2 && MapPosition.TryParse(args[1], out var secondPosition))
                command.secondPosition = secondPosition;
            return true;
        }

        // !move a2
        private bool TryParseMoveCommand(List<string> args, ref Command command) => args.Count >= 1 && MapPosition.TryParse(args[0], out command.position);

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