using System.Collections.Generic;
using System.Linq;
using Client = AO.Network.Server.Client;
using PacketSender = AO.Network.PacketSender;

namespace AO.Core.Console
{
	public static class ConsoleLogic
	{
		private const char PREFIX = '/';
		
		private delegate void CommandHandler(Client fromClient, string[] args);
		private static readonly Dictionary<string, CommandHandler> commandHandlers = new()
		{
			{ "salir", CommandsHandler.HandleExitCommand },
			{ "give", CommandsHandler.HandleGiveItemCommand },
			{ "kill", CommandsHandler.HandleKillCommand },
			{ "meditar", CommandsHandler.HandleMeditateCommand },
			{ "spawn", CommandsHandler.HandleSpawnNpcCommand },
			{ "speed", CommandsHandler.HandleSpeedCommand },
			{ "tp", CommandsHandler.HandleTeleportCommand },
			{ "gold", CommandsHandler.HandleGoldCommand },
			{ "invitar", CommandsHandler.HandleInvitePartyCommand },
			{ "aceptar", CommandsHandler.HandleAcceptParty },
			{ "changeloglevel", CommandsHandler.HandleChangeLogLevel },
			{ "reload", CommandsHandler.HandleReload },
			{ "kick", CommandsHandler.HandleKick },
			{ "status", CommandsHandler.HandleStatus },
			{ "getquest", CommandsHandler.HandleGetQuest },
			{ "completequest", CommandsHandler.HandleCompleteQuest },
			{ "findid", CommandsHandler.HandleFindId },
			{ "revivir", CommandsHandler.HandleReviveCommand },
			{ "kms", CommandsHandler.HandleKms },
			{ "desc", CommandsHandler.HandleDescription }
		};

		public static void ProcessCommand(Client fromClient, string inputValue)
		{
			//If it doesn't start with '/' it's a chat message
			if (string.IsNullOrEmpty(inputValue) || inputValue[0] != PREFIX)
			{
				PacketSender.ChatBroadcast(fromClient.ClientGameData.Player, inputValue);
				return;
			}

			//Remove '/' from index 0
			inputValue = inputValue.Remove(0, 1);

			//Split the input on the first white space and get the command word in lower case
			string[] inputSplit = inputValue.Split(' ');
			string commandInput = inputSplit[0].ToLower();

			if (commandHandlers.TryGetValue(commandInput, out CommandHandler handler))
			{
				//If the command exists parse the rest of the inputs to an array and invoke the handler
				string[] args = inputSplit.Skip(1).ToArray();
				handler(fromClient, args);
			}
		}
	}
}
