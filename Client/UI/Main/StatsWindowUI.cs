using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AOClient.Core;
using AOClient.Network;
using AOClient.Player;
using AOClient.UI.Main.Talents;
using AOClient.UI.Main.Talents.Worker;

namespace AOClient.UI.Main
{
    public sealed class StatsWindowUI : MonoBehaviour
    {
        public WorkerTalentsUI WorkerTalents { get; private set; }
        public readonly HashSet<TalentNodeUIBase> NodesToSendSkillUp = new();

        public byte AvailableTalentPoints
        {
            get => availableTalentPoints;
            set
            {
                availableTalentPoints = value;
                availableTalentPointsText.text = $"Puntos de Talento: {value}";
            }
        }

        public ushort AssignableSkills
        {
            get => assignableSkills;
            set
            {
                assignableSkills = value;
                assignableSkillsText.text = $"Skills Libres: {value}";
            }
        }
        
        [SerializeField] private Button closeButton, showSkillsButton, showTalentsButton;

        [Header("Attributes")]
        [SerializeField] private TextMeshProUGUI[] attributesValues;

        [Header("Character")]
        [SerializeField] private TextMeshProUGUI faction, playerClass;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI[] statsValues;

        [Header("Skills")]
        [SerializeField] private Transform skillsPanel;
        [SerializeField] private TextMeshProUGUI skillsTitleText;
        [SerializeField] private SkillUI skillPrefab;
        [SerializeField] private TextMeshProUGUI assignableSkillsText;

        [Header("Talents")] 
        [SerializeField] private WorkerTalentsUI workerTalentsPrefab;
        [SerializeField] private TextMeshProUGUI availableTalentPointsText;

        private ushort assignableSkills;
        private byte availableTalentPoints;
        private Transform skillsTalentsParentPanel;
        
        private readonly Dictionary<Skill, byte> skillsChanged = new();
        private readonly Dictionary<Skill, SkillUI> skills = new();

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
            skillsTalentsParentPanel = skillsPanel.parent;
            
            foreach (var (skill, skillName) in Constants.SkillsNames)
            {
                SkillUI newSkill = Instantiate(skillPrefab, skillsPanel);
                newSkill.Skill = skill;
                newSkill.AddButton.onClick.AddListener(() => AddSkill(newSkill.Skill));
                newSkill.SubButton.onClick.AddListener(() => SubSkill(newSkill.Skill));
                newSkill.SkillNameText.text = $"{skillName}:";
                skills.Add(newSkill.Skill, newSkill);
            }
            
            showSkillsButton.onClick.AddListener(() =>
            {
                WorkerTalents.gameObject.SetActive(false);
                skillsPanel.gameObject.SetActive(true);
                skillsTitleText.gameObject.SetActive(true);
            });
            
            showTalentsButton.onClick.AddListener(() =>
            {
                skillsPanel.gameObject.SetActive(false);
                skillsTitleText.gameObject.SetActive(false);
                WorkerTalents.gameObject.SetActive(true);
            });
            
            Close();
        }

        public void ShowHideWindow()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void SetFaction(Faction faction)
        {
            switch (faction)
            {
                case Faction.Citizen:
                    this.faction.text = "Status: Ciudadano";
                    break;
                case Faction.Criminal:
                    this.faction.text = "Status: Criminal";
                    break;
                case Faction.Imperial:
                    this.faction.text = "Status: Imperial";
                    break;
                case Faction.Chaos:
                    this.faction.text = "Status: Caos";
                    break;
            }
        }

        public void SetStat(PlayerStat playerStat, uint value)
        {
            switch (playerStat)
            {
                case PlayerStat.CriminalsKilled:
                    statsValues[0].text = value.ToString();
                    break;
                case PlayerStat.CitizensKilled:
                    statsValues[1].text = value.ToString();
                    break;
                case PlayerStat.UsersKilled:
                    statsValues[2].text = value.ToString();
                    break;
                case PlayerStat.NpcsKilled:
                    statsValues[3].text = value.ToString();
                    break;
                case PlayerStat.Deaths:
                    statsValues[4].text = value.ToString();
                    break;
                case PlayerStat.RemainingJailTime:
                    statsValues[5].text = value.ToString();
                    break;
            }
        }

        public void SetStats(List<uint> stats)
        {
            for (int i = 0; i < stats.Count; i++)
                statsValues[i].text = stats[i].ToString();
        }

        public void SetClassAndTalents(ClassType @class)
        {
            SetClassName(@class);
            if (@class != ClassType.Worker)
            {
                showSkillsButton.transform.parent.gameObject.SetActive(false);
                availableTalentPointsText.gameObject.SetActive(false);
                return;
            }

            WorkerTalents = Instantiate(workerTalentsPrefab, skillsTalentsParentPanel);
            WorkerTalents.gameObject.SetActive(false);
        }

        private void SetClassName(ClassType @class)
        {
            string className = "Clase: ";
            switch (@class)
            {
                case ClassType.Mage:
                    className += "Mago";
                    break;
                case ClassType.Druid:
                    className += "Druida";
                    break;
                case ClassType.Cleric:
                    className += "Clérigo";
                    break;
                case ClassType.Bard:
                    className += "Bardo";
                    break;
                case ClassType.Paladin:
                    className += "Paladín";
                    break;
                case ClassType.Assassin:
                    className += "Asesino";
                    break;
                case ClassType.Warrior:
                    className += "Guerrero";
                    break;
                case ClassType.Hunter:
                    className += "Cazador";
                    break;
                case ClassType.Worker:
                    className += "Trabajador";
                    break;
            }

            playerClass.text = className;
        }

        public void SetAttributes(List<byte> attributes)
        {
            for (int i = 0; i < attributes.Count; i++)
                attributesValues[i].text = attributes[i].ToString();
        }

        public void SetSkills(Dictionary<Skill, byte> skillsValues)
        {
            foreach (var entry in skillsValues)
                SetSkill(entry.Key, entry.Value);
        }

        public void SetSkill(Skill skill, byte value)
        {
            SkillUI skillUI = skills[skill];
            skillUI.Value = value;
            skillUI.SkillValueText.text = value.ToString();
        }

        private void AddSkill(Skill skill)
        {
            SkillUI skillUI = skills[skill];

            if (assignableSkills > 0 && skillUI.Value < 100) //Check that the player has assignable skills and that the value of the skill is lower than 100
            {
                //Add it to the dictionary of skills changed and the original value. Only the first time modifying, so that the player CANNOT remove more points that are on the server (original value)
                if (!skillsChanged.ContainsKey(skillUI.Skill)) 
                    skillsChanged.Add(skillUI.Skill, skillUI.Value);

                skillUI.Value++;
                AssignableSkills--;
            }

            skillUI.SkillValueText.text = skillUI.Value.ToString();
        }

        private void SubSkill(Skill skill)
        {
            SkillUI skillUI = skills[skill];

            if (skillUI.Value > 0) //Check that the skill value is greater than zero to not get negatives
            {
                if (skillsChanged.TryGetValue(skillUI.Skill, out byte originalValue)) //If the dictionary contains the skill it means the player just added extra points
                {
                    if (skillUI.Value > originalValue) //Allow to remove a point as long as the value is greater than the original
                    {
                        skillUI.Value--;
                        AssignableSkills++;
                        if (skillUI.Value == originalValue) //If the player is back to the original value remove it from the dictionary because it shouldn't be sent to the server
                            skillsChanged.Remove(skillUI.Skill);
                    }
                }
            }

            skillUI.SkillValueText.text = skillUI.Value.ToString();
        }

        private void Close()
        {
            if (skillsChanged.Count > 0) //Only notify the server if the player actually assigned skills
            {
                var auxDic = new Dictionary<Skill, byte>();

                foreach (var entry in skillsChanged)
                    auxDic.Add(entry.Key, skills[entry.Key].Value); //Dont add the value in skillsChanged dictionary because thats the unchanged value, instead add the modified value inside the skill

                PacketSender.SkillsChanged(auxDic);
            }

            if (NodesToSendSkillUp.Count > 0)
                PacketSender.SkillUpTalents(NodesToSendSkillUp);
            
            skillsChanged.Clear();
            NodesToSendSkillUp.Clear();
            gameObject.SetActive(false);
        }
    }
}
