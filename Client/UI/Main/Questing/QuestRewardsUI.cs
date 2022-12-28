using System.Collections.ObjectModel;
using UnityEngine;
using AOClient.Core;
using AOClient.Questing.Rewards;

namespace AOClient.UI.Main.Questing
{
    [RequireComponent(typeof(QuestRewardPanelHelper))]
    public class QuestRewardsUI : MonoBehaviour
    {
        public GoldRewardUI GoldRewardInstance
        {
            get
            {
                goldRewardInstance.gameObject.SetActive(true);
                return goldRewardInstance;
            }
        }

        public ExperienceRewardUI ExperienceRewardInstance
        {
            get
            {
                experienceRewardInstance.gameObject.SetActive(true);
                return experienceRewardInstance;
            }
        }
        
        public SkillRewardUI SkillRewardInstance
        {
            get
            {
                skillRewardInstance.gameObject.SetActive(true);
                return skillRewardInstance;
            }
        }

        public ItemRewardUI ItemRewardInstance
        {
            get
            {
                var itemRewardUI = itemsRewardsPool.GetObject();
                itemRewardUI.gameObject.SetActive(true);
                return itemRewardUI;
            }
        }

        public FactionRankRewardUI FactionRankRewardInstance
        {
            get
            {
                factionRankRewardInstance.gameObject.SetActive(true);
                return factionRankRewardInstance;
            }
        }

        public TalentPointRewardUI TalentPointRewardInstance
        {
            get
            {
                talentPointRewardInstance.gameObject.SetActive(true);
                return talentPointRewardInstance;
            }
        }
 
        [Header("Rewards Prefabs")] 
        [SerializeField] private GoldRewardUI goldRewardPrefab;
        [SerializeField] private ExperienceRewardUI experienceRewardPrefab;
        [SerializeField] private SkillRewardUI skillRewardPrefab;
        [SerializeField] private ItemRewardUI itemRewardPrefab;
        [SerializeField] private FactionRankRewardUI factionRankRewardPrefab;
        [SerializeField] private TalentPointRewardUI talentPointRewardPrefab;
        
        private GoldRewardUI goldRewardInstance;
        private ExperienceRewardUI experienceRewardInstance;
        private SkillRewardUI skillRewardInstance;
        private FactionRankRewardUI factionRankRewardInstance;
        private TalentPointRewardUI talentPointRewardInstance;
        private Pool<ItemRewardUI> itemsRewardsPool;

        private QuestRewardPanelHelper rewardPanelHelper;

        private void Awake()
        {
            goldRewardInstance = Instantiate(goldRewardPrefab);
            experienceRewardInstance = Instantiate(experienceRewardPrefab);
            skillRewardInstance = Instantiate(skillRewardPrefab);
            factionRankRewardInstance = Instantiate(factionRankRewardPrefab);
            talentPointRewardInstance = Instantiate(talentPointRewardPrefab);
            itemsRewardsPool = new Pool<ItemRewardUI>(itemRewardPrefab);
            rewardPanelHelper = GetComponent<QuestRewardPanelHelper>();
        }

        public void LoadRewards(ReadOnlyCollection<IQuestReward> rewards)
        {
            foreach (var reward in rewards)
                rewardPanelHelper.LoadPanel(reward);
        }
        
        public void ResetRewards()
        {
            itemsRewardsPool.ResetObjects();
            rewardPanelHelper.Reset();
        }
    }
}