using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RFTestTaskMaze
{
    public static class HelperExtensions
    {
        public static int ToInt(this bool b) => b ? 1 : 0;

        public static Vector2Int ToDirectionVector(this Direction direction)
        {
            switch (direction)
            {
                default:
                case Direction.Right: return Vector2Int.right;
                case Direction.Up:    return Vector2Int.up;
                case Direction.Left:  return Vector2Int.left;
                case Direction.Down:  return Vector2Int.down;
            }
        }

        public static Direction OppositeDirection(this Direction direction)
        {
            return (Direction) (((int)direction + 2) % 4);
        }
    }
}