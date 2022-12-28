using System;
using System.Threading;
using AO.Core.Ids;

namespace AO.Network.Server
{
    public sealed class Client
    {
        public readonly ClientId Id;
        public readonly Tcp Tcp;
        public readonly Udp Udp;
        public readonly ClientGameData ClientGameData = new();

        public bool Authenticated { get; set; }
        public bool RunningTask { get; set; }
        public CancellationTokenSource CurrentTaskCancelToken { get; set; }

        private System.Timers.Timer timer;
        
        private readonly ServerOptions options;
        private readonly Action<ClientId> removeClient;
        
        // TODO refactor this to only have one callback that removes the client and then calls options callback
        public Client(ClientId assignedId, ServerOptions options, System.Net.Sockets.UdpClient udpListener, Action<ClientId> removeClient)
        {
            Id = assignedId;
            this.options = options;
            this.removeClient = removeClient;

            if (options.Protocol != ServerProtocol.Udp)
                Tcp = new Tcp(this, options);

            if (options.Protocol != ServerProtocol.Tcp)
                Udp = new Udp(this, options, udpListener);
        }

        public bool IsConnectedTcp()
        {
            return Tcp.Socket is not null && Tcp.Socket.Connected;
        }
        
        /// <summary>Disconnects the client's both Tcp and Udp connections, then disconnects the player from the world if it exists and saves it to database.</summary>
        /// <param name="force">Whether or not to forcefully remove the player from the world.</param>
        public void Disconnect(bool force = false)
        {
            Tcp.Disconnect();
            Udp.Disconnect();

            CurrentTaskCancelToken?.Cancel();
            
            options.ClientDisconnectedCallback?.Invoke(this, ServerProtocol.Both);
            
            removeClient(Id);

            if (ClientGameData.AccountId == 0) 
                return;

            if (ClientGameData.Player)
            {
                ThreadManager.ExecuteOnMainThread((state) =>
                {
                    var (clientGameData, forceDisconnect) = ((ClientGameData, bool))state;
                    clientGameData.Player.DisconnectPlayerFromWorld(forceDisconnect);
                }, (ClientGameData, force));
            }

            NetworkManager.Instance.RemoveConnectedAccount(ClientGameData.AccountId);
        }

        public void StartAuthenticationTimer(Action<Client> onClientNotAuthenticatedInTime)
        {
            timer = new System.Timers.Timer
            {
                Interval = 100,
                AutoReset = false, 
            };

            timer.Elapsed += (_,_) => OnAuthenticationTimerElapsed(onClientNotAuthenticatedInTime);
        }

        private void OnAuthenticationTimerElapsed(Action<Client> onClientNotAuthenticatedInTime)
        {
#if !UNITY_EDITOR
            if (!Authenticated)
                onClientNotAuthenticatedInTime(this);
#endif
            timer.Dispose();
        }
    }
}