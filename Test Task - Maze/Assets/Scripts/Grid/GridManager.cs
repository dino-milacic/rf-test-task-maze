using System;
using System.Collections;
using RFTestTaskMaze.MazeSolver;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RFTestTaskMaze
{
    public class GridManager : MonoBehaviour, IGridManager
    {
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _subfieldHighlight;

        [SerializeField] public int MazeWidth = 100;
        [SerializeField] public int MazeHeight = 100;

        private Transform _transform;
        private Tile[] _tileMap;

        private void Awake()
        {
            _transform = transform;

            _cameraTransform.position = new Vector3(MazeWidth / 2f, MazeHeight / 2f, _cameraTransform.position.z);
            _camera.orthographicSize = MazeHeight / 2f + 1;
            _cameraZoom = MazeHeight / 2f + 1;
            _cameraTarget = _cameraTransform.position;

            StartCoroutine(GenerateMaze());
        }

        public int Width => MazeWidth;
        public int Height => MazeHeight;

        public bool TryGetTile(int x, int y, out Tile tile)
        {
            tile = null;
            
            if (x < 0 || x >= MazeWidth) return false;
            if (y < 0 || y >= MazeHeight) return false;
            
            tile = _tileMap[y * MazeWidth + x];
            return true;
        }

        public bool TryGetTile(Vector2Int position, out Tile tile) => TryGetTile(position.x, position.y, out tile);

        public void HighlightSubfield(RectInt subfield)
        {
            _subfieldHighlight.position = new Vector3(subfield.x, subfield.y, 1f);
            _subfieldHighlight.localScale = new Vector3(subfield.width, subfield.height, 1f);
        }

        private Vector3 _cameraTarget;
        private float _cameraZoom;

        public void SetCameraTarget(Vector2 targetPos)
        {
            _cameraTarget = new Vector3(targetPos.x, targetPos.y, _cameraTransform.position.z);
        }

        public void SetCameraZoom(float zoom)
        {
            _cameraZoom = zoom;
        }

        private void Update()
        {
            var position = _cameraTransform.position;
            Vector3 posDelta = (_cameraTarget - position) * Time.deltaTime;
            position += posDelta;
            _cameraTransform.position = position;

            var newZoom = Mathf.Clamp(_cameraZoom, 30, MazeHeight / 2f);
            var orthographicSize = _camera.orthographicSize;
            float zoomDelta = newZoom - orthographicSize;
            orthographicSize += zoomDelta * Time.deltaTime;
            _camera.orthographicSize = orthographicSize;
        }

        private IEnumerator GenerateMaze()
        {
            _tileMap = new Tile[MazeWidth * MazeHeight];

            yield return CreateTileGrid();
            yield return CreateOuterWalls();

            IMazeGenerator generator = new RecursiveDivisionGenerator();
            yield return generator.GenerateMaze(this, MazeWidth, MazeHeight);
            
            
            IMazeSolver solver = new DFSSolver();
            int startingX = Random.Range(0, MazeWidth);
            int startingY = Random.Range(0, MazeHeight);
            yield return solver.SolveMaze(this, new Vector2Int(startingX, startingY));
        }

        public IEnumerator CreateTileGrid()
        {
            for (int y = 0; y < MazeHeight; y++)
            {
                for (int x = 0; x < MazeWidth; x++)
                {
                    Tile tile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity, _transform);
                    _tileMap[y * MazeWidth + x] = tile;
                    tile.Initialize(x, y);
                }

                yield return null;
            }
        }

        private IEnumerator CreateOuterWalls()
        {
            for (int x = 0; x < MazeWidth; x++)
            {
                if (TryGetTile(x, 0, out Tile bottomEdgeTile))
                {
                    bottomEdgeTile.BuildWall(Direction.Down);
                }

                if (TryGetTile(x, MazeHeight - 1, out Tile topEdgeTile))
                {
                    topEdgeTile.BuildWall(Direction.Up);
                }
            }
            yield return null;
            
            for (int y = 0; y < MazeHeight; y++)
            {
                if (TryGetTile(0, y, out Tile leftEdgeTile))
                {
                    leftEdgeTile.BuildWall(Direction.Left);
                }

                if (TryGetTile(MazeWidth - 1, y, out Tile rightEdgeTile))
                {
                    rightEdgeTile.BuildWall(Direction.Right);
                }
            }
            yield return null;
        }

        

        
    }
}