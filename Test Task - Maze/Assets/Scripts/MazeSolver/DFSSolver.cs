using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskMaze.MazeSolver
{
    public class DFSSolver : IMazeSolver
    {
        private IGridManager _gridMan;
        
        public IEnumerator SolveMaze(IGridManager gridManager, Vector2Int startingPosition)
        {
            _gridMan = gridManager;

            Stack<Tile> tileStack = new Stack<Tile>();
            Vector2Int position = startingPosition;

            Dictionary<Vector2Int, Tile> visited = new Dictionary<Vector2Int, Tile>();
            Dictionary<Vector2Int, Tile> lookedAt = new Dictionary<Vector2Int, Tile>();
            
            Debug.LogFormat("START @ {0}", startingPosition);

            if (_gridMan.TryGetTile(position, out Tile startTile))
            {
                tileStack.Push(startTile);
            }

            bool exitFound = false;
            while (tileStack.Count > 0 && !exitFound)
            {
                Tile tile = tileStack.Pop();
            
                _gridMan.SetCameraTarget(tile.Position);
                
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

                        // check down the corridor if exit is visible
                        Tile n = neighbor;
                        while (n.CanMoveToward(dir))
                        {
                            yield return null;
                            if (IsExit(n))
                            {
                                OnSolved(visited, n);
                                exitFound = true;
                                break;
                            }
                            
                            n.ChangeSelectionColor(new Color(1f, 0.74f, 0f));
                            n.ToggleSelected(true, false);
                        
                            lookedAt[n.Position] = n;
                            
                            if (!_gridMan.TryGetTile(n.Position + dir.ToDirectionVector(), out n) || lookedAt.ContainsKey(n.Position)) break;
                        }
                        
                        if (exitFound) break;
                        
                        tileStack.Push(neighbor);
                    }
                }
                yield return null;
            }
            
            
            yield return null;
        }

        private void OnSolved(Dictionary<Vector2Int, Tile> visited, Tile tile)
        {
            _gridMan.SetCameraTarget(new Vector2(_gridMan.Width / 2f, _gridMan.Height / 2f));
            _gridMan.SetCameraZoom(_gridMan.Height / 2f);
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