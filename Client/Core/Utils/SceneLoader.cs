using System.Collections;
using System.Threading.Tasks;
using AO.Core.Ids;
using AOClient.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using AOClient.UI;
using UnityEngine.UI;
using Scene = AOClient.UI.Scene;

namespace AOClient.Core.Utils
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        public bool ReadyToShowMain { get; set; }

        [SerializeField] private GameObject loadingScreenCanvas;
        [SerializeField] private Slider loadingSlider;

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }
        }

        public void LoadLoginScene()
        {
            loadingScreenCanvas.SetActive(false);
            SceneManager.LoadScene((int)Scene.Login);
        }

        public void LoadLoginScene(string message)
        {
            StartCoroutine(LoadLoginSceneAsync(message));
        }

        private IEnumerator LoadLoginSceneAsync(string message)
        {
            loadingScreenCanvas.SetActive(false);
            var loadingScene =  SceneManager.LoadSceneAsync((int)Scene.Login);

            while (!loadingScene.isDone)
                yield return null;

            UIManager.LoginRegister.ShowPopupWindow(message);
        }

        public void LoadRegisterScene()
        {
            loadingScreenCanvas.SetActive(false);
            SceneManager.LoadScene((int)Scene.Register);
        }

        public void LoadCharacterScreenScene()
        {
            StartCoroutine(LoadCharacterScreenSceneAsync());
        }

        private IEnumerator LoadCharacterScreenSceneAsync()
        {
            loadingScreenCanvas.SetActive(false);
            var loadingScene = SceneManager.LoadSceneAsync((int)Scene.CharacterScreen);

            while (!loadingScene.isDone && !CharacterScreen.CharactersReceived)
                yield return null;

            PacketSender.GetCharacters();

            while (!CharacterScreen.CharactersReceived)
                yield return null;
        }

        public void LoadCharacterCreatingScene()
        {
            StartCoroutine(LoadCharacterCreatingSceneAsync());
        }

        private IEnumerator LoadCharacterCreatingSceneAsync()
        {
            loadingScreenCanvas.SetActive(false);
            PacketSender.GetRacesAttributes();

            var loadingScene = SceneManager.LoadSceneAsync((int)Scene.CharacterCreation);

            while (!loadingScene.isDone && CharacterCreation.RacesAttValues is null)
                yield return null;
        }

        public async void LoadMainScene(CharacterId characterId, Scene worldScene)
        {
            ReadyToShowMain = false;
            loadingScreenCanvas.SetActive(true);
            var scene = SceneManager.LoadSceneAsync((int)worldScene);
            scene.allowSceneActivation = false;
            loadingSlider.value = 0;
            
            do
            {
                await Task.Delay(100);
                loadingSlider.value = scene.progress * 100;
            } while (scene.progress < 0.9f); // Unity finished loading the scene at 90% for some reason
            
            scene.allowSceneActivation = true;

            await Task.Delay(500);
            
            GameManager.Instance.Initialize();
            
            // Await for the GameManager to finish initialing
            while (!GameManager.GameManagerLoaded)
                await Task.Delay(100);
            
            // Once the scene and GameManager are done loading, send BeginEnterWorld
            PacketSender.BeginEnterWorld(characterId);
            
            // Await for all the packets needed to be received, ReadyToShowMain will be set to true once EndEnterWorld is received
            var beginTime = Time.realtimeSinceStartup;
            while (!ReadyToShowMain)
            {
                loadingSlider.value += 1;
                await Task.Delay(100);
                
                if ((Time.realtimeSinceStartup - beginTime) > 10f)
                {
                    #if !AO_DEBUG || !UNITY_EDITOR
                    // If it's been more than 10 seconds and the server has yet to send EndEnterWorld, disconnect the client
                    Client.Instance.Disconnect();
                    return;
                    #else
                    break;
                    #endif
                }
            }

            loadingSlider.value = 100;
            // Finally hide the loading canvas
            loadingScreenCanvas.SetActive(false);
        }
    }
}