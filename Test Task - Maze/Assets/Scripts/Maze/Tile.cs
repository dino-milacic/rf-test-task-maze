using UnityEngine;

namespace RFTestTaskMaze
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _wallRenderer;
        [SerializeField] private Color _selectedColor = Color.green;
        [SerializeField] private Color _neutralColor = Color.white;
        
        // TODO; perhaps move to outside location (scriptable object?) to avoid same references in every instance  
        [SerializeField] private Sprite[] _wallSprites;

        private bool[] _walls = {false, false, false, false};
        private Direction _exitDirection;
        
        private Vector2Int _position;
        public Vector2Int Position => _position;

        private bool _isExit;
        public bool IsExit => _isExit;

        private void Awake()
        {
            DetermineWallSpriteType();
        }

        public void Initialize(int x, int y)
        {
            _position = new Vector2Int(x, y);
            name = $"Tile-[{x}, {y}]";
        }

        public bool CanMoveToward(Direction direction)
        {
            return !_walls[(int) direction];
        }

        public void ToggleSelected(bool isSelected, bool overwrite = true)
        {
            if (!overwrite && _wallRenderer.color != _neutralColor) return;
            
            _wallRenderer.color = isSelected? _selectedColor : _neutralColor;
        }

        public void ChangeSelectionColor(Color newColor)
        {
            _selectedColor = newColor;
        }

        public void BuildWall(Direction edge)
        {
            _walls[(int) edge] = true;
            DetermineWallSpriteType();
        }

        public void BreakDownWall(Direction edge, bool isExit = false)
        {
            _walls[(int) edge] = false;
            DetermineWallSpriteType();

            if (isExit)
            {
                _isExit = true;
                _exitDirection = edge;
            }
        }

        public void MarkExit()
        {
            _isExit = true;
        }

        public void PatchExitHole()
        {
            if (!_isExit) return;

            _isExit = false;
            BuildWall(_exitDirection);
        }

        private void DetermineWallSpriteType()
        {
            int right = _walls[0].ToInt();
            int up    = _walls[1].ToInt();
            int left  = _walls[2].ToInt();
            int down  = _walls[3].ToInt();

            // wall sprite index is determined in binary by |D|L|U|P| where a 1 means there's a wall at that edge
            int wallTypeFlag = right | up << 1 | left << 2 | down << 3;
            _wallRenderer.sprite = _wallSprites[wallTypeFlag];
        }

        public override string ToString() => $"[T({Position.x}, {Position.y})]";
    }
}