using AOClient.Questing.Rewards;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Questing
{
    public class QuestRewardPanelHelper : MonoBehaviour
    {
        private static readonly Vector2 defaultCellSize = new(225f, 32f);
        private static readonly Vector2 itemCellSize = new(32f, 32f);
        
        [SerializeField] private GridLayoutGroup[] panelGrids;
        private byte[] locks;

        private byte choosableItemsPanelId, itemsPanelId;
        private GridLayoutGroup choosableItemsPanel, itemsPanel;
        
        private void Awake()
        {
            locks = new byte[panelGrids.Length];
        }

        public void LoadPanel(IQuestReward reward)
        {
            GridLayoutGroup panel;
            byte id;
            Vector2 size;
            
            switch (reward)
            {
                case ItemReward _:
                    panel = itemsPanel ? itemsPanel : GetFirstAvailablePanel(out itemsPanelId);
                    id = itemsPanelId;
                    size = itemCellSize;
                    break;
                case ChoosableItemReward _:
                    panel = choosableItemsPanel ? choosableItemsPanel : GetFirstAvailablePanel(out choosableItemsPanelId);
                    id = choosableItemsPanelId;
                    size = itemCellSize;
                    break;
                default:
                    panel = GetFirstAvailablePanel(out id);
                    size = defaultCellSize;
                    break;
            }
            
            if (panel == null) return;

            panel.cellSize = size;
            reward.AddRewardToPanel(panel.transform);
            
            if (panel.transform.childCount == 0)
                locks[id] = 0;
        }

        public void Reset()
        {
            for (byte i = 0; i < panelGrids.Length; i++)
            {
                locks[i] = 0;
                foreach (Transform child in panelGrids[i].transform)
                {
                    child.SetParent(transform);
                    child.gameObject.SetActive(false);
                }
            }
        }

        private GridLayoutGroup GetFirstAvailablePanel(out byte panelId)
        {
            for (byte i = 0; i < panelGrids.Length; i++)
            {
                if (locks[i] == 0)
                {
                    locks[i] = 1;
                    panelId = i;
                    return panelGrids[i];
                }
            }

            panelId = default;
            return null;
        }
    }
}