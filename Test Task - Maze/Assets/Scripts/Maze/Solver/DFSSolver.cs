using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RFTestTaskMaze.MazeSolver
{
    public class DFSSolver : IMazeSolver
    {
        private IMazeManager _mazeMan;
        private ICameraManager _cameraMan;
        private Action<List<Tile>> _onSolutionFound;
        
        public IEnumerator SolveMaze(IMazeManager mazeManager, Vector2Int startingPosition, Action<List<Tile>> onSolutionFound)
        {
            _mazeMan = mazeManager;
            _cameraMan = Services.Get<ICameraManager>();
            _onSolutionFound = onSolutionFound;

            Stack<Tile> tileStack = new Stack<Tile>();
            Vector2Int position = startingPosition;

            Dictionary<Vector2Int, Tile> visited = new Dictionary<Vector2Int, Tile>();
            Dictionary<Vector2Int, Tile> lookedAt = new Dictionary<Vector2Int, Tile>();
            
            Debug.LogFormat("START @ {0}", startingPosition);

            if (_mazeMan.TryGetTile(position, out Tile startTile))
            {
                tileStack.Push(startTile);
            }

            bool exitFound = false;
            while (tileStack.Count > 0 && !exitFound)
            {
                Tile tile = tileStack.Pop();
            
                _cameraMan.SetTarget(tile.Position);
                
                if (!visited.ContainsKey(tile.Position))
                {
                    if (tile.IsExit)
                    {
                        OnSolved(visited, lookedAt, tile);
                        break;
                    }
                    
                    if (tile != startTile) tile.ChangeSelectionColor(Color.red);
                    tile.ToggleSelected(true);

                    visited[tile.Position] = tile;

                    int turnDirection = Random.Range(0, 2) == 1 ? 1 : -1;
                    for (int i = 0; i < 4; i++)
                    {
                        Direction dir = (Direction) ((i * turnDirection + 4) % 4);
                        
                        if (!tile.CanMoveToward(dir)) continue;
                        if (!_mazeMan.TryGetTile(tile.Position + dir.ToDirectionVector(), out Tile neighbor)) continue;
                        if (visited.ContainsKey(neighbor.Position)) continue;

                        // check down the corridor if exit is visible from current position
                        Tile n = neighbor;
                        while (n.CanMoveToward(dir))
                        {
                            yield return null;
                            if (n.IsExit)
                            {
                                OnSolved(visited, lookedAt, n);
                                exitFound = true;
                                break;
                            }
                            
                            if (n != startTile) n.ChangeSelectionColor(new Color(1f, 0.74f, 0f));
                            n.ToggleSelected(true, false);
                        
                            lookedAt[n.Position] = n;
                            
                            if (!_mazeMan.TryGetTile(n.Position + dir.ToDirectionVector(), out n) || lookedAt.ContainsKey(n.Position)) break;
                        }
                        if (exitFound) break;
                        
                        tileStack.Push(neighbor);
                    }
                }
                yield return null;
            }
        }

        private void OnSolved(Dictionary<Vector2Int, Tile> visited, Dictionary<Vector2Int, Tile> lookedAt, Tile tile)
        {
            _cameraMan.Recenter();
            
            List<Tile> affectedTiles = visited.Select(x => x.Value).ToList();
            affectedTiles.AddRange(lookedAt.Where(x => !affectedTiles.Contains(x.Value)).Select(x => x.Value).ToList());
            
            float solutionPercent = affectedTiles.Count / (float) (_mazeMan.Width * _mazeMan.Height);
            Debug.LogFormat("DONE! {0} found exit @ {1} [solution covered {2:F2}% of maze]", GetType().Name, tile, solutionPercent * 100);
            
            _onSolutionFound?.Invoke(affectedTiles);
        }
    }
}