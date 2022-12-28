using System;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Players;
using AO.World;
using AO.Items;
using AO.Players.Talents;
using AO.Players.Talents.Worker;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Professions
{
    public static class CollectionProfessions
    {
        private static Item[][] fishes;
        private static Item[] regularWood, elficWood, ironOre, silverOre, goldOre, ironIngot, silverIngot, goldIngot;

        public static void SetItems()
        {
            fishes = new[]
            {
                new[]
                {
                    GameManager.Instance.GetItem(Constants.PORGY_ID)
                },
                new[]
                {
                    GameManager.Instance.GetItem(Constants.PORGY_ID),
                    GameManager.Instance.GetItem(Constants.PEJERREY_ID)
                },
                new[]
                {
                    GameManager.Instance.GetItem(Constants.PORGY_ID),
                    GameManager.Instance.GetItem(Constants.PEJERREY_ID),
                    GameManager.Instance.GetItem(Constants.HAKE_ID)
                },
                new[]
                {
                    GameManager.Instance.GetItem(Constants.PORGY_ID),
                    GameManager.Instance.GetItem(Constants.PEJERREY_ID),
                    GameManager.Instance.GetItem(Constants.HAKE_ID),
                    GameManager.Instance.GetItem(Constants.SWORDFISH_ID)
                },
                new[]
                {
                    GameManager.Instance.GetItem(Constants.PORGY_ID),
                    GameManager.Instance.GetItem(Constants.PEJERREY_ID),
                    GameManager.Instance.GetItem(Constants.HAKE_ID),
                    GameManager.Instance.GetItem(Constants.SWORDFISH_ID),
                    GameManager.Instance.GetItem(Constants.HIPPOCAMPUS_ID)
                },
            };
            
            // These are arrays because WorkParameters::CollectingItems is an array
            // so to not allocate memory every time a new WorkParameters is created, these static arrays are used instead
            regularWood = new[] { GameManager.Instance.GetItem(Constants.WOOD_ID) };
            elficWood = new[] { GameManager.Instance.GetItem(Constants.ELFIC_WOOD_ID) };
            ironOre = new[] { GameManager.Instance.GetItem(Constants.IRON_ORE_ID) };
            silverOre = new[] { GameManager.Instance.GetItem(Constants.SILVER_ORE_ID) };
            goldOre = new[] { GameManager.Instance.GetItem(Constants.GOLD_ORE_ID) };
            ironIngot = new[] { GameManager.Instance.GetItem(Constants.IRON_INGOT_ID) };
            silverIngot = new[] { GameManager.Instance.GetItem(Constants.SILVER_INGOT_ID) };
            goldIngot = new[] { GameManager.Instance.GetItem(Constants.GOLD_INGOT_ID) };
        }
        
        // ########## FISHING START ########## //
        public static void TryStartFishing(Player player, Vector2 clickPosition)
        {
            if (!player.Inventory.TryGetEquippedItem(ItemType.Weapon, out Item equippedTool))
                return;

            if (equippedTool.WeaponType != WeaponType.FishingRod && equippedTool.WeaponType != WeaponType.FishingNet)
                return;
            
            player.Flags.IsWorking = false;

            clickPosition.Round(0);

            if (!WorldMap.Tiles.TryGetValue(clickPosition, out Tile tileAtPosition))
                return;

            if (!tileAtPosition.IsWater)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NoWaterToFish);
                return;
            }

            if ((player.CurrentTile.Position - clickPosition).magnitude > 4f)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToFish);
                return;
            }

            var parameters = new WorkParameters();
            const bool clickedOnSchool = false; // TODO schools
            GetFishableFish(player, equippedTool, clickedOnSchool, parameters);
            player.StartCoroutine(player.WorkCoroutine(TryFishing, parameters));
        }
        
        private static void GetFishableFish(Player player, Item equippedTool, bool clickedOnSchool, WorkParameters parameters)
        {
            // Non worker classes can only fish the most basic fish
            if (player.Class.ClassType != ClassType.Worker)
            {
                parameters.CollectingItems = fishes[0];
                return;
            }
            
            // Set the index to the all fishes which is the last index in fishes array (fishes.Length - 1)
            // Subtracting 1 from this variable will remove one type of fish
            int fishesArrayIndex = fishes.Length - 1;
            
            // If the player is fishing with a net set the amount modifier
            // No need to check for the net talent because it can't be equipped without it
            bool usingNet = equippedTool.WeaponType == WeaponType.FishingNet; 
            if (usingNet)
                parameters.FishingAmountModifier = Constants.FISHING_NET_AMOUNT_MOD;
            
            TalentTree<FishingTalent> fishingTalentTree = player.WorkerTalentTrees.FishingTree;
            
            // If the player isn't in an unsafe zone subtract 1
            if (player.Flags.ZoneType != ZoneType.UnsafeZone)
                fishesArrayIndex -= 1;
            
            // Check if the player is sailing, and if they are, get the boat item
            if (player.Flags.IsSailing && player.Inventory.TryGetEquippedItem(ItemType.Boat, out Item boat))
            {
                // If the player is sailing check if they are trying to fish from a school
                if (clickedOnSchool)
                {
                    // To fish from a school the player must be on a galley, using a fishing net and must have the school node
                    var schoolNode = fishingTalentTree.GetNode(FishingTalent.SchoolFishing);
                    if (boat.IsGalley && usingNet && schoolNode.Acquired)
                    {
                        // Nothing was subtracted from index so all the fishes will be chosen (schools only spawn in unsafe zones)
                        parameters.CollectingItems = fishes[fishesArrayIndex];
                        return;
                    }

                    // If the player couldn't fish from a school subtract 1 if is using a galley 2 if using a regular boat
                    fishesArrayIndex -= boat.IsGalley ? 1 : 2;
                }
                else
                {
                    // If the player isn't fishing from a school subtract 1 if is using a galley 2 if using a regular boat
                    fishesArrayIndex -= boat.IsGalley ? 1 : 2;
                }
            }
            else
            {
                // If the player isn't sailing subtract 3 from the index
                // -1 for not fishing from a school, -1 from the boat not being a galley and -1 more for not having a boat
                fishesArrayIndex -= 3;
            }

            // Now check for the fish types that require talents to fish them

            var swordFishNode = fishingTalentTree.GetNode(FishingTalent.FishSwordFish);
            if (!swordFishNode.Acquired && fishesArrayIndex == 3)
                fishesArrayIndex -= 1;
            
            var hakeNode = fishingTalentTree.GetNode(FishingTalent.FishHake);
            if (!hakeNode.Acquired && fishesArrayIndex == 2)
                fishesArrayIndex -= 1;
            
            var pejerreyNode = fishingTalentTree.GetNode(FishingTalent.FishPejerrey);
            if (!pejerreyNode.Acquired && fishesArrayIndex == 1)
                fishesArrayIndex -= 1;
            
            // If fishesArrayIndex is negative it means that the player doesn't have any talents unlocked
            // Finally set CollectingItems to fishesArrayIndex or 0 if it was negative
            parameters.CollectingItems = fishes[fishesArrayIndex];
        }

        private static void TryFishing(Player player, WorkParameters parameters)
        {
            if (!PlayerMethods.TakeStamina(player, Constants.FISHING_STAM_COST))
                return;

            byte skill = player.Skills[Skill.Fishing];
            int luck = Mathf.RoundToInt(-0.00125f * skill * skill - 0.3f * skill + 49);

            if (ExtensionMethods.RandomNumber(1, luck) <= 6)
            {
                int max = Math.Max(1, (player.Level - 4) / 5);
                ushort amount = (ushort)(ExtensionMethods.RandomNumber(1, max) * parameters.FishingAmountModifier);
                Item fish = parameters.CollectingItems[UnityEngine.Random.Range(0, parameters.CollectingItems.Length)];
                /*
                float maxAmount;
                byte playerLevel = player.Level;
                switch (random)
                {
                    case 0:
                        fish = porgy;
                        maxAmount = 0.00125f * playerLevel * playerLevel + 0.3f * playerLevel + 4;
                        break;
                    case 1:
                        fish = pejerrey;
                        maxAmount
                        break;
                    case 2:
                        fish = hake;
                        maxAmount
                        break;
                    case 3:
                        fish = swordfish;
                        maxAmount
                        break;
                    case 4:
                        fish = hippocampus;
                        maxAmount = ((0.00003 * playerLevel - 0.002f) * playerLevel + 0.098f) * playerLevel + 0.7f;
                        break;
                }
                */
                player.Events.RaiseResourceGathered(fish.Id, amount);
                PlayerMethods.TryLevelSkill(player, Skill.Fishing);
                
                if (!player.Inventory.AddItemToInventory(fish, amount))
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.InventoryFull);
                    player.Flags.IsWorking = false;
                }
            }
        }
        // ########## FISHING END ########## //
        
        
        // ########## WOODCUTTING START ########## //
        public static void TryStartCuttingWood(Player player, Collider2D collision, Vector2 collisionPosition)
        {
            player.Flags.IsWorking = false;

            var parameters = new WorkParameters();
            if (!CheckTreeCollision(player, collision, parameters))
                return;

            if ((player.CurrentTile.Position - collisionPosition).magnitude > 2f)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToCutWood);
                return;
            }

            if (player.Class.ClassType == ClassType.Worker)
            {
                var woodCuttingTalentTree = player.WorkerTalentTrees.WoodCuttingTree;
                var fastCuttingNode = woodCuttingTalentTree.GetNode(WoodCuttingTalent.FastCutting);

                parameters.IntervalModifier = fastCuttingNode.Points switch
                {
                    1 => WoodCuttingNodesConstants.WOOD_CUTTING_SPEED_MOD_1,
                    2 => WoodCuttingNodesConstants.WOOD_CUTTING_SPEED_MOD_2,
                    3 => WoodCuttingNodesConstants.WOOD_CUTTING_SPEED_MOD_3,
                    _ => parameters.IntervalModifier
                };
            }

            player.StartCoroutine(player.WorkCoroutine(TryCuttingWood, parameters));
        }
        
        private static bool CheckTreeCollision(Player player, Collider2D collision, WorkParameters parameters)
        {
            if (collision)
            {
                if (collision.CompareTag(Tag.Tree.Name))
                {
                    parameters.CollectingItems = regularWood;
                    return true;
                }
                if (collision.CompareTag(Tag.ElficWood.Name))
                {
                    if (!CanCutWood(player, WoodCuttingTalent.CutElficWood))
                        return false;
                    
                    parameters.CollectingItems = elficWood;
                    return true;
                }
            }
            
            PacketSender.SendMultiMessage(player.Id, MultiMessage.NoTreeToCut);
            return false;
        }

        private static bool CanCutWood(Player player, WoodCuttingTalent talentNeeded)
        {
            var woodCuttingTalentTree = player.WorkerTalentTrees?.WoodCuttingTree;
            var node = woodCuttingTalentTree?.GetNode(talentNeeded);

            if (node is null || !node.Acquired)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantCutThatTree);
                return false;
            }

            return true;
        }
        
        private static void TryCuttingWood(Player player, WorkParameters parameters)
        {
            if (!PlayerMethods.TakeStamina(player, Constants.WOODCUTTING_STAM_COST))
                return;

            byte skill = player.Skills[Skill.Woodcutting];
            int luck = Mathf.RoundToInt(-0.00125f * skill * skill - 0.3f * skill + 49);

            if (ExtensionMethods.RandomNumber(1, luck) <= 6) 
            {
                int max = Math.Max(1, (player.Level - 4) / 5);
                ushort amount = (ushort)ExtensionMethods.RandomNumber(1, max);
                Item wood = parameters.CollectingItems[0];

                player.Events.RaiseResourceGathered(wood.Id, amount);
                PlayerMethods.TryLevelSkill(player, Skill.Woodcutting);
                
                if (!player.Inventory.AddItemToInventory(wood, amount))
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.InventoryFull);
                    player.Flags.IsWorking = false;
                }
            }
        }
        // ########## WOODCUTTING END ########## //
        
        
        // ########## MINING START ########## //
        public static void TryStartMining(Player player, Collider2D collision, Vector2 collisionPosition)
        {
            player.Flags.IsWorking = false;

            var parameters = new WorkParameters();
            if (!CheckDepositCollision(player, collision, parameters))
                return;
            
            if ((player.CurrentTile.Position - collisionPosition).magnitude > 2f)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToMine);
                return;
            }

            if (player.Class.ClassType == ClassType.Worker)
            {
                var miningTalentTree = player.WorkerTalentTrees.MiningTree;
                var fastMiningNode = miningTalentTree.GetNode(MiningTalent.FastMining);

                parameters.IntervalModifier = fastMiningNode.Points switch
                {
                    1 => MiningNodesConstants.MINING_SPEED_MOD_1,
                    2 => MiningNodesConstants.MINING_SPEED_MOD_2,
                    3 => MiningNodesConstants.MINING_SPEED_MOD_3,
                    _ => parameters.IntervalModifier
                };
            }
            
            player.StartCoroutine(player.WorkCoroutine(TryMining, parameters));
        }
        
        private static bool CheckDepositCollision(Player player, Collider2D collision, WorkParameters parameters)
        {
            if (collision)
            {
                if (collision.CompareTag(Tag.IronDeposit.Name))
                {
                    parameters.CollectingItems = ironOre;
                    return true;
                }
                if (collision.CompareTag(Tag.SilverDeposit.Name))
                {
                    if (!CanMineDeposit(player, MiningTalent.MineSilver))
                        return false;

                    parameters.CollectingItems = silverOre;
                    return true;
                }
                if (collision.CompareTag(Tag.GoldDeposit.Name))
                {
                    if (!CanMineDeposit(player, MiningTalent.MineGold))
                        return false;

                    parameters.CollectingItems = goldOre;
                    return true;
                }
            }
            
            PacketSender.SendMultiMessage(player.Id, MultiMessage.NoDepositToMine);
            return false;
        }

        private static bool CanMineDeposit(Player player, MiningTalent talentNeeded)
        {
            var miningTalentTree = player.WorkerTalentTrees?.MiningTree;
            var node = miningTalentTree?.GetNode(talentNeeded);
            
            if (node is null || !node.Acquired)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantMineThat);
                return false;
            }

            return true;
        }
        
        private static void TryMining(Player player, WorkParameters parameters)
        {
            if (!PlayerMethods.TakeStamina(player, Constants.MINING_STAM_COST))
                return;

            byte skill = player.Skills[Skill.Mining];
            int luck = Mathf.RoundToInt(-0.00125f * skill * skill - 0.3f * skill + 49);

            if (ExtensionMethods.RandomNumber(1, luck) <= 5)
            {
                int max = Math.Max(1, (player.Level - 4) / 5);
                ushort amount = (ushort)ExtensionMethods.RandomNumber(1, max);
                Item ore = parameters.CollectingItems[0];
                
                player.Events.RaiseResourceGathered(ore.Id, amount);
                PlayerMethods.TryLevelSkill(player, Skill.Mining);
                
                if (!player.Inventory.AddItemToInventory(ore, amount))
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.InventoryFull);
                    player.Flags.IsWorking = false;
                }
            }
        }
        // ########## MINING END ########## //
        
        
        // ########## SMELTING START ########## //
        public static void TryStartSmelting(Player player, Collider2D collision, Vector2 collisionPosition)
        {
            player.Flags.IsWorking = false;

            if (!collision || !collision.CompareTag(Tag.Forge.Name))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NoForgeToSmelt);
                return;
            }

            if ((player.CurrentTile.Position - collisionPosition).magnitude > 2f)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToSmelt);
                return;
            }

            if (!player.Inventory.TryGetEquippedItem(ItemType.Miscellaneous, out var item))
                return;

            if (!CanSmeltMineral(player, item.Id))
                return;

            var workStruct = new WorkParameters();
            player.StartCoroutine(player.WorkCoroutine(SmeltOre, workStruct));
        }
        
        private static bool CanSmeltMineral(Player player, ItemId itemId)
        {
            MiningTalent talentNeeded;

            if (itemId == Constants.IRON_ORE_ID)
                return true;

            if (itemId == Constants.SILVER_ORE_ID)
                talentNeeded = MiningTalent.MineSilver;
            else if (itemId == Constants.GOLD_ID)
                talentNeeded = MiningTalent.MineGold;
            else
                return false;

            bool canSmelt = false;
            if (player.Class.ClassType == ClassType.Worker)
            {
                var miningTalentTree = player.WorkerTalentTrees.MiningTree;
                canSmelt = miningTalentTree.GetNode(talentNeeded).Acquired;
            }

            if (!canSmelt)
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantSmeltThat);

            return canSmelt;
        }

        private static void SmeltOre(Player player, WorkParameters parameters)
        {
            if (!player.Inventory.TryGetEquippedItemSlot(ItemType.Miscellaneous, out var oreSlot))
            {
                player.Flags.IsWorking = false;
                return;
            }

            (bool hasEnoughOre, Item ingot, ushort amountOfIngots, ushort oreNeeded) = CalculateOreNeeded(player, parameters, oreSlot);
            
            if (!hasEnoughOre)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughOre);
                player.Flags.IsWorking = false;
                return;
            }

            if (!PlayerMethods.TakeStamina(player, (ushort)(Constants.SMELTING_STAM_COST * amountOfIngots)))
                return;

            if (player.Inventory.AddItemToInventory(ingot, amountOfIngots))
            {
                oreSlot.Quantity -= oreNeeded;
                player.Events.RaiseResourceGathered(ingot.Id, amountOfIngots);
                PacketSender.PlayerUpdateInventory(player.Id, oreSlot.Slot, oreSlot.Quantity);
                PlayerMethods.TryLevelSkill(player, Skill.Mining);
                return;
            }
            
            PacketSender.SendMultiMessage(player.Id, MultiMessage.InventoryFull);
            player.Flags.IsWorking = false;
        }

        private static (bool hasEnoughOre, Item ingot, ushort amountOfIngots, ushort oreNeeded) CalculateOreNeeded(Player player, WorkParameters parameters, InventorySlot invSlot)
        {
            if (player.Class.ClassType != ClassType.Worker)
                parameters.SmeltingOreRequiredModifier = 0.7f;
            
            ushort amountOfIngots = 10;
            ushort oreNeeded, oreNeededForSingleIngot;
            Item ingot;
            
            switch (invSlot.Item.MiscellaneousType)
            {
                case MiscellaneousType.IronOre:
                    oreNeeded = (ushort)(Constants.ORE_REQUIRED_IRON_INGOT * parameters.SmeltingOreRequiredModifier * amountOfIngots);
                    oreNeededForSingleIngot = (ushort)(Constants.ORE_REQUIRED_IRON_INGOT * parameters.SmeltingOreRequiredModifier);
                    ingot = ironIngot[0];
                    break;
                case MiscellaneousType.SilverOre:
                    oreNeeded = (ushort)(Constants.ORE_REQUIRED_SILVER_INGOT * parameters.SmeltingOreRequiredModifier * amountOfIngots);
                    oreNeededForSingleIngot = (ushort)(Constants.ORE_REQUIRED_SILVER_INGOT * parameters.SmeltingOreRequiredModifier);
                    ingot = silverIngot[0];
                    break;
                case MiscellaneousType.GoldOre:
                    oreNeeded = (ushort)(Constants.ORE_REQUIRED_GOLD_INGOT * parameters.SmeltingOreRequiredModifier * amountOfIngots);
                    oreNeededForSingleIngot = (ushort)(Constants.ORE_REQUIRED_GOLD_INGOT * parameters.SmeltingOreRequiredModifier);
                    ingot = goldIngot[0];
                    break;
                default:
                    player.Flags.IsWorking = false;
                    return (false, null, 0, 0);
            }

            bool enoughOre = true;
            if (invSlot.Quantity < oreNeeded)
            {
                // If the player doesn't have enough ore calculate how many ingots they can make with the remaining ore
                amountOfIngots = (ushort)(invSlot.Quantity / oreNeededForSingleIngot);
                oreNeeded =  (ushort)(amountOfIngots * oreNeededForSingleIngot);
                enoughOre = amountOfIngots >= 1;
            }

            return (enoughOre, ingot, amountOfIngots, oreNeeded);
        }
        // ########## SMELTING END ########## //
    }
}