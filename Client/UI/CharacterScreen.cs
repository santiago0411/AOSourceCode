using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using UnityEngine;
using UnityEngine.UI;
using AOClient.Core.Utils;

namespace AOClient.UI
{
    public class CharacterScreen : MonoBehaviour
    {
        public static bool CharactersReceived { get; private set; }

        [SerializeField] private Button createCharacterButton;
        [SerializeField] private Button enterWorldButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Dropdown charsDropdown;

        [SerializeField] private Dropdown sceneDropdown;

        private Dictionary<CharacterId, string> characters;

        private void Start()
        {
            createCharacterButton.onClick.AddListener(SceneLoader.Instance.LoadCharacterCreatingScene);
            enterWorldButton.onClick.AddListener(EnterWorld);
            exitButton.onClick.AddListener(SceneLoader.Instance.LoadLoginScene);
            
            sceneDropdown.AddOptions(new List<string> { "Dev", "Game World" });
        }

        private void OnDestroy()
        {
            CharactersReceived = false;
        }

        public void LoadCharacters(Dictionary<CharacterId, string> chars)
        {
            CharactersReceived = true;

            if (chars is null)
            {
                SceneLoader.Instance.LoadCharacterCreatingScene();
                return;
            }

            characters = chars;

            foreach (var c in chars)
                charsDropdown.AddOptions(new List<string> { c.Value });
        }

        private void EnterWorld()
        {
            string selectedCharName = charsDropdown.options[charsDropdown.value].text;
            CharacterId selectedCharId = characters.Where(x => x.Value.Equals(selectedCharName)).Select(x => x.Key).FirstOrDefault();
            var selectedScene = sceneDropdown.value == 0 ? Scene.Dev : Scene.Main;
            SceneLoader.Instance.LoadMainScene(selectedCharId, selectedScene);
        }
    }
}
