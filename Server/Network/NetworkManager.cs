using System;
using System.Collections.Generic;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Network.Server;

namespace AO.Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        public static bool AcceptingClients;

        private Server.Server server;
        
        private readonly LoggerAdapter log = new(typeof(NetworkManager));
        private readonly HashSet<uint> connectedAccounts = new();

        private const ushort PORT = 30500;

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }
        }

        private void OnApplicationQuit()
        {
            server.Stop();
            // Call UpdateMain one last time to finish any actions that might be left
            ThreadManager.UpdateMain();
            // Stay in a while loop until all database operations are finished
            while (CharacterManager.SavingPlayersCount > 0)
                CharacterManager.Instance.SavePlayerCoroutine();
            log.Info("All players have been saved to database.");
            log.Info("Application shutdown successful.");
        }

        /// <summary>Adds an account to the list of connected accounts.</summary>
        public void AddConnectedAccount(uint accountId)
        {
            connectedAccounts.Add(accountId);
        }

        /// <summary>Returns whether the account is already connected to the server.</summary>
        public bool IsAccountLoggedIn(uint accountId)
        {
            return connectedAccounts.Contains(accountId);
        }

        /// <summary>Removes an account from the list of connected accounts.</summary>
        public void RemoveConnectedAccount(uint accountId)
        {
            connectedAccounts.Remove(accountId);
        }

        public void SendPacketTcp(ClientId toClient, Packet packet)
        {
            Tcp.LogPacketId(packet.PacketId);
            server.SendPacketTcp(toClient, packet);
        }

        public void SendPacketTcpToAll(Packet packet)
        {
            Tcp.LogPacketId(packet.PacketId);
            server.SendPacketTcpToAll(packet);
        }

        public void SendPacketTcpToAll(Packet packet, ClientId exceptClient)
        {
            Tcp.LogPacketId(packet.PacketId);
            server.SendPacketTcpToAll(packet, exceptClient);
        }

        public void SendPacketUdp(ClientId toClient, Packet packet)
        {
            server.SendPacketUdp(toClient, packet);
        }

        public void SendPacketUdpToAll(Packet packet)
        {
            server.SendPacketUdpToAll(packet);
        }

        public void SendPacketUdpToAll(Packet packet, ClientId exceptClient)
        {
            server.SendPacketUdpToAll(packet, exceptClient);
        }

        public void InitializeServer()
        {
            server = new Server.Server(GetServerOptions());

            try
            {
                server.Listen();
                AcceptingClients = true;
            }
            catch (Exception ex)
            {
                log.Error("Error starting server. {0}", ex.Message);
                GameManager.CloseApplication();
            }
        }

        private ServerOptions GetServerOptions()
        {
            return new ServerOptions
            {
                IPAddress = System.Net.IPAddress.Any,
                Port = PORT,
                MaxClients = 100,
                Protocol = ServerProtocol.Both,
                ReceiveDataBufferSize = 4096,
                SendDataBufferSize = 4096,
                ClientConnectedCallback = OnClientConnected,
                ClientConnectionEstablishedCallback = OnClientConnectionEstablished,
                ClientDisconnectedCallback = OnClientDisconnected,
                ServerIsFullCallback = OnServerIsFull,
                DataReceivedCallback = OnDataReceived
            };
        }

        private bool OnClientConnected(Client client)
        {
            log.Debug(AcceptingClients ? "Client has been accepted." : "Client has not been accepted.");
            return AcceptingClients;
        }

        private void OnClientConnectionEstablished(Client client)
        {
            log.Debug("Client {0} with id: {1} has been accepted.", client.Tcp.RemoteIPEndPoint.ToString(), client.Id);
            PacketSender.Welcome(client.Id);
            client.StartAuthenticationTimer(OnClientNotAuthenticatedInTime);
        }

        private void OnClientNotAuthenticatedInTime(Client client)
        {
            log.Warn("Client {0} with id: {1} has not send the authentication packet in time and will be disconnected.", client.Tcp.RemoteIPEndPoint.ToString(), client.Id);
            client.Disconnect();
        }

        private void OnClientDisconnected(Client client, ServerProtocol protocol)
        {
            log.Debug("Client {0} with id: {1} has disconnected.", client.Tcp.RemoteIPEndPoint.ToString(), client.Id);
        }

        private void OnServerIsFull()
        {
            log.Warn("Server is full!!");
        }

        private void OnDataReceived(Client client, Packet packet)
        {
            if (packet.UnreadLength() < 2)
            {
                log.Warn("{0} with client id: {1}, sent a packet with not enough bytes to read a packet id. Disconnecting.", client.Tcp.RemoteIPEndPoint.ToString(), client.Id);
                client.Disconnect();
                return;
            }

            var packetId = (ClientPackets)packet.ReadShort();

            if (packetHandlers.TryGetValue(packetId, out Handler handler))
            {
                Tcp.LogPacketId(packetId);
                handler(client, packet);
                packet.Dispose();
                return;
            }

            log.Warn("{0} with client id: {1}, sent a packet with an invalid id. Disconnecting.", client.Tcp.RemoteIPEndPoint.ToString(), client.Id);
            client.Disconnect();
        }
        
        private delegate void Handler(Client fromClient, Packet packet);
        private readonly Dictionary<ClientPackets, Handler> packetHandlers = new()
        {
            { ClientPackets.WelcomeReceived, 		PacketHandler.WelcomeReceived 			},
            { ClientPackets.Login, 					PacketHandler.Login 					},
            { ClientPackets.RegisterAccount, 		PacketHandler.RegisterAccount 			},
            { ClientPackets.GetRacesAttributes, 	PacketHandler.GetRacesAttributes 		},
            { ClientPackets.GetCharacters, 			PacketHandler.GetCharacters 			},
            { ClientPackets.CreateCharacter, 		PacketHandler.CreateCharacter 			},
            { ClientPackets.BeginEnterWorld, 		PacketHandler.BeginEnterWorld 			},
            { ClientPackets.PlayerChat, 			PacketHandler.PlayerChat 				},
            { ClientPackets.PlayerMovementInputs, 	PacketHandler.PlayerMovementInputs 		},
            { ClientPackets.PlayerInput, 			PacketHandler.PlayerInput 				},
            { ClientPackets.PlayerItemAction, 		PacketHandler.PlayerItemAction 			},
            { ClientPackets.PlayerDropItem, 		PacketHandler.PlayerDropItem 			},
            { ClientPackets.PlayerLeftClick, 		PacketHandler.PlayerLeftClick 			},
            { ClientPackets.PlayerSwappedItemSlot, 	PacketHandler.PlayerSwappedItemSlot		},
            { ClientPackets.NpcTrade, 				PacketHandler.NpcTrade 					},
            { ClientPackets.EndNpcTrade, 			PacketHandler.EndNpcTrade				},
            { ClientPackets.PlayerSelectedSpell, 	PacketHandler.PlayerSelectedSpell 		},
            { ClientPackets.PlayerLeftClickRequest, PacketHandler.PlayerLeftClickRequest 	},
            { ClientPackets.MovePlayerSpell,		PacketHandler.MovePlayerSpell 			},
            { ClientPackets.SkillsChanged, 			PacketHandler.SkillsChanged 			},
            { ClientPackets.DropGold, 				PacketHandler.DropGold 					},
            { ClientPackets.CraftItem, 				PacketHandler.CraftItem 				},
            { ClientPackets.CloseCraftingWindow, 	PacketHandler.CloseCraftingWindow 		},
            { ClientPackets.SelectQuest, 			PacketHandler.SelectQuest 				},
            { ClientPackets.SelectQuestItemReward, 	PacketHandler.SelectQuestItemReward 	},
            { ClientPackets.AcceptQuest, 			PacketHandler.AcceptQuest 				},
            { ClientPackets.CompleteQuest, 			PacketHandler.CompleteQuest 			},
            { ClientPackets.CanSkillUpTalent, 		PacketHandler.CanSkillUpTalent 			},
            { ClientPackets.SkillUpTalents, 		PacketHandler.SkillUpTalents 			},
            { ClientPackets.ChangePartyPercentages, PacketHandler.ChangePartyPercentages    },
            { ClientPackets.KickPartyMember,        PacketHandler.KickPartyMember           },
            { ClientPackets.SendMail,	            PacketHandler.SendMail	                },
            { ClientPackets.FetchMails,	            PacketHandler.FetchMails	            },
            { ClientPackets.CollectMailItem,        PacketHandler.CollectMailItem           },
            { ClientPackets.DeleteMail,             PacketHandler.DeleteMail                },
        };
    }
}
