using System.Collections;
using System.Linq;
using AO.Core.Ids;
using UnityEngine;
using TMPro;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player.Utils;
using AOClient.UI;

namespace AOClient.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public ClientId Id { get; private set; }
        public string Username { get; private set; }
        public string Description { get; set; }
        public ClassType Class { get; private set; }
        public RaceType Race { get; set; }
        public Gender Gender { get; private set; }
        public Faction Faction { get; private set; }
        public Map CurrentMap { get; set; }
        public Inventory[] Inventory { get; private set; }
        public uint Gold { get; private set; }
        public QuestManager QuestManager { get; private set; }
        public PlayerEvents Events { get; private set; }
        public ClickRequest ClickRequest { get; set; } = ClickRequest.NoRequest;
        
        /// <summary>Used to check if the player just logged into the game and avoid playing movement animation.</summary>
        private bool isLoggedIn;
        private Heading heading = Heading.South;
        private bool isDead;
        private bool isSailing;
        private bool isMeditating;
        private Camera mainCamera;

        private Spell[] spells;

        [Header("Sprite Renderers")]
        [SerializeField] private SpriteRenderer helmetRenderer;
        [SerializeField] private SpriteRenderer headRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer shieldRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;

        [Header("Animators")]
        [SerializeField] private Animator armorAnimator;
        [SerializeField] private Animator shieldAnimator;
        [SerializeField] private Animator weaponAnimator;

        [Header("Text Boxes")]
        [SerializeField] private TextMeshProUGUI chatBubble;
        [SerializeField] private TextMeshProUGUI nameTag;
        [SerializeField] private Canvas nameTagCanvas;

        [Header("Layer Sorter")]
        [SerializeField] private LayerSorter layerSorter;

        private ParticleSystem meditationParticles;
        private RuntimeAnimatorController defaultBodyAnim, ghostBodyAnim, bodyBeforeSailing;
        /// <summary>Contains the head sprites facing up, down, left, right.</summary>
        private Sprite[] headSprites = new Sprite[4];
        /// <summary>Contains the helm sprites if the player has one equipped.</summary>
        private Sprite[] helmSprites = new Sprite[4];
        /// <summary>Contains the ghost form head sprites facing up, down, left, right.</summary>
        private Sprite[] ghostHeadSprites = new Sprite[4];
        
        private void Start()
        {
            chatBubble.richText = false;
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case Layer.OBSTACLES:
                    layerSorter.OnObstacleCollisionEnter(collision);
                    break;
                case Layer.ROOF when Id == Client.Instance.MyId:
                    mainCamera.cullingMask = ~(1 << Layer.Roof.Id);
                    break;
                case Layer.MAP:
                    var map = collision.GetComponent<Map>();
                    GameManager.Instance.ChangedMap(this, map.Number);
                    break;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == Layer.Obstacles.Id)
            {
                layerSorter.OnObstacleCollisionExit(collision);
            }
            else if (collision.gameObject.layer == Layer.Roof.Id)
            {
                if (Id == Client.Instance.MyId)
                    mainCamera.cullingMask = ~(1 >> Layer.Roof.Id);
            }
        }

        /// <summary>Instantiates a new player.</summary>
        public void Initialize(ref SpawnPlayerInfo info, bool isLocalPlayer)
        {
            //gameObject.AddComponent<ThreeDTests>();

            if (isLocalPlayer)
            {
               Inventory = new Inventory[Constants.PLAYER_INV_SPACE];
               QuestManager = new QuestManager();
               Events = new PlayerEvents();
               spells = new Spell[Constants.PLAYER_SPELL_SPACE];
            }
            
            nameTag.text = info.Username;
            mainCamera = Camera.main;
            nameTag.canvas.worldCamera = mainCamera;
            chatBubble.canvas.worldCamera = mainCamera;

            Id = info.ClientId;
            Username = info.Username;
            Description = info.Description;
            name = Username;
            Class = (ClassType)info.Class;
            Race = (RaceType)info.Race;
            Gender = (Gender)info.Gender;
            Faction = (Faction)info.Faction;

            defaultBodyAnim = GetDefaultBodyAnim(Race, Gender);
            armorAnimator.runtimeAnimatorController = defaultBodyAnim;
            headSprites = GameManager.Instance.GetHeadSprites(info.HeadId);

            if (Race is RaceType.Dwarf or RaceType.Gnome)
                headRenderer.gameObject.transform.position = Constants.ShortHeadPosition;

            SetFaction(Faction, info.IsGm);

            ghostHeadSprites = GameManager.Instance.GetHeadSprites(Constants.GHOST_HEAD);
            ghostBodyAnim = GameManager.Instance.GetArmorAnimation(Constants.GHOST_ANIMATION);
        }

        /// <summary>Sets the player's private information.</summary>
        public void LoadPlayerPrivateInfo(byte @class)
        {
            UIManager.GameUI.StatsWindow.SetClassAndTalents((ClassType)@class);
            UIManager.GameUI.PlayerInfo.SetName(Username);
        }

        /// <summary>Sets the player's name tag color according to the faction.</summary>
        public void SetFaction(Faction faction, bool isGm)
        {
            Faction = faction;
            
            if (isGm)
            {
                nameTag.color = Constants.GameMasterColor;
            }
            else
            {
                switch (faction)
                {
                    case Faction.Citizen:
                        nameTag.color = Constants.CitizenColor;
                        break;
                    case Faction.Criminal:
                        nameTag.color = Constants.CriminalColor;
                        break;
                    case Faction.Imperial:
                        nameTag.color = Constants.ImperialColor;
                        break;
                    case Faction.Chaos:
                        nameTag.color = Constants.ChaosColor;
                        break;
                }
            }

            if (Id == Client.Instance.MyId)
                UIManager.GameUI.StatsWindow.SetFaction(faction);
        }

        /// <summary>Sets the player's resources max amount.</summary>
        public static void SetMaxResources(int maxHealth, ushort maxMana, ushort maxStamina, ushort maxHunger, ushort maxThirst)
        {
            UIManager.GameUI.PlayerResources.SetMaxHpText(maxHealth);
            UIManager.GameUI.PlayerResources.SetMaxManaText(maxMana);
            UIManager.GameUI.PlayerResources.SetMaxStaminaText(maxStamina);
            UIManager.GameUI.PlayerResources.SetMaxHungerText(maxHunger);
            UIManager.GameUI.PlayerResources.SetMaxThirstText(maxThirst);
        }

        /// <summary>Updates the player's resources current values.</summary>
        public static void UpdateResources(int health, ushort mana, ushort stamina, ushort hunger, ushort thirst)
        {
            UIManager.GameUI.PlayerResources.SetCurrentHpText(health);
            UIManager.GameUI.PlayerResources.SetCurrentManaText(mana);
            UIManager.GameUI.PlayerResources.SetCurrentStaminaText(stamina);
            UIManager.GameUI.PlayerResources.SetCurrentHungerText(hunger);
            UIManager.GameUI.PlayerResources.SetCurrentThirstText(thirst);
        }

        /// <summary>Writes text to the chat bubble.</summary>
        /// <param name="message">The message to display.</param>
        /// <param name="isChat">Whether it's a regular chat or spell magic words.</param>
        public void SetChatBubbleText(string message, bool isChat)
        {
            if (!gameObject.activeInHierarchy) 
                return;
            
            chatBubble.color = isChat ? new Color32(255, 255, 255 ,255) : new Color32(65, 190, 156, 255);
            chatBubble.text = message;
            StopCoroutine(ClearChatBubble());
            StartCoroutine(ClearChatBubble());
        }

        /// <summary>Clears the chat bubble.</summary>
        public void ClearChatText()
        {
            chatBubble.text = string.Empty;
        }

        public Inventory[] GetSlotsWithItem(ItemId itemId)
        {
            return Inventory.Where(x => x is not null && x.Item.Id == itemId).ToArray();
        }

        /// <summary>Adds an item to the player's inventory.</summary>
        public void AddItemToInventory(byte slot, ItemId itemId, ushort quantity, uint sellingPrice)
        {
            Inventory newSlot = new Inventory(slot, GameManager.Instance.GetItem(itemId), quantity, sellingPrice);
            Inventory[slot] = newSlot;
            Events.RaiseInventorySlotChanged(newSlot);
            UIManager.GameUI.InventoryUI.UpdateInventory(newSlot, false);
        }

        /// <summary>Updates the player's inventory according to new quantity.</summary>
        public void UpdateInventory(byte slotNumber, ushort quantity)
        {
            Inventory slot = Inventory[slotNumber];
            slot.Quantity = quantity;
            Events.RaiseInventorySlotChanged(slot);
            if (slot.Quantity <= 0)
                Inventory[slotNumber] = null;

            UIManager.GameUI.InventoryUI.UpdateInventory(slot, slot.Quantity <= 0);
        }

        /// <summary>Swaps the two items' slots.</summary>
        public void SwapInventorySlots(byte slotA, byte slotB)
        {
            Inventory oldSlot = Inventory[slotA];
            Inventory newSlot = Inventory[slotB];

            Inventory[slotA] = newSlot;
            Inventory[slotB] = oldSlot;

            oldSlot.Slot = slotB;

            if (newSlot is not null)
                newSlot.Slot = slotA;
            
            UIManager.GameUI.InventoryUI.SwapSlots(slotA, slotB);
        }

        public void ClearEquippedItems()
        {
            armorAnimator.runtimeAnimatorController = defaultBodyAnim;
            weaponAnimator.runtimeAnimatorController = null;
            shieldAnimator.runtimeAnimatorController = null;
            helmSprites = null;
        }

        /// <summary>Equips an item and changes the animation.</summary>
        public void EquipItem(byte slot, Item item, bool equipped)
        {
            switch (item.ItemType)
            {
                case ItemType.Armor:
                    if (!isSailing && !isDead)
                        armorAnimator.runtimeAnimatorController = equipped ? GameManager.Instance.GetArmorAnimation(item.AnimationId) : defaultBodyAnim;
                    break;
                case ItemType.Weapon:
                    weaponAnimator.runtimeAnimatorController = equipped ? GameManager.Instance.GetWeaponAnimation(item.AnimationId) : null;
                    break;
                case ItemType.Shield:
                    shieldAnimator.runtimeAnimatorController = equipped ? GameManager.Instance.GetShieldAnimation(item.AnimationId) : null;
                    break;
                case ItemType.Helmet:
                    helmetRenderer.transform.localPosition = Constants.Hats.Contains(item.AnimationId) ? Constants.HatRendererPosition : Constants.HelmRendererPosition;
                    helmSprites = equipped ? GameManager.Instance.GetHelmSprites(item.AnimationId) : null;
                    break;
                case ItemType.Mount:
                    break;
            }

            if (GameManager.Instance.LocalPlayer.Id == Id)
            {
                Inventory[slot].Equipped = equipped;
                UIManager.GameUI.InventoryUI.ItemEquip(slot, equipped);
            }
        }

        /// <summary>Updates the player's total gold on ui and raises gold changed event.</summary>
        public void UpdateGold(uint gold)
        {
            Gold = gold;
            Events.RaiseTotalGoldChanged(gold);
            UIManager.GameUI.PlayerInfo.SetGold(gold);
        }

        /// <summary>Updates the player's spells at the slot where the spell is.</summary>
        public void UpdateSpells(Spell spell, byte slot)
        {
            spell.Slot = slot;
            spells[spell.Slot] = spell;
            UIManager.GameUI.Spells.UpdateSpells(spell);
        }

        /// <summary>Returns the spell at the specified index.</summary>
        public Spell GetSpellAtIndex(byte index) => spells[index];

        public void MoveSpells(byte slotOne, byte slotTwo)
        {
            Spell spell = spells[slotOne];
            spells[slotOne] = spells[slotTwo];
            spells[slotOne].Slot = slotOne;
            spells[slotTwo] = spell;
            spells[slotTwo].Slot = slotTwo;

            UIManager.GameUI.Spells.SpellMoved(slotOne, slotTwo);
        }

        /// <summary>Changes the animations to the ghost body and head.</summary>
        public void Die()
        {
            //If the player died while sailing
            if (isSailing)
            {
                //Get the dead boat animation
                armorAnimator.runtimeAnimatorController = GameManager.Instance.GetArmorAnimation(Constants.DEAD_BOAT_ANIMATION);
            }
            else
            {
                //If not sailing set the regular ghost form
                if (Race is RaceType.Dwarf or RaceType.Gnome)
                    headRenderer.gameObject.transform.localPosition = Constants.TallHeadPosition;

                headRenderer.sprite = ghostHeadSprites[(int)heading];
                armorAnimator.runtimeAnimatorController = ghostBodyAnim;
            }

            isDead = true;
        }

        /// <summary>Resets the animations back to the naked body.</summary>
        public void Revive()
        {
            if (Race is RaceType.Dwarf or RaceType.Gnome)
                headRenderer.gameObject.transform.localPosition = Constants.ShortHeadPosition;

            headRenderer.sprite = headSprites[(int)heading];

            //If the player is revived while sailing
            if (isSailing)
            {
                //Get the live boat animation
                armorAnimator.runtimeAnimatorController = GameManager.Instance.GetArmorAnimation(Constants.BOAT_ANIMATION);
            }
            else
            {
                //If the player got revived on ground set the naked body
                armorAnimator.runtimeAnimatorController = defaultBodyAnim;
            }

            isDead = false;
        }

        public void UseBoat(bool isSailing)
        {
            this.isSailing = isSailing;

            //If the player just got on the boat
            if (isSailing)
            {
                //Save the body animation
                bodyBeforeSailing = armorAnimator.runtimeAnimatorController;

                //Get right boat animation id depending on if the player is dead or not
                ushort boatAnimId = isDead ? Constants.DEAD_BOAT_ANIMATION : Constants.BOAT_ANIMATION;
                armorAnimator.runtimeAnimatorController = GameManager.Instance.GetArmorAnimation(boatAnimId);

                //Disable all the other animators
                headRenderer.gameObject.SetActive(false);
                shieldAnimator.gameObject.SetActive(false);
                weaponAnimator.gameObject.SetActive(false);
            }
            else //If the player is getting out of the boat
            {
                //Set the body animation accordingly
                armorAnimator.runtimeAnimatorController = isDead ? ghostBodyAnim : bodyBeforeSailing;

                //Re-enable all the other animators
                headRenderer.gameObject.SetActive(true);
                shieldAnimator.gameObject.SetActive(true);
                weaponAnimator.gameObject.SetActive(true);
            }
        }

        private void ChangeSortingLayer()
        {
            float yPosition = transform.position.y;
            helmetRenderer.sortingOrder = (int)(CurrentMap.Boundaries.max.y * 2 - yPosition * 2) + 1;
            headRenderer.sortingOrder = (int)(CurrentMap.Boundaries.max.y * 2 - yPosition * 2);
            bodyRenderer.sortingOrder = (int)(CurrentMap.Boundaries.max.y * 2 - yPosition * 2);
            shieldRenderer.sortingOrder = (int)(CurrentMap.Boundaries.max.y * 2 - yPosition * 2);
            weaponRenderer.sortingOrder = (int)(CurrentMap.Boundaries.max.y * 2 - yPosition * 2);
            nameTagCanvas.sortingOrder = (int)(CurrentMap.Boundaries.max.y * 2 - yPosition * 2);
        }

        /// <summary>Plays the walking animation according to the facing direction.</summary>
        public void PlayAnimationOnce(Heading heading, bool isMoving)
        {
            // TODO avoid playing animations the very first time the player logs in
            if (isLoggedIn)
            {
                this.heading = heading;

                armorAnimator.SetInteger(Constants.FacingHash, (int)heading);
                armorAnimator.SetBool(Constants.IsMovingHash, isMoving);

                //Avoid warning log cause animator shouldn't be changed if it doesn't have an animation
                if (shieldAnimator.runtimeAnimatorController)
                {
                    shieldAnimator.SetInteger(Constants.FacingHash, (int)heading);
                    shieldAnimator.SetBool(Constants.IsMovingHash, isMoving);
                }

                if (weaponAnimator.runtimeAnimatorController)
                {
                    weaponAnimator.SetInteger(Constants.FacingHash, (int)heading);
                    weaponAnimator.SetBool(Constants.IsMovingHash, isMoving);
                }

                headRenderer.sprite = isDead ? ghostHeadSprites[(int)heading] : headSprites[(int)heading];
                helmetRenderer.sprite = helmSprites?[(int)heading];

                if (isMoving)
                {
                    ChangeSortingLayer();
                    
                    switch (heading)
                    {
                        case Heading.North:
                            armorAnimator.Play(Constants.BodyUpHash);
                            PlayShieldAnimation(Constants.ShieldUpHash);
                            PlayWeaponAnimation(Constants.WeaponUpHash);
                            break;
                        case Heading.South:
                            armorAnimator.Play(Constants.BodyDownHash);
                            PlayShieldAnimation(Constants.ShieldDownHash);
                            PlayWeaponAnimation(Constants.WeaponDownHash);
                            break;
                        case Heading.West:
                            armorAnimator.Play(Constants.BodyLeftHash);
                            PlayShieldAnimation(Constants.ShieldLeftHash);
                            PlayWeaponAnimation(Constants.WeaponLeftHash);
                            break;
                        case Heading.East:
                            armorAnimator.Play(Constants.BodyRightHash);
                            PlayShieldAnimation(Constants.ShieldRightHash);
                            PlayWeaponAnimation(Constants.WeaponRightHash);
                            break;
                    }
                }
            }
            isLoggedIn = true;
        }

        /// <summary>Updates the position label.</summary>
        public void UpdatePosition(Vector2 position)
        {
            if (CurrentMap is not null)
            {
                float x = position.x - CurrentMap.Boundaries.min.x;
                float y = CurrentMap.Boundaries.max.y - position.y;
                UIManager.GameUI.MapPosition.UpdatePosition($"Mapa: {CurrentMap.Number} - X: {x + 0.5f} - Y: {y - 0.5f}", CurrentMap.Name);
            }
        }

        public void Meditate()
        {
            if (isMeditating)
            {
                Destroy(meditationParticles.gameObject);
            }
            else
            {
                meditationParticles = Instantiate(GameManager.Instance.GetParticles(2), transform, true);
                meditationParticles.transform.localPosition = Vector3.zero;
            }

            isMeditating = !isMeditating;
        }
        
        public void SetClickRequest(ClickRequest request)
        {
            UIManager.ChangeCursor(UIManager.CastCursor);
            ClickRequest = request;
            var console = UIManager.GameUI.Console;

            switch (request)
            {
                case ClickRequest.CastSpell:
                    console.WriteLine(Constants.CLICK_REQUEST_CAST_SPELL);
                    break;
                case ClickRequest.ProjectileAttack:
                    console.WriteLine(Constants.CLICK_REQUEST_PROJECTILE_ATTACK);
                    break;
                case ClickRequest.TameAnimal:
                    console.WriteLine(Constants.CLICK_REQUEST_TAME_ANIMAL);
                    break;
                case ClickRequest.PetChangeTarget:
                    console.WriteLine(Constants.CLICK_REQUEST_PET_CHANGE_TARGET);
                    break;
                case ClickRequest.Steal:
                    console.WriteLine(Constants.CLICK_REQUEST_STEAL);
                    break;
                case ClickRequest.InviteToParty:
                    console.WriteLine(Constants.CLICK_REQUEST_INVITE_PARTY);
                    break;
                case ClickRequest.Mine:
                    console.WriteLine(Constants.CLICK_REQUEST_MINE);
                    break;
                case ClickRequest.CutWood:
                    console.WriteLine(Constants.CLICK_REQUEST_CUT_WOOD);
                    break;
                case ClickRequest.Fish:
                    console.WriteLine(Constants.CLICK_REQUEST_FISH);
                    break;
                case ClickRequest.Smelt:
                    console.WriteLine(Constants.CLICK_REQUEST_SMELT);
                    break;
                case ClickRequest.CraftBlacksmithing:
                    console.WriteLine(Constants.CLICK_REQUEST_CRAFT_BLACKSMITHING);
                    break;
            }
        }

        /// <summary>Clears the chat bubble after 10 seconds.</summary>
        private IEnumerator ClearChatBubble()
        {
            yield return new WaitForSeconds(10);
            chatBubble.text = string.Empty;
        }

        /// <summary>Plays a shield animation if the player has a shield equipped</summary>
        private void PlayShieldAnimation(int directionHash)
        {
            //Check if the animator is active and has a valid animation,(it gets deactivated when on a boat)
            if (shieldAnimator.gameObject.activeSelf && shieldAnimator.runtimeAnimatorController)
                shieldAnimator.Play(directionHash);
        }

        /// <summary>Plays a weapon animation if the player has a weapon equipped</summary>
        private void PlayWeaponAnimation(int directionHash)
        {
            //Check if the animator is active and has a valid animation,(it gets deactivated when on a boat)
            if (weaponAnimator.gameObject.activeSelf && weaponAnimator.runtimeAnimatorController)
                weaponAnimator.Play(directionHash);
        }

        /// <summary>Gets the naked body anim according to race and gender.</summary>
        private static RuntimeAnimatorController GetDefaultBodyAnim(RaceType race, Gender gender)
        {
            var anim = race switch
            {
                RaceType.Human => gender == Gender.Male ? DefaultAnimation.HumanMale : DefaultAnimation.HumanFemale,
                RaceType.Elf => gender == Gender.Male ? DefaultAnimation.ElfMale : DefaultAnimation.ElfFemale,
                RaceType.NightElf => gender == Gender.Male ? DefaultAnimation.NelfMale : DefaultAnimation.NelfFemale,
                RaceType.Dwarf => gender == Gender.Male ? DefaultAnimation.DwarfMale : DefaultAnimation.DwarfFemale,
                RaceType.Gnome => gender == Gender.Male ? DefaultAnimation.GnomeMale : DefaultAnimation.GnomeFemale,
                _ => DefaultAnimation.HumanMale
            };
            return GameManager.Instance.GetArmorAnimation((ushort)anim);
        }
    }
}
