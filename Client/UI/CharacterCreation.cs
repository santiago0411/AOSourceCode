using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using AOClient.UI.Main;

namespace AOClient.UI
{
    public class CharacterCreation : MonoBehaviour
    {
        public static Dictionary<RaceType, Dictionary<PlayerAttribute, sbyte>> RacesAttValues { get; set; }
        public bool Created { get; set; }

        [Header("General")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button createButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Toggle templateToggle;

        [Header("PopUp")]
        [SerializeField] private GameObject windowPopup;
        [SerializeField] private TextMeshProUGUI windowPopupText;
        [SerializeField] private Button windowPopupButton;

        [Header("Skills")]
        [SerializeField] private Transform skillsPanel;
        [SerializeField] private SkillUI skillPrefab;

        [Header("Dropdowns")]
        [SerializeField] private Dropdown classesDropdown;
        [SerializeField] private Dropdown racesDropdown;
        [SerializeField] private Dropdown genderDropdown;
        
        [Header("Attributes")]
        [SerializeField] private TextMeshProUGUI[] modifiers;
        [SerializeField] private TextMeshProUGUI[] finalValues;

        private int assignedSkills;

        private readonly Dictionary<Skill, SkillUI> skills = new();
        private List<Tuple<RaceType, byte>> maleHeads;
        private List<Tuple<RaceType, byte>> femaleHeads;

        private void Start()
        {
            windowPopupButton.onClick.AddListener(HidePopupWindow);

            createButton.onClick.AddListener(CreateCharacter);
            cancelButton.onClick.AddListener(SceneLoader.Instance.LoadCharacterScreenScene);
            racesDropdown.onValueChanged.AddListener((_) => OnRacesDropdownValueChanged());

            foreach (var entry in Constants.SkillsNames)
            {
                SkillUI skillUI = Instantiate(skillPrefab, skillsPanel);
                skillUI.Skill = entry.Key;
                skillUI.SkillNameText.text = entry.Value;
                skillUI.AddButton.onClick.AddListener(() => AddSkill(skillUI.Skill));
                skillUI.SubButton.onClick.AddListener(() => RemoveSkill(skillUI.Skill));

                skills.Add(entry.Key, skillUI);
            }

            LoadHeads();
            OnRacesDropdownValueChanged();
        }

        public void ShowPopupWindow(string message)
        {
            windowPopupText.text = message;
            windowPopup.SetActive(true);
        }

        public void HidePopupWindow()
        {
            if (Created)
            {
                windowPopup.SetActive(false);
                return;
            }

            SceneLoader.Instance.LoadCharacterScreenScene();
        }

        private void AddSkill(Skill skill)
        {
            if (assignedSkills < 10)
            {
                assignedSkills += 1;
                SkillUI skillUI = skills[skill];
                skillUI.Value += 1;
                skillUI.SkillValueText.text = skillUI.Value.ToString();
            }
        }

        private void RemoveSkill(Skill skill)
        {
            SkillUI skillUI = skills[skill];

            if (skillUI.Value > 0)
            {
                assignedSkills -= 1;
                skillUI.Value -= 1;
                skillUI.SkillValueText.text = skillUI.Value.ToString();
            }
        }

        private void CreateCharacter()
        {
            string charName = nameInputField.text;
            byte @class = Convert.ToByte(classesDropdown.value + 1);
            byte race = Convert.ToByte(racesDropdown.value + 1);
            byte gender = Convert.ToByte(genderDropdown.value);
            byte headId = gender == 0
                        ? femaleHeads.FirstOrDefault(x => x.Item1 == (RaceType)race)!.Item2
                        : maleHeads.FirstOrDefault(x => x.Item1 == (RaceType)race)!.Item2;

            PacketSender.CreateCharacter(templateToggle.isOn, charName, @class, race, headId, gender, skills);
        }

        private void OnRacesDropdownValueChanged()
        { 
            for (int i = 0; i < 5; i++)
            {
                RaceType race = (RaceType)racesDropdown.value + 1;
                PlayerAttribute att = (PlayerAttribute)i + 1;
                modifiers[i].text = RacesAttValues[race][att].ToString();
                finalValues[i].text = (18 + RacesAttValues[race][att]).ToString();
            }
        }

        private void LoadHeads()
        {
            maleHeads = new List<Tuple<RaceType, byte>>()
            {
                new(RaceType.Human, 1),
                new(RaceType.Human, 4),
                new(RaceType.Elf, 2),
                new(RaceType.NightElf, 10),
                new(RaceType.Dwarf, 5),
                new(RaceType.Gnome, 9),
            };

            femaleHeads = new List<Tuple<RaceType, byte>>()
            {
                new(RaceType.Human, 8),
                new(RaceType.Elf, 6),
                new(RaceType.NightElf, 7),
                new(RaceType.Dwarf, 11),
                new(RaceType.Gnome, 12),
            };
        }
    }
}
