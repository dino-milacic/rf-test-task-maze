using System;
using UnityEngine;

namespace RFTestTaskMaze
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _wallRenderer;
        [SerializeField] private Sprite[] _wallSprites;
        [SerializeField] private Color _selectedColor = Color.green;

        [SerializeField] private bool[] _walls = {false, false, false, false};
        
        private Vector2Int _position;
        public Vector2Int Position => _position;
        
        private void Awake()
        {
            DetermineWallSpriteType();
        }

        public void Initialize(int x, int y)
        {
            _position = new Vector2Int(x, y);
            name = $"Tile-[{x}, {y}]";
        }

        public void ToggleSelected(bool isSelected, bool overwrite = true)
        {
            if (!overwrite && _wallRenderer.color != Color.white) return;
            
            _wallRenderer.color = isSelected? _selectedColor : Color.white;
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

        public void BreakDownWall(Direction edge)
        {
            _walls[(int) edge] = false;
            DetermineWallSpriteType();
        }

        private void DetermineWallSpriteType()
        {
            int right = _walls[0].ToInt();
            int up    = _walls[1].ToInt();
            int left  = _walls[2].ToInt();
            int down  = _walls[3].ToInt();

            int wallTypeFlag = right | up << 1 | left << 2 | down << 3;
            if (((WallTypes)wallTypeFlag).ToString() != _wallSprites[wallTypeFlag].name)
            {
                Debug.LogWarningFormat("{0} -> {1} [{2}]", Convert.ToString(wallTypeFlag, 2).PadLeft(4, '0'), (WallTypes) wallTypeFlag, _wallSprites[wallTypeFlag].name);
            }
            _wallRenderer.sprite = _wallSprites[wallTypeFlag];
        }

        public override string ToString() => $"[T({Position.x}, {Position.y})]";

        public bool CanMoveToward(Direction direction)
        {
            return !_walls[(int) direction];
        }
    }
}