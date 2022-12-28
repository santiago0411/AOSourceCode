using UnityEngine;
using UnityEngine.SceneManagement;
using AOClient.UI.Main;

namespace AOClient.UI
{
    public class UIManager : MonoBehaviour
    {
        public static LoginRegister LoginRegister => instance.loginRegister;
        public static CharacterScreen CharacterScreen => instance.characterScreen;
        public static CharacterCreation CharacterCreation => instance.characterCreation;
        public static GameUI GameUI => instance.gameUI;

        public static Texture2D CastCursor => instance.castCursor;

        private static UIManager instance;
        
        private Scene lastScene;
        private LoginRegister loginRegister;
        private CharacterScreen characterScreen;
        private CharacterCreation characterCreation;
        private GameUI gameUI;
        private Texture2D castCursor;
        
        [SerializeField] private GameUI gameUIPrefab;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else if (instance != this)
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

            var cursors = Resources.LoadAll<Texture2D>("Cursors");
            castCursor = cursors[0];
        }

        public static void ChangeCursor(Texture2D cursor)
        {
            if (cursor)
                Cursor.SetCursor(cursor, Vector2.one * 10, CursorMode.Auto);
            else
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene unityScene, LoadSceneMode loadMode)
        {
            switch (lastScene)
            {
                case Scene.Login when loginRegister is not null:
                    Destroy(loginRegister);
                    loginRegister = null;
                    break;
                case Scene.Register when loginRegister is not null:
                    Destroy(loginRegister);
                    loginRegister = null;
                    break;
                case Scene.CharacterScreen when characterScreen is not null:
                    Destroy(characterScreen);
                    characterScreen = null;
                    break;
                case Scene.CharacterCreation when characterCreation is not null:
                    Destroy(characterCreation);
                    characterCreation = null;
                    break;
                case Scene.Main when gameUI is not null:
                    Destroy(gameUI);
                    gameUI = null;
                    break;
            }

            switch (unityScene.buildIndex)
            {
                case (int)Scene.Login:
                    lastScene = Scene.Login;
                    loginRegister = FindObjectOfType<LoginRegister>();
                    break;
                case (int)Scene.Register:
                    lastScene = Scene.Register;
                    loginRegister = FindObjectOfType<LoginRegister>();
                    break;
                case (int)Scene.CharacterScreen:
                    lastScene = Scene.CharacterScreen;
                    characterScreen = FindObjectOfType<CharacterScreen>();
                    break;
                case (int)Scene.CharacterCreation:
                    lastScene = Scene.CharacterCreation;
                    characterCreation = FindObjectOfType<CharacterCreation>();
                    break;
                case (int)Scene.Main:
                case (int)Scene.Dev:
                    lastScene = Scene.Main;
                    gameUI = Instantiate(gameUIPrefab);
                    break;
            }
        }
    }
}
