using UnityEngine;

namespace RFTestTaskMaze
{
    public class AppController : MonoBehaviour
    {
        private static AppController _instance = null;

        public static AppController Instance
        {
            get
            {
                if (_instance == null)
                {
                    var instance = FindObjectOfType<AppController>();
                    if (instance == null)
                    {
                        GameObject appControllerGObj = new GameObject("AppController");
                        instance = appControllerGObj.AddComponent<AppController>();
                        instance.Initialize();
                        DontDestroyOnLoad(appControllerGObj);

                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                _instance.Initialize();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private GameController _gameController;

        private void Initialize()
        {
            _gameController = new GameController(); 
        }

        private void Start()
        {
            _gameController.StartGame();
        }

        private void Update()
        {
            _gameController.Update();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _gameController.PauseGame();
            }
            else
            {
                _gameController.ResumeGame();
            }
            
        }

        private void OnApplicationQuit()
        {
            _gameController.QuitGame();
        }
    }
}