using RFTestTaskMaze.MazeSolver;
using UnityEngine;

namespace RFTestTaskMaze
{
    public class GameController
    {
        private IMazeManager _mazeManager;
        private ICameraManager _cameraMan;

        public void StartGame()
        {
            _cameraMan = Services.Get<ICameraManager>();
            
            _mazeManager = Services.Get<IMazeManager>();
            _mazeManager.SetMazeGenerator(new RecursiveDivisionGenerator());
            _mazeManager.SetMazeSolver(new DFSSolver());
            
            _mazeManager.CreateMaze();
        }

        public void Update()
        {
            // TODO: should be handled by some input manager or menu
            
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 mouseWorldPosition = _cameraMan.Camera.ScreenToWorldPoint(Input.mousePosition);
                
                Debug.LogFormat("L Mouse clicked at {0} -> {1}", Input.mousePosition, mouseWorldPosition);

                Vector2Int startPosition = new Vector2Int(Mathf.FloorToInt(mouseWorldPosition.x),
                    Mathf.FloorToInt(mouseWorldPosition.y));
                
                _mazeManager.SolveMaze(startPosition);
            }
            
            if (Input.GetMouseButtonUp(1))
            {
                Vector2 mouseWorldPosition = _cameraMan.Camera.ScreenToWorldPoint(Input.mousePosition);
                
                Debug.LogFormat("R Mouse clicked at {0} -> {1}", Input.mousePosition, mouseWorldPosition);

                Vector2Int exitPosition = new Vector2Int(Mathf.FloorToInt(mouseWorldPosition.x),
                    Mathf.FloorToInt(mouseWorldPosition.y));
                
                _mazeManager.SpecifyExit(exitPosition);
            }
            
            if (Input.GetKeyUp(KeyCode.Backspace))
            {
                _mazeManager.ResetSolution();
            }
            
            if (Input.GetKeyUp(KeyCode.Return))
            {
                _mazeManager.SolveMaze();
            }
        }

        public void PauseGame()
        {
            
        }

        public void ResumeGame()
        {
            
        }

        public void QuitGame()
        {
            
        }
    }
}