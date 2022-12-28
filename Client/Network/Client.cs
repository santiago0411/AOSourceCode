using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using UnityEngine;

[assembly: System.Reflection.AssemblyVersion("0.6.0")]
namespace AOClient.Network
{
    public class Client : MonoBehaviour
    {
        /// <summary>Contains the current instance of this class.</summary>
        public static Client Instance { get; private set; }

        /// <summary>Contains the IP address of the server.</summary>
        public string Ip { get; set; } = "127.0.0.1";
    
        /// <summary>Contains the port the connection is bound to.</summary>
        public readonly int Port = 30500;

        /// <summary>Contains the ID assigned by the server to the client upon connection.</summary>
        public ClientId MyId { get; set; } = -1;
        /// <summary>Contains the TCP connection.</summary>
        public Tcp Tcp { get; private set; }
        /// <summary>Contains the UDP connection.</summary>
        public Udp Udp { get; private set; }

        /// <summary>Contains whether the client is connected.</summary>
        public bool IsConnected => Tcp is { IsConnected: true };

        /// <summary>Contains the last ms calculation.</summary>
        public int Ms { get; private set; }

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                StartCoroutine(Ping()); 
            }
            else if (Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }

            Application.targetFrameRate = 200;
        }

        private void Start()
        {
            Tcp = new Tcp();
            Udp = new Udp();
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        /// <summary>Attempts to connect to the server.</summary>
        /// <param name="onConnected">Action to be executed if the connection is successful.</param>
        public void ConnectToServer(Action onConnected)
        {
            Tcp.Connect(onConnected);
        }

        /// <summary>Disconnects from the server and stops all network traffic.</summary>
        public void Disconnect(string message = Constants.YOU_HAVE_BEEN_DISCONNECTED)
        {
            Tcp?.Disconnect();
            Udp?.Disconnect();
            MyId = -1;
            
            ThreadManager.ExecuteOnMainThread(() => SceneLoader.Instance.LoadLoginScene(message));
        }

        /// <summary>Coroutine that pings the server to fetch MS.</summary>
        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private static IEnumerator Ping()
        {
            for (;;)
            {
                int counter = 0;
                var ping = new Ping(Instance.Ip);
                yield return new WaitForSeconds(1f);

                while (!ping.isDone)
                {
                    counter++;
                    if (counter >= 20)
                    {
                        DebugLogger.Error("Could not fetch ms!!");
                        break;
                    }
                    yield return null;
                }

                Instance.Ms = ping.time;
                if (Instance.Ms > 9999) 
                    Instance.Ms = 9999;
            }
        }
        
        public delegate void Handler(Packet packet);
        public static readonly ReadOnlyDictionary<ServerPackets, Handler> PacketHandlers = new(new Dictionary<ServerPackets, Handler>
        {
            { ServerPackets.Welcome,						PacketHandler.Welcome 						},
            { ServerPackets.LoginReturn,					PacketHandler.LoginReturn 					},
            { ServerPackets.RegisterAccountReturn,			PacketHandler.RegisterAccountReturn 		},
            { ServerPackets.GetRacesAttributesReturn,		PacketHandler.GetRacesAttributesReturn 		},
            { ServerPackets.GetCharactersReturn,			PacketHandler.GetCharactersReturn 			},
            { ServerPackets.CreateCharacterReturn,			PacketHandler.CreateCharacterReturn 		},
            { ServerPackets.SpawnPlayer,					PacketHandler.SpawnPlayer 					},
            { ServerPackets.PlayerMaxResources,				PacketHandler.PlayerMaxResources 			},
            { ServerPackets.PlayerPrivateInfo,				PacketHandler.PlayerPrivateInfo 			},
            { ServerPackets.PlayerSkills,					PacketHandler.PlayerSkills 					},
            { ServerPackets.ChatBroadcast,					PacketHandler.ChatBroadcast 				},
            { ServerPackets.PlayerPosition,					PacketHandler.PlayerPosition 				},
            { ServerPackets.PlayerRangeChanged,				PacketHandler.PlayerRangeChanged 			},
            { ServerPackets.PlayerUpdatePosition,			PacketHandler.PlayerUpdatePosition 			},
            { ServerPackets.PlayerDisconnected,				PacketHandler.PlayerDisconnected 			},
            { ServerPackets.PlayerResources,				PacketHandler.PlayerResources 				},
            { ServerPackets.PlayerIndividualResource,		PacketHandler.PlayerIndividualResource 		},
            { ServerPackets.PlayerStats,					PacketHandler.PlayerStats 					},
            { ServerPackets.PlayerTalentsPoints,			PacketHandler.PlayerTalentPoints 			},
            { ServerPackets.PlayerLeveledUpTalents,			PacketHandler.PlayerLeveledUpTalents 		},
            { ServerPackets.PlayerGainedXp,					PacketHandler.PlayerGainedXp 				},
            { ServerPackets.PlayerAttributes,				PacketHandler.PlayerAttributes 				},
            { ServerPackets.ClickRequest,					PacketHandler.ClickRequest 					},
            { ServerPackets.PlayerGold,						PacketHandler.PlayerGold 					},
            { ServerPackets.WorldItemSpawned,				PacketHandler.WorldItemSpawned 				},
            { ServerPackets.WorldItemDestroyed,				PacketHandler.WorldItemDestroyed 			},
            { ServerPackets.PlayerInventory,				PacketHandler.PlayerInventory 				},
            { ServerPackets.PlayerUpdateInventory,			PacketHandler.PlayerUpdateInventory 		},
            { ServerPackets.PlayerSwapInventorySlots,		PacketHandler.PlayerSwapInventorySlots 		},
            { ServerPackets.PlayerEquippedItems,			PacketHandler.PlayerEquippedItems 			},
            { ServerPackets.OnPlayerItemEquippedChanged,	PacketHandler.OnPlayerItemEquippedChanged 	},
            { ServerPackets.EndEnterWorld,					PacketHandler.EndEnterWorld 				},
            { ServerPackets.ConsoleMessage,					PacketHandler.ConsoleMessage 				},
            { ServerPackets.UpdatePlayerSpells,				PacketHandler.UpdatePlayerSpells 			},
            { ServerPackets.MovePlayerSpell,				PacketHandler.MovePlayerSpell 				},
            { ServerPackets.SayMagicWords,					PacketHandler.SayMagicWords 				},
            { ServerPackets.NpcSpawn,						PacketHandler.NpcSpawn 						},
            { ServerPackets.NpcPosition,					PacketHandler.NpcPosition 					},
            { ServerPackets.NpcRangeChanged,				PacketHandler.NpcRangeChanged 				},
            { ServerPackets.NpcFacing,						PacketHandler.NpcFacing 					},
            { ServerPackets.NpcStartTrade,					PacketHandler.NpcTrade 						},
            { ServerPackets.NpcInventoryUpdate,				PacketHandler.NpcInventoryUpdate 			},
            { ServerPackets.NpcDespawned,					PacketHandler.NpcDespawned 					},
            { ServerPackets.UpdatePlayerStatus,				PacketHandler.UpdatePlayerStatus 			},
            { ServerPackets.MultiMessage,					PacketHandler.MultiMessage 					},
            { ServerPackets.PlayerInputReturn,				PacketHandler.PlayerInputReturn 			},
            { ServerPackets.CreateParticle,					PacketHandler.CreateParticle 				},
            { ServerPackets.OpenCraftingWindow,				PacketHandler.OpenCraftingWindow 			},
            { ServerPackets.DoorState,						PacketHandler.DoorState 					},
            { ServerPackets.QuestAssigned,					PacketHandler.QuestAssigned 				},
            { ServerPackets.QuestProgressUpdate,			PacketHandler.QuestProgressUpdate 			},
            { ServerPackets.QuestCompleted,					PacketHandler.QuestCompleted 				},
            { ServerPackets.NpcQuests,						PacketHandler.NpcQuests 					},
            { ServerPackets.CanSkillUpTalentReturn,			PacketHandler.CanSkillUpTalentReturn 		},
            { ServerPackets.OnYouJoinedParty,				PacketHandler.OnYouJoinedParty 				},
            { ServerPackets.OnPlayerJoinedParty,			PacketHandler.OnPlayerJoinedParty 			},
            { ServerPackets.OnPlayerLeftParty,				PacketHandler.OnPlayerLeftParty 			},
            { ServerPackets.OnCanEditPercentagesChanged,	PacketHandler.OnCanEditPercentagesChanged   },
            { ServerPackets.OnExperiencePercentageChanged,	PacketHandler.OnExperiencePercentageChanged },
            { ServerPackets.OnPartyLeaderChanged,           PacketHandler.OnPartyLeaderChanged          },
            { ServerPackets.OnPartyGainedExperience,        PacketHandler.OnPartyGainedExperience       },
            { ServerPackets.OnPartyMemberGainedExperience,  PacketHandler.OnPartyMemberGainedExperience },
            { ServerPackets.FetchMailsReturn,               PacketHandler.FetchMailsReturn              },
            { ServerPackets.RemoveMailItem,                 PacketHandler.RemoveMailItem                },
            { ServerPackets.PlayerDescriptionChanged,       PacketHandler.PlayerDescriptionChanged      },
            #if AO_DEBUG
            { ServerPackets.DebugNpcPath,					PacketHandler.DebugNpcPath 					},
            #endif
        });
    }
}
