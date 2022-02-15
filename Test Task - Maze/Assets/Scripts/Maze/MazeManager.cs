using System.Collections;
using System.Collections.Generic;
using RFTestTaskMaze.MazeSolver;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RFTestTaskMaze
{
    public class MazeManager : MonoBehaviour, IMazeManager
    {
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private Transform _subfieldHighlight;

        [SerializeField] private int _mazeWidth = 100;
        [SerializeField] private int _mazeHeight = 100;

        private Transform _transform;
        private Tile[] _tileMap;

        private IMazeGenerator _mazeGenerator;
        private IMazeSolver _mazeSolver;
        private ICameraManager _cameraManager;
        
        private bool _isMazeGenerated;
        private bool _isMazeBeingSolved;

        private List<Tile> _exits = new List<Tile>();
        private List<Tile> _affectedTiles = new List<Tile>();
        private Tile _startingTile;

        #region Unity Lifecycle

        private void Awake()
        {
            _transform = transform;

            _cameraManager = Services.Get<ICameraManager>();
            _cameraManager.Initialize(_mazeHeight / 3f, _mazeHeight /2f, new Rect(0, 0, _mazeWidth, _mazeHeight));
            
            Services.Register(this);
        }

        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        #endregion

        #region IMazeManager Implementation
        
        public int Width => _mazeWidth;
        public int Height => _mazeHeight;

        public void SetMazeGenerator(IMazeGenerator generator)
        {
            _mazeGenerator = generator;
        }

        public void SetMazeSolver(IMazeSolver solver)
        {
            _mazeSolver = solver;
        }

        public void CreateMaze()
        {
            StartCoroutine(GenerateMaze());
        }

        public void SolveMaze()
        {
            int startingX = Random.Range(0, _mazeWidth);
            int startingY = Random.Range(0, _mazeHeight);
            SolveMaze(new Vector2Int(startingX, startingY));
        }

        public void SolveMaze(Vector2Int startingPosition)
        {
            if (!_isMazeGenerated)
            {
                Debug.LogWarningFormat("Cannot solve maze. Maze not fully generated yet.");
                return;
            }
            
            if (_isMazeBeingSolved)
            {
                Debug.LogWarningFormat("Cannot begin solving maze until the previous solver is done.");
                return;
            }

            if (_exits.Count == 0)
            {
                _exits = CreateExits();
            }

            if (!TryGetTile(startingPosition, out Tile clickedTile))
            {
                Debug.LogWarningFormat("No grid tile at position {0}", startingPosition);   
                return;
            }

            _startingTile = clickedTile;
            _startingTile.ToggleSelected(true);
            StartCoroutine(RunMazeSolver(startingPosition));
        }

        public void ResetSolution()
        {
            if (!_isMazeGenerated)
            {
                Debug.LogWarningFormat("Cannot reset solution. Maze not fully generated yet.");
                return;
            }
            
            if (_isMazeBeingSolved)
            {
                Debug.LogWarningFormat("Cannot reset non-existing solution.");
                return;
            }

            foreach (Tile tile in _affectedTiles)
            {
                tile.ToggleSelected(false);
            }
            _affectedTiles.Clear();

            foreach (Tile exit in _exits)
            {
                exit.PatchExitHole();
                exit.ToggleSelected(false);
            }
            _exits.Clear();
            
            if (_startingTile != null)
            {
                _startingTile.ToggleSelected(false);
                _startingTile = null;
            }
        }
        
        public void SpecifyExit(Vector2Int exitPosition)
        {
            if (!_isMazeGenerated)
            {
                Debug.LogWarningFormat("Cannot specify exit. Maze not fully generated yet.");
                return;
            }
            
            if (_isMazeBeingSolved)
            {
                Debug.LogWarningFormat("Cannot specify exit while solver under way.");
                return;
            }
            
            if (!TryMarkExit(exitPosition, out Tile clickedTile)) return;

            _exits.Add(clickedTile);
        }

        public bool TryGetTile(int x, int y, out Tile tile)
        {
            tile = null;
            
            if (x < 0 || x >= _mazeWidth) return false;
            if (y < 0 || y >= _mazeHeight) return false;
            
            tile = _tileMap[y * _mazeWidth + x];
            return true;
        }

        public bool TryGetTile(Vector2Int position, out Tile tile) => TryGetTile(position.x, position.y, out tile);

        public void HighlightSubfield(RectInt subfield)
        {
            _subfieldHighlight.position = new Vector3(subfield.x, subfield.y, 1f);
            _subfieldHighlight.localScale = new Vector3(subfield.width, subfield.height, 1f);
        }
        
        #endregion

        #region Maze Generation 

        private IEnumerator GenerateMaze()
        {
            _isMazeGenerated = false;
            _tileMap = new Tile[_mazeWidth * _mazeHeight];

            _cameraManager.Recenter();

            // create grid and prepare outer wall
            yield return CreateTileGrid();
            yield return CreateOuterWalls();

            // generate the meat of the maze 
            yield return _mazeGenerator.GenerateMaze(this, _mazeWidth, _mazeHeight);
            _isMazeGenerated = true;
        }

        public IEnumerator CreateTileGrid()
        {
            for (int y = 0; y < _mazeHeight; y++)
            {
                for (int x = 0; x < _mazeWidth; x++)
                {
                    Tile tile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity, _transform);
                    _tileMap[y * _mazeWidth + x] = tile;
                    tile.Initialize(x, y);
                }

                yield return null;
            }
        }

        private IEnumerator CreateOuterWalls()
        {
            for (int x = 0; x < _mazeWidth; x++)
            {
                if (TryGetTile(x, 0, out Tile bottomEdgeTile))
                {
                    bottomEdgeTile.BuildWall(Direction.Down);
                }

                if (TryGetTile(x, _mazeHeight - 1, out Tile topEdgeTile))
                {
                    topEdgeTile.BuildWall(Direction.Up);
                }
            }
            yield return null;
                    
            for (int y = 0; y < _mazeHeight; y++)
            {
                if (TryGetTile(0, y, out Tile leftEdgeTile))
                {
                    leftEdgeTile.BuildWall(Direction.Left);
                }

                if (TryGetTile(_mazeWidth - 1, y, out Tile rightEdgeTile))
                {
                    rightEdgeTile.BuildWall(Direction.Right);
                }
            }
            yield return null;
        }

        #endregion

        #region Solving Preparation

        private IEnumerator RunMazeSolver(Vector2Int startingPos)
        {
            _isMazeBeingSolved = true;
            yield return _mazeSolver.SolveMaze(this, startingPos, OnMazeSolutionFound);
            _isMazeBeingSolved = false;
        }
        
        private void OnMazeSolutionFound(List<Tile> affectedTiles)
        {
            _affectedTiles = affectedTiles;
        }

        private List<Tile> CreateExits()
        {
            int bottomX = Random.Range(0, Width);
            int topX = Random.Range(0, Width);
            int leftY = Random.Range(0, Height);
            int rightY = Random.Range(0, Height);

            TryMarkExit(new Vector2Int(bottomX, 0), out Tile bottomExit);
            TryMarkExit(new Vector2Int(topX, Height - 1), out Tile topExit);
            TryMarkExit(new Vector2Int(0, leftY), out Tile leftExit);
            TryMarkExit(new Vector2Int(Width - 1, rightY), out Tile rightExit);

            return new List<Tile> {bottomExit, topExit, leftExit, rightExit};
        }
        
        private bool TryMarkExit(Vector2Int exitPosition, out Tile exit)
        {
            if (!TryGetTile(exitPosition, out exit))
            {
                Debug.LogWarningFormat("No grid tile at position {0}", exitPosition);   
                return false;
            }

            bool edgeExit = true;
            Direction exitDirection = Direction.Right;
            Color exitColor = Color.magenta;

            if (exitPosition.x == 0) exitDirection = Direction.Left;
            else if (exitPosition.y == 0) exitDirection = Direction.Down;
            else if (exitPosition.x == Width - 1) exitDirection = Direction.Right;
            else if (exitPosition.y == Height - 1) exitDirection = Direction.Up;
            else edgeExit = false;

            if (edgeExit)
            {
                exit.BreakDownWall(exitDirection, true);
            }
            else
            {
                exit.MarkExit();
            }
            
            exit.ChangeSelectionColor(exitColor);
            exit.ToggleSelected(true);

            return true;
        }

        #endregion        

    }
}