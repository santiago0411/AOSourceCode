using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Utilities;

namespace AOClient.UI
{
    public class LoginRegister : MonoBehaviour
    {
        public bool Registered { get; set; }

        //[0] is account [1] is password [2] is confirmPW, [3] is email
        private TMP_InputField[] inputFields;
        private GameObject windowPopup;
        private TextMeshProUGUI windowPopupText;  
        private int fieldIndex = 1; //Starts on 1 because 0 is already selected
        private bool canTab = true;
        private int currentSceneIndex;
        private TMP_InputField ipField;


        private void Start()
        {
            currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            windowPopup = GameObject.Find("Popup");
            windowPopupText = windowPopup.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.gameObject.name.Equals("WindowText"));
            windowPopup.SetActive(false);
            windowPopup.GetComponentInChildren<Button>().onClick.AddListener(HidePopupWindow);

            if (currentSceneIndex == (int)Scene.Login)
            {
                ipField = GameObject.Find("IpInputField").GetComponent<TMP_InputField>();

                FindInputFields(new[] { "AccountInputField", "PasswordInputField" });

                #if UNITY_EDITOR
                inputFields[0].text = "Chaji";
                inputFields[1].text = "1234";
                #endif

                Button loginButton = GameObject.Find("LogInButton").GetComponent<Button>();
                loginButton.onClick.AddListener(Login);

                Button registerButton = GameObject.Find("RegisterButton").GetComponent<Button>();
                registerButton.onClick.AddListener(() => SceneLoader.Instance.LoadRegisterScene());

                if (Client.Instance.IsConnected)
                {
                    Client.Instance.Disconnect();
                }

                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var versionTextField = GameObject.Find("Version").GetComponent<TextMeshProUGUI>();
                versionTextField.text = $"Version: {version}";
            }
            else if (currentSceneIndex == (int)Scene.Register)
            {
                FindInputFields(new[] { "AccountInputField", "PasswordInputField", "ConfirmPasswordInputField", "EmailInputField" });

                Button registerButton = GameObject.Find("RegisterButton").GetComponent<Button>();
                registerButton.onClick.AddListener(Register);

                Button cancelButton = GameObject.Find("CancelButton").GetComponent<Button>();
                cancelButton.onClick.AddListener(() => SceneLoader.Instance.LoadLoginScene());
            }
        }

        private void Update()
        {
            if (canTab)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (inputFields.Length <= fieldIndex)
                    {
                        fieldIndex = 0;
                    }
                    inputFields[fieldIndex].Select();
                    fieldIndex++;
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (currentSceneIndex == (int)Scene.Login)
                {
                    Login();
                }
                else if (currentSceneIndex == (int)Scene.Register)
                {
                    Register();
                }
            }
        }

        private void Login()
        {
            if (CheckFieldsAreEmpty())
            {
                ShowPopupWindow("Debe completar todos los campos");
                return;
            }

            if (!string.IsNullOrEmpty(ipField.text))
            {
                Client.Instance.Ip = ipField.text;
            }

            if (!Client.Instance.IsConnected)
                Client.Instance.ConnectToServer(LoginCallback);
            else
                LoginCallback();
        }

        private void LoginCallback()
        {
            string passwordHash = Cryptography.HashPassword(inputFields[1].text);

            if (Client.Instance.IsConnected)
                PacketSender.Login(inputFields[0].text, passwordHash);
            else
                ShowPopupWindow("No se pudo conectar al servidor.");
        }

        private void Register()
        {
            if (CheckFieldsAreEmpty())
            {
                ShowPopupWindow("Debe completar todos los campos.");
                return;
            }

            if (!string.IsNullOrEmpty(ipField.text))
            {
                Client.Instance.Ip = ipField.text;
            }


            if (!Client.Instance.IsConnected)
                Client.Instance.ConnectToServer(RegisterCallback);
            else
                RegisterCallback();
        }

        private void RegisterCallback()
        {
            string passwordHash = Cryptography.HashPassword(inputFields[1].text);
            string confirmHash = Cryptography.HashPassword(inputFields[2].text);

            if (passwordHash.Equals(confirmHash))
            {
                if (Client.Instance.IsConnected)
                    PacketSender.RegisterAccount(inputFields[0].text, passwordHash, inputFields[3].text);
                else
                    ShowPopupWindow("No se pudo conectar al servidor.");
            }
            else
            {
                ShowPopupWindow("Las contraseñas ingresadas no coinciden.");
            }
        }

        public void ShowPopupWindow(string message)
        {
            windowPopupText.text = message;
            canTab = false;
            windowPopup.SetActive(true);
        }

        public void HidePopupWindow()
        {
            canTab = true;
            if (Registered)
            {
                if (currentSceneIndex == (int)Scene.Register)
                    SceneLoader.Instance.LoadLoginScene();
            }
            else
            {
                windowPopup.SetActive(false);
            }
        }

        private void FindInputFields(string[] names)
        {
            inputFields = FindObjectsOfType<TMP_InputField>();
            TMP_InputField[] aux = new TMP_InputField[inputFields.Length];
            Array.Copy(inputFields, aux, inputFields.Length);

            //Orders input fields according to the names sent
            for (int i = 0; i < names.Length; i++)
            {
                inputFields[i] = aux.FirstOrDefault(x => x.gameObject.name.Equals(names[i]));
            }

            inputFields[0].Select();
        }

        private bool CheckFieldsAreEmpty()
        {
            return inputFields.Any(field => string.IsNullOrWhiteSpace(field.text));
        }
    }
}
