using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskMaze.MazeSolver
{
    public interface IMazeSolver
    {
        IEnumerator SolveMaze(IMazeManager mazeManager, Vector2Int startingPosition, Action<List<Tile>> onSolutionFound);
    }
}