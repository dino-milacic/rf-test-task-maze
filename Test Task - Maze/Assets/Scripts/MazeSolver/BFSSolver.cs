using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskMaze.MazeSolver
{
    public class BFSSolver : IMazeSolver
    {
        private IGridManager _gridMan;
        
        public IEnumerator SolveMaze(IGridManager gridManager, Vector2Int startingPosition)
        {
            _gridMan = gridManager;

            Queue<Tile> tileStack = new Queue<Tile>();
            Vector2Int position = startingPosition;

            Dictionary<Vector2Int, Tile> visited = new Dictionary<Vector2Int, Tile>();
            Dictionary<Vector2Int, Tile> lookedAt = new Dictionary<Vector2Int, Tile>();
            
            Debug.LogFormat("START @ {0}", startingPosition);

            if (_gridMan.TryGetTile(position, out Tile startTile))
            {
                tileStack.Enqueue(startTile);
            }

            while (tileStack.Count > 0)
            {
                Tile tile = tileStack.Dequeue();
                
                if (!visited.ContainsKey(tile.Position))
                {
                    if (IsExit(tile))
                    {
                        OnSolved(visited, tile);
                        break;
                    }
                    tile.ChangeSelectionColor(Color.red);
                    tile.ToggleSelected(true);

                    visited[tile.Position] = tile;

                    int turnDirection = Random.Range(0, 2) == 1 ? 1 : -1;
                    
                    for (int i = 0; i < 4; i++)
                    {
                        Direction dir = (Direction) ((i * turnDirection + 4) % 4);
                        
                        if (!tile.CanMoveToward(dir)) continue;
                        if (!_gridMan.TryGetTile(tile.Position + dir.ToDirectionVector(), out Tile neighbor)) continue;
                        if (visited.ContainsKey(neighbor.Position)) continue;

                        tileStack.Enqueue(neighbor);
                    }
                }
                yield return null;
            }
            
            
            yield return null;
        }

        private void OnSolved(Dictionary<Vector2Int, Tile> visited, Tile tile)
        {
            float solutionPercent = visited.Count / (float) (_gridMan.Width * _gridMan.Height);
            Debug.LogFormat("DONE! {0} found exit @ {1} [solution covered {2:F2}% of maze]", GetType().Name, tile, solutionPercent * 100);
        }

        private bool IsExit(Tile tile)
        {
            return tile.Position.x == 0 && tile.CanMoveToward(Direction.Left)
                   || tile.Position.y == 0 && tile.CanMoveToward(Direction.Down)
                   || tile.Position.x == _gridMan.Width - 1 && tile.CanMoveToward(Direction.Right)
                   || tile.Position.y == _gridMan.Height - 1 && tile.CanMoveToward(Direction.Up);
        }
    }
}