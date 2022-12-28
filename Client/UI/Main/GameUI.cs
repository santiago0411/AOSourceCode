using AOClient.UI.Main.Crafting;
using AOClient.UI.Main.Inventory;
using AOClient.UI.Main.Mailing;
using AOClient.UI.Main.Party;
using AOClient.UI.Main.PlayerInfo;
using AOClient.UI.Main.Questing;
using AOClient.UI.Main.Spells;
using UnityEngine;

namespace AOClient.UI.Main
{
    public class GameUI : MonoBehaviour
    {
        public ConsoleUI Console => console;
        public RectTransform CameraTransform { get; private set; }

        public InventoryUI InventoryUI => inventoryUI;
        public DropItemsUI DropItems => dropItems;
        public SpellsUI Spells => spells;
        
        public PlayerInfoUI PlayerInfo => playerInfo;
        public PlayerResourcesUI PlayerResources => playerResources;
        public MenuUI Menu => menu;
        public MapPositionUI MapPosition => mapPosition;
        
        public StatsWindowUI StatsWindow => statsWindow;
        public NpcTradeWindowUI NpcTradeWindow => npcTradeWindow;
        public CraftingUI CraftingWindow => craftingWindow;
        public QuestWindowUI QuestWindow => questWindow;
        public PartyWindowUI PartyWindow => partyWindow;
        public MailUI MailWindow => mailWindow;
        
        
        [SerializeField] private ConsoleUI console;
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private DropItemsUI dropItems;
        [SerializeField] private SpellsUI spells;
        [SerializeField] private PlayerInfoUI playerInfo;
        [SerializeField] private PlayerResourcesUI playerResources;
        [SerializeField] private MenuUI menu;
        [SerializeField] private MapPositionUI mapPosition;
        [SerializeField] private StatsWindowUI statsWindow;
        [SerializeField] private NpcTradeWindowUI npcTradeWindow;
        [SerializeField] private CraftingUI craftingWindow;
        [SerializeField] private QuestWindowUI questWindow;
        [SerializeField] private PartyWindowUI partyWindow;
        [SerializeField] private MailUI mailWindow;

        private void Start()
        {
            var cameraSlot = GameObject.Find("CameraSlot");
            CameraTransform = cameraSlot.transform as RectTransform;
            cameraSlot.SetActive(false);
        }
    }
}