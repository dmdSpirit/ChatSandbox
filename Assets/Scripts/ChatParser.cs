using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using UnityEngine;

namespace dmdspirit
{
    public class ChatParser : MonoSingleton<ChatParser>
    {
        private enum ChatCommands
        {
            None,
            Join,
            Wood,
            Stone,
            Help
        }

        private struct Command
        {
            public string user;
            public ChatCommands commandType;
            public TeamTag teamTag;
        }

        public event Action<string, TeamTag> OnUserJoin;

        // FIXME: Should be separate entity, parsing all commands using ingame logic.
        public event Action<string, ResourceType> OnGatherCommand;

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
            {
                switch (command.commandType)
                {
                    case ChatCommands.Join:
                        OnUserJoin?.Invoke(command.user, command.teamTag);
                        break;
                    case ChatCommands.Wood:
                    case ChatCommands.Stone:
                        var resourceType = command.commandType == ChatCommands.Stone ? ResourceType.Stone : ResourceType.Tree;
                        OnGatherCommand?.Invoke(command.user, resourceType);
                        break;
                }
            }

            commandList.Clear();
        }

        private void ChatCommandReceivedHandler(object sender, OnChatCommandReceivedArgs e)
        {
            Debug.Log($"Command: ({e.Command.CommandText}) and args: ({string.Join(",", e.Command.ArgumentsAsList)})");
            // FIXME: Should this return "error message" to chat?
            if (Enum.TryParse<ChatCommands>(e.Command.CommandText, true, out var command) == false) return;
            switch (command)
            {
                case ChatCommands.Join:
                    // FIXME: Ugly.
                    if (e.Command.ArgumentsAsList.Count > 0 && Enum.TryParse<TeamTag>(e.Command.ArgumentsAsList[0].ToString(), true, out var teamTag))
                        commandList.Add(new Command() {user = e.Command.ChatMessage.DisplayName, commandType = ChatCommands.Join, teamTag = teamTag});
                    else
                        commandList.Add(new Command() {user = e.Command.ChatMessage.DisplayName, commandType = ChatCommands.Join, teamTag = TeamTag.None});
                    return;
                case ChatCommands.Wood:
                    commandList.Add(new Command() {user = e.Command.ChatMessage.DisplayName, commandType = ChatCommands.Wood});
                    break;
                case ChatCommands.Stone:
                    commandList.Add(new Command() {user = e.Command.ChatMessage.DisplayName, commandType = ChatCommands.Stone});
                    break;
                case ChatCommands.Help:
                    client.SendMessage(e.Command.ChatMessage.Channel, $"List of commands: {GetCommandList()}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private void ConnectedHandler(object sender, OnConnectedArgs e) => Debug.Log($"Game connected to {e.AutoJoinChannel}");

        private void DisconnectedHandler(object sender, OnDisconnectedArgs e) => Debug.Log($"Game disconnected.");
    }
}