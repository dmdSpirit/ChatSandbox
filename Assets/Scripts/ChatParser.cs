using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using UnityEngine;

namespace dmdspirit
{
    public class ChatParser : MonoBehaviour
    {
        private struct Message
        {
            public string userName;
            public string messageText;
        }

        private TwitchClient client;

        private void Start()
        {
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

        private void ChatCommandReceivedHandler(object sender, OnChatCommandReceivedArgs e)
        {
            Debug.Log($"Command: ({e.Command.CommandText}) and args: ({string.Join(",", e.Command.ArgumentsAsList)})");
        }

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