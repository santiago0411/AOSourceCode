using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Core.Utils;
using AO.Network;
using AO.Network.Server;
using AO.Players;
using AO.Players.Utils;

namespace AO.Core
{
    public static class TasksManager
    {
        private static readonly LoggerAdapter log = new(typeof(TasksManager));

        private sealed class LoginTaskSpec
        {
            public Client Client;
            public string AccountName;
            public string Password;
            public DbConnection Connection;
            public CancellationTokenSource CancellationTokenSource;
        }
        
        public static async Task LoginTask(Client fromClient, string accountName, string password)
        {
            var connection = DatabaseManager.GetConnection();
            var tokenSource = new CancellationTokenSource();
            var loginTaskSpec = new LoginTaskSpec
            {
                Client = fromClient,
                AccountName = accountName,
                Password = password,
                Connection = connection,
                CancellationTokenSource = tokenSource
            };

            fromClient.RunningTask = true;
            fromClient.CurrentTaskCancelToken = tokenSource;

            await Task.Factory.StartNew(GetAccountIdLoginTask, loginTaskSpec, tokenSource.Token).Unwrap()
                .ContinueWith(TryLoginTask, loginTaskSpec, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current).Unwrap()
                .ContinueWith((task, state) =>
                {
                    var cancelToken = (CancellationTokenSource)state;
                    OnTaskError(task, cancelToken.Token, nameof(LoginTask));
                }, tokenSource, TaskContinuationOptions.NotOnRanToCompletion)
                
                .ContinueWith((_, state) =>
                {
                    var spec = (LoginTaskSpec)state;
                    spec.Client.RunningTask = false;
                    spec.CancellationTokenSource.Dispose();
                    spec.Client.CurrentTaskCancelToken = null;
                    spec.Connection.Close();
                }, loginTaskSpec);
        }

        private static async Task<uint> GetAccountIdLoginTask(object state)
        {
            var spec = (LoginTaskSpec)state;
            await spec.Connection.OpenAsync(spec.CancellationTokenSource.Token);
            var (status, accountId) = await DatabaseOperations.FetchAccountId(spec.AccountName, spec.Connection, spec.CancellationTokenSource.Token);

            if (status != DatabaseResultStatus.Ok)
            {
                PacketSender.LoginReturn(spec.Client.Id, LoginRegisterMessage.Error);
                spec.CancellationTokenSource.Cancel();
                return 0U;
            }

            if (accountId == 0)
            {
                PacketSender.LoginReturn(spec.Client.Id, LoginRegisterMessage.InvalidAccountOrPassword);
                spec.CancellationTokenSource.Cancel();
                return 0U;
            }

            if (NetworkManager.Instance.IsAccountLoggedIn(accountId))
            {
                PacketSender.LoginReturn(spec.Client.Id, LoginRegisterMessage.AccountAlreadyLoggedIn);
                spec.CancellationTokenSource.Cancel();
                return 0U;
            }

            return accountId;
        }

        private static async Task TryLoginTask(Task<uint> previousTask, object state)
        {
            var spec = (LoginTaskSpec)state;
            
            LoginRegisterMessage loginMessage = await DatabaseOperations.VerifyLoginInfo(spec.AccountName.ToLower(), spec.Password, spec.Connection, spec.CancellationTokenSource.Token);

            if (loginMessage == LoginRegisterMessage.LoginOk)
            {
                spec.Client.ClientGameData.AccountId = previousTask.Result;
                NetworkManager.Instance.AddConnectedAccount(previousTask.Result);
            }

            PacketSender.LoginReturn(spec.Client.Id, loginMessage);
        }

        private sealed class RegisterAccountTaskSpec
        {
            public Client Client;
            public string AccountName;
            public string Password;
            public string Email;
            public DbConnection Connection;
            public CancellationTokenSource CancellationTokenSource;
        }

        public static async Task RegisterAccountTask(Client fromClient, string accountName, string password, string email)
        {
            var connection = DatabaseManager.GetConnection();
            var tokenSource = new CancellationTokenSource();
            var taskSpec = new RegisterAccountTaskSpec
            {
                Client = fromClient,
                AccountName = accountName,
                Password = password,
                Email = email,
                Connection = connection,
                CancellationTokenSource = tokenSource
            };

            fromClient.RunningTask = true;
            fromClient.CurrentTaskCancelToken = tokenSource;

            await Task.Factory.StartNew(GetAccountIdTaskRegister, taskSpec, tokenSource.Token).Unwrap()
                .ContinueWith(CheckEmailTask, taskSpec, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current).Unwrap()
                .ContinueWith(RegisterAccountTask, taskSpec, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current).Unwrap()
                .ContinueWith((task, state) =>
                {
                    var cancelToken = (CancellationTokenSource)state;
                    OnTaskError(task, cancelToken.Token, nameof(RegisterAccountTask));
                }, tokenSource, TaskContinuationOptions.NotOnRanToCompletion)

                .ContinueWith((_, state) =>
                {
                    var spec = (RegisterAccountTaskSpec)state;
                    spec.Client.RunningTask = false;
                    spec.CancellationTokenSource.Dispose();
                    spec.Client.CurrentTaskCancelToken = null;
                    spec.Connection.Close();
                }, taskSpec);
        }

        private static async Task GetAccountIdTaskRegister(object state)
        {
            var spec = (RegisterAccountTaskSpec)state;
            await spec.Connection.OpenAsync(spec.CancellationTokenSource.Token);
            var (status, accountId) = await DatabaseOperations.FetchAccountId(spec.AccountName, spec.Connection, spec.CancellationTokenSource.Token);

            if (status != DatabaseResultStatus.Ok)
            {
                PacketSender.RegisterAccountReturn(spec.Client.Id, LoginRegisterMessage.Error);
                spec.CancellationTokenSource.Cancel();
                return;
            }

            if (accountId != 0)
            {
                PacketSender.RegisterAccountReturn(spec.Client.Id, LoginRegisterMessage.AccountAlreadyExists);
                spec.CancellationTokenSource.Cancel();
            }
        }

        private static async Task CheckEmailTask(Task _, object state)
        {
            var spec = (RegisterAccountTaskSpec)state;
            var (status, email) = await DatabaseOperations.FetchAccountEmail(spec.Email, spec.Connection, spec.CancellationTokenSource.Token);

            if (status != DatabaseResultStatus.Ok)
            {
                PacketSender.RegisterAccountReturn(spec.Client.Id, LoginRegisterMessage.Error);
                spec.CancellationTokenSource.Cancel();
                return;
            }

            if (!string.IsNullOrEmpty(email))
            {
                PacketSender.RegisterAccountReturn(spec.Client.Id, LoginRegisterMessage.EmailAlreadyUsed);
                spec.CancellationTokenSource.Cancel();
            }
        }

        private static async Task RegisterAccountTask(Task _, object state)
        {
            var spec = (RegisterAccountTaskSpec)state;
            LoginRegisterMessage message = await DatabaseOperations.WriteNewAccount(spec.AccountName, spec.Password, spec.Email, spec.Connection);
            PacketSender.RegisterAccountReturn(spec.Client.Id, message);
        }

        public static async Task CreateCharacterTask(CharacterCreationSpec characterCreationSpec)
        {
            var tokenSource = new CancellationTokenSource();
            Transaction transaction = await DatabaseManager.BeginTransactionAsync(tokenSource.Token);

            if (transaction is null)
            {
                tokenSource.Dispose();
                PacketSender.CreateCharacterReturn(characterCreationSpec.Client.Id, CreateCharacterMessage.Error);
                return;
            }

            characterCreationSpec.Client.RunningTask = true;
            characterCreationSpec.CancellationTokenSource = characterCreationSpec.Client.CurrentTaskCancelToken = tokenSource;
            characterCreationSpec.Transaction = transaction;

            await Task.Factory
                .StartNew(ValidateSentValuesTask, characterCreationSpec, tokenSource.Token).Unwrap()
                .ContinueWith(CreateCharacterTask, characterCreationSpec, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current).Unwrap()
                .ContinueWith((task, state) =>
                {
                    var cancelToken = (CancellationTokenSource)state;
                    OnTaskError(task, cancelToken.Token, nameof(CreateCharacterTask));
                }, tokenSource, TaskContinuationOptions.NotOnRanToCompletion)
                
                .ContinueWith((_, state) =>
                {
                    var spec = (CharacterCreationSpec)state;
                    spec.Client.RunningTask = false;
                    spec.CancellationTokenSource.Dispose();
                    spec.Client.CurrentTaskCancelToken = null;
                }, characterCreationSpec);
        }

        private static async Task<Dictionary<CharactersTableColumn, object>> ValidateSentValuesTask(object state)
        {
            var spec = (CharacterCreationSpec)state;
            
            if (!await CharacterManager.Instance.ValidateSentCharacterValues(spec))
            {
                spec.CancellationTokenSource.Cancel();
                return null;
            }

            var values = new Dictionary<CharactersTableColumn, object>(Queries.CREATE_CHARACTER_QUERY_PARAMS_COUNT)
            {
                { CharactersTableColumn.AccountId, spec.Client.ClientGameData.AccountId },
                { CharactersTableColumn.Username, spec.CharacterName },
                { CharactersTableColumn.Class, spec.Class },
                { CharactersTableColumn.Race, spec.Race },
                { CharactersTableColumn.Gender, spec.Gender },
                { CharactersTableColumn.HeadId, spec.HeadId }
            };
            
            CharacterInitialResources resources;

            if (!spec.IsTemplate)
            {
                AddSkillsToDict(spec.Skills, values);
                resources = CharacterManager.Instance.CalculateInitialResourcesValues((ClassType)spec.Class, (RaceType)spec.Race);
            }
            else
            {
                AddSkillsToDictTemplate(values);
                resources = TemplateCharacterUtils.TemplateResourcesValues((ClassType)spec.Class, (RaceType)spec.Race);
            }

            values.Add(CharactersTableColumn.MaxHealth, resources.Health);
            values.Add(CharactersTableColumn.CurrentHealth, resources.Health);
            values.Add(CharactersTableColumn.MaxMana, resources.Mana);
            values.Add(CharactersTableColumn.CurrentMana, resources.Mana);
            values.Add(CharactersTableColumn.MaxStamina, resources.Stamina);
            values.Add(CharactersTableColumn.CurrentStamina, resources.Stamina);
            values.Add(CharactersTableColumn.MaxHunger, resources.Hunger);
            values.Add(CharactersTableColumn.CurrentHunger, resources.Hunger);
            values.Add(CharactersTableColumn.MaxThirst, resources.Thirst);
            values.Add(CharactersTableColumn.CurrentThirst, resources.Thirst);

            values.Add(CharactersTableColumn.Map, 1); // Map number
            values.Add(CharactersTableColumn.XPos, 40); // X
            values.Add(CharactersTableColumn.YPos, 40); // Y

            return values;
        }

        private static void AddSkillsToDict(Dictionary<Skill, byte> values, Dictionary<CharactersTableColumn, object> dictionary)
        {
            dictionary.Add(CharactersTableColumn.Magic, values[Skill.Magic]);
            dictionary.Add(CharactersTableColumn.ArmedCombat, values[Skill.ArmedCombat]);
            dictionary.Add(CharactersTableColumn.RangedWeapons, values[Skill.RangedWeapons]);
            dictionary.Add(CharactersTableColumn.UnarmedCombat, values[Skill.UnarmedCombat]);
            dictionary.Add(CharactersTableColumn.Stabbing, values[Skill.Stabbing]);
            dictionary.Add(CharactersTableColumn.CombatTactics, values[Skill.CombatTactics]);
            dictionary.Add(CharactersTableColumn.MagicResistance, values[Skill.MagicResistance]);
            dictionary.Add(CharactersTableColumn.ShieldDefense, values[Skill.ShieldDefense]);
            dictionary.Add(CharactersTableColumn.Meditation, values[Skill.Meditation]);
            dictionary.Add(CharactersTableColumn.Survival, values[Skill.Survival]);
            dictionary.Add(CharactersTableColumn.AnimalTaming, values[Skill.AnimalTaming]);
            dictionary.Add(CharactersTableColumn.Hiding, values[Skill.Hiding]);
            dictionary.Add(CharactersTableColumn.Trading, values[Skill.Trading]);
            dictionary.Add(CharactersTableColumn.Thieving, values[Skill.Thieving]);
            dictionary.Add(CharactersTableColumn.Leadership, values[Skill.Leadership]);
            dictionary.Add(CharactersTableColumn.Sailing, values[Skill.Sailing]);
            dictionary.Add(CharactersTableColumn.HorseRiding, values[Skill.HorseRiding]);
            dictionary.Add(CharactersTableColumn.Mining, values[Skill.Mining]);
            dictionary.Add(CharactersTableColumn.Blacksmithing, values[Skill.Blacksmithing]);
            dictionary.Add(CharactersTableColumn.WoodCutting, values[Skill.Woodcutting]);
            dictionary.Add(CharactersTableColumn.WoodWorking, values[Skill.Woodworking]);
            dictionary.Add(CharactersTableColumn.Fishing, values[Skill.Fishing]);
            dictionary.Add(CharactersTableColumn.Tailoring, values[Skill.Tailoring]);
        }

        private static void AddSkillsToDictTemplate(Dictionary<CharactersTableColumn, object> dictionary)
        {
            dictionary.Add(CharactersTableColumn.Magic, 100);
            dictionary.Add(CharactersTableColumn.ArmedCombat, 100);
            dictionary.Add(CharactersTableColumn.RangedWeapons, 100);
            dictionary.Add(CharactersTableColumn.UnarmedCombat, 100);
            dictionary.Add(CharactersTableColumn.Stabbing, 100);
            dictionary.Add(CharactersTableColumn.CombatTactics, 100);
            dictionary.Add(CharactersTableColumn.MagicResistance, 100);
            dictionary.Add(CharactersTableColumn.ShieldDefense, 100);
            dictionary.Add(CharactersTableColumn.Meditation, 100);
            dictionary.Add(CharactersTableColumn.Survival, 100);
            dictionary.Add(CharactersTableColumn.AnimalTaming, 100);
            dictionary.Add(CharactersTableColumn.Hiding, 100);
            dictionary.Add(CharactersTableColumn.Trading, 100);
            dictionary.Add(CharactersTableColumn.Thieving, 100);
            dictionary.Add(CharactersTableColumn.Leadership, 100);
            dictionary.Add(CharactersTableColumn.Sailing, 100);
            dictionary.Add(CharactersTableColumn.HorseRiding, 100);
            dictionary.Add(CharactersTableColumn.Mining, 100);
            dictionary.Add(CharactersTableColumn.Blacksmithing, 100);
            dictionary.Add(CharactersTableColumn.WoodCutting, 100);
            dictionary.Add(CharactersTableColumn.WoodWorking, 100);
            dictionary.Add(CharactersTableColumn.Fishing, 100);
            dictionary.Add(CharactersTableColumn.Tailoring, 100);
        }

        private static async Task CreateCharacterTask(Task<Dictionary<CharactersTableColumn, object>> previousTask, object state)
        {
            var spec = (CharacterCreationSpec)state;

            // First create a new character and get the new Id
            var (status, newCharId) = await DatabaseOperations.WriteNewCharacter(previousTask.Result, spec.Transaction);
            if (status != DatabaseResultStatus.Ok)
            {
                OnDatabaseFailed(spec);
                return;
            }
            
            // Then write the newbie inventory and spells if it's not a template character
            if (!spec.IsTemplate)
            {
                var classType = (ClassType)spec.Class;
                bool isMagicClass = classType != ClassType.Hunter && classType != ClassType.Warrior && classType != ClassType.Worker;
                string inventory = CharacterManager.GetNewbieInventory((RaceType)spec.Race, (Gender)spec.Gender, isMagicClass);
                string spells = isMagicClass ? Constants.STARTING_SPELLS : Constants.EMPTY_SPELLS;
                status = await DatabaseOperations.WriteNewbieInventoryAndSpells(inventory, spells, newCharId, spec.Transaction);
            }
            else
            {
                // If it's a template character write template
                status = await DatabaseOperations.WriteTemplateCharacter(newCharId, TemplateCharacterUtils.GetTemplateHit((ClassType)spec.Class), spec.Transaction);
            }

            if (status != DatabaseResultStatus.Ok)
            {
                OnDatabaseFailed(spec);
                return;
            }
            
            // Finally commit the transaction if everything succeeded
            if (!await spec.Transaction.CommitTransactionAsync())
            {
                spec.CancellationTokenSource.Cancel();
                PacketSender.CreateCharacterReturn(spec.Client.Id, CreateCharacterMessage.Error);
                return;
            }

            spec.Client.ClientGameData.AccountCharacters.Add(newCharId, new AOCharacterInfo(newCharId, spec.CharacterName));
            PacketSender.CreateCharacterReturn(spec.Client.Id, CreateCharacterMessage.Ok, newCharId);
        }
        
        private static void OnDatabaseFailed(CharacterCreationSpec spec)
        {
            spec.CancellationTokenSource.Cancel();
            PacketSender.CreateCharacterReturn(spec.Client.Id, CreateCharacterMessage.Error);
        }

        public static async Task SendPlayerIntoGameTask(Client fromClient, AOCharacterInfo characterInfo)
        {
            fromClient.RunningTask = true;
            
            // Check if this player already exists in the game world and is still valid
            if (TryGetExistingPlayer(characterInfo.CharacterId, out Player newPlayer))
            {
                fromClient.ClientGameData.Player = newPlayer;
                newPlayer.Flags.Disconnecting = false;
            }
            else
            {
                // Otherwise initialize a new game object
                newPlayer = await InitializeNewPlayer(fromClient, characterInfo);
                if (!newPlayer)
                    return;
                
                // Set the player in client data and add it to GameManager dictionary
                fromClient.ClientGameData.Player = newPlayer;
                CharacterManager.Instance.AddOnlinePlayer(newPlayer);
            }

            // Spawn all the existing players in the new player client and the new player for all existing clients 
            CharacterManager.Instance.SendAllPlayersToNewPlayer(fromClient.Id);
            CharacterManager.Instance.SendNewPlayerToAllPlayers(newPlayer);

            // Once the player is spawned send all their character info and the required packets to synchronize them with the map current state
            PacketSender.PlayerMaxResources(newPlayer);
            PacketSender.PlayerResources(newPlayer);
            PacketSender.PlayerUpdatePosition(fromClient.Id, (int)newPlayer.CurrentTile.Position.x, (int)newPlayer.CurrentTile.Position.y);
            PacketSender.PlayerPrivateInfo(newPlayer);
            PacketSender.PlayerAttributes(newPlayer);
            PacketSender.PlayerStats(newPlayer);
            PacketSender.PlayerSkills(newPlayer);
            PacketSender.PlayerTalentsPoints(newPlayer);
            PacketSender.UpdatePlayerSpells(newPlayer);
            // IMPORTANT: Report quests before sending inventory and gold so progresses get updated client-side
            newPlayer.QuestManager.ReportAllQuests(newPlayer);
            newPlayer.Inventory.SendAllInventoryToPlayer();
            PacketSender.PlayerGold(newPlayer);
            GameManager.Instance.MovePlayerIntoNewMap(newPlayer, newPlayer.CurrentMap);

            if (newPlayer.Flags.IsDead)
                PacketSender.UpdatePlayerStatus(newPlayer, PlayerStatus.Died);

            PacketSender.EndEnterWorld(newPlayer.Id);
            fromClient.RunningTask = false;
        }

        private static bool TryGetExistingPlayer(CharacterId characterId, out Player player)
        {
            return CharacterManager.Instance.TryGetOnlinePlayer(characterId, out player) && player;
        }

        private static async Task<Player> InitializeNewPlayer(Client client, AOCharacterInfo characterInfo)
        {
            // If it doesn't already exist get the character data from DB
            var (status, playerData) = await DatabaseOperations.FetchCharacterInfo(characterInfo.CharacterId);
            if (status != DatabaseResultStatus.Ok)
            {
                OnPlayerInitializationFailed(client);
                return null;
            }
            
            // Instantiate a new player prefab and initialize the data
            var newPlayer = CharacterManager.Instance.InstantiatePlayerPrefab(characterInfo.GameMasterRank);
            if (!newPlayer.Initialize(client, characterInfo, playerData))
            {
                OnPlayerInitializationFailed(client);
                return null;
            }

            return newPlayer;
        }

        private static void OnPlayerInitializationFailed(Client client)
        {
            client.RunningTask = false;
            PacketSender.PlayerDisconnected(client.Id);
        }

        private static void OnTaskError(Task failedTask, CancellationToken ct, string taskName)
        {
            // Cancelled by our token, not an error
            if (ct.IsCancellationRequested) 
                return;

            log.Error("Task {0} failed to run to completion. Status: {1}", taskName, failedTask.Status);
            var exception = failedTask.Exception;
            
            if (exception is not null)
            {
                for (int i = 0; i < exception.InnerExceptions.Count; i++)
                {
                    var e = exception.InnerExceptions[i];
                    log.Error("Exception {0}: {1}\n{2}",  i + 1, e.Message, e.StackTrace);
                }
                
                return;
            }

            try
            {
                // Waiting the task rethrows the exception that made it fail
                failedTask.Wait();
            }
            catch (Exception ex)
            {
                log.Error("Task failed with unknown exception: {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
