using System;
using AOClient.Core.Utils;
using AOClient.Network;
using UnityEngine;
using TMPro;

namespace AOClient.UI.Main
{
    public sealed class ConsoleUI : MonoBehaviour
    {
        public GameObject Chatbox => chatbox;
        public TMP_InputField ChatboxField => chatboxField;

        [SerializeField] private GameObject chatbox;
        [SerializeField] private TMP_InputField chatboxField;
        [SerializeField] private TextMeshProUGUI consoleText, fpsText, msText;

        private int lines;
        private int frameCounter;
        private float timeCounter;
        private float lastFramerate;
        private const float REFRESH_TIME = 0.5f;

        private void Start()
        {
            Chatbox.SetActive(false);
            chatboxField.richText = false;
            
            consoleText.text = string.Empty;
            consoleText.parseCtrlCharacters = true;
        }

        private void Update()
        {
            if (timeCounter < REFRESH_TIME)
            {
                timeCounter += Time.deltaTime;
                frameCounter++;
            }
            else
            {
                lastFramerate = frameCounter / timeCounter;
                frameCounter = 0;
                timeCounter = 0.0f;
                fpsText.text = $"FPS: {(int)lastFramerate}";
                msText.text = $"MS: {Client.Instance.Ms}";
            }
        }

        /// <summary>Writes a new line to the console and formats the text according to the type.</summary>
        public void WriteLine(string text, ConsoleMessage type = ConsoleMessage.DefaultMessage)
        {
            consoleText.text += type switch
            {
                ConsoleMessage.DefaultMessage => $"<color=#41be9c>{text}</color>\n",
                ConsoleMessage.Warning => $"<b><i><color=#2033df>{text}</color></i></b>\n",
                ConsoleMessage.PlayerClickCitizen => $"<color=#2757ff>{text}</color>\n",
                ConsoleMessage.PlayerClickCriminal => $"<color=#bf1a1a>{text}</color>\n",
                ConsoleMessage.PlayerClickImperial => $"<color=#38b6ff>{text}</color>\n",
                ConsoleMessage.PlayerClickChaos => $"<color=#851212>{text}</color>\n",
                ConsoleMessage.PlayerClickGameMaster => $"<color=#00F000>{text}</color>\n",
                ConsoleMessage.Combat => $"<b><color=red>{text}</color></b>\n",
                _ => $"<color=#41be9c>{text}</color>\n"
            };

            lines++;
            if (lines >= 50)
            {
                int firstLine = consoleText.text.IndexOf("\n", StringComparison.Ordinal);
                consoleText.text = consoleText.text.Substring(firstLine);
                lines--;
            }
        }
    }
}
