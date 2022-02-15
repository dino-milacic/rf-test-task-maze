using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskMaze
{
    public class RecursiveDivisionGenerator : IMazeGenerator
    {
        private int _verticalCounter;
        private int _horizontalCounter;

        private IGridManager _gridManager;

        public IEnumerator GenerateMaze(IGridManager gridManager, int width, int height)
        {
            _gridManager = gridManager;
            
            _verticalCounter = 0;
            _horizontalCounter = 0;
            
            RectInt field = new RectInt(0, 0, width, height);
            
            yield return SubdivideMaze(GetRandomCutAxis(field), field);
            _gridManager.HighlightSubfield(new RectInt());
            CreateExits(width, height);
        }

        private void CreateExits(int width, int height)
        {
            int bottomX = Random.Range(0, width);
            int topX = Random.Range(0, width);
            int leftY = Random.Range(0, height);
            int rightY = Random.Range(0, height);

            if (_gridManager.TryGetTile(bottomX, 0, out Tile bottomExit))
            {
                bottomExit.BreakDownWall(Direction.Down);
                bottomExit.ChangeSelectionColor(Color.magenta);
                bottomExit.ToggleSelected(true);
            }

            if (_gridManager.TryGetTile(topX, height - 1, out Tile topExit))
            {
                topExit.BreakDownWall(Direction.Up);
                topExit.ChangeSelectionColor(Color.magenta);
                topExit.ToggleSelected(true);
            }

            if (_gridManager.TryGetTile(0, leftY, out Tile leftExit))
            {
                leftExit.BreakDownWall(Direction.Left);
                leftExit.ChangeSelectionColor(Color.magenta);
                leftExit.ToggleSelected(true);
            }

            if (_gridManager.TryGetTile(width - 1, rightY, out Tile rightExit))
            {
                rightExit.BreakDownWall(Direction.Right);
                rightExit.ChangeSelectionColor(Color.magenta);
                rightExit.ToggleSelected(true);
            }
        }

        private IEnumerator SubdivideMaze(CutAxis cutAxis, RectInt subfield)
        {
            bool isSubdivisionVertical = cutAxis == CutAxis.Vertical;
            if (subfield.width > 5 || subfield.height > 5) _gridManager.HighlightSubfield(subfield);
            
            if (subfield.width < 2 || subfield.height < 2)
            {
                // _gridManager.HighlightSubfield(new RectInt()); // effectively removes highlight
                yield break;
            }
            
            _gridManager.SetCameraTarget(subfield.center);
            _gridManager.SetCameraZoom(subfield.height / 2f);

            if (isSubdivisionVertical) // cutting the field vertically into a upper and lower subfield
            {
                // decide the cut row
                int min = Mathf.Max(subfield.height / 4, 1);
                int cutRow = Mathf.Min(min + Random.Range(0, subfield.height / 2), subfield.height - 1);

                CutFieldAtRow(cutRow, subfield);
                
                // recursively subdivide upper and lower subfields
                
                // 1) lower subfield
                RectInt lowerSubfield = new RectInt();
                lowerSubfield.x = subfield.x;
                lowerSubfield.y = subfield.y;
                lowerSubfield.width = subfield.width;
                lowerSubfield.height = cutRow;
                yield return SubdivideMaze(GetRandomCutAxis(lowerSubfield), lowerSubfield);
                
                // 2) upper subfield
                RectInt upperSubfield = new RectInt();
                upperSubfield.x = subfield.x;
                upperSubfield.y = subfield.y + cutRow;
                upperSubfield.width = subfield.width;
                upperSubfield.height = Mathf.Abs(subfield.height - cutRow);
                
                yield return SubdivideMaze(GetRandomCutAxis(upperSubfield), upperSubfield);
            }
            else // cutting the field horizontally into a left and right subfield
            {
                int min = Mathf.Max(subfield.width / 4, 1);
                int cutColumn = Mathf.Min(min + Random.Range(0, subfield.width / 2), subfield.width - 1);

                CutFieldAtColumn(cutColumn, subfield);
                
                // recursively subdivide left and right subfields
                
                // 1) left subfield
                RectInt leftSubfield = new RectInt();
                leftSubfield.x = subfield.x;
                leftSubfield.y = subfield.y;
                leftSubfield.width = cutColumn;
                leftSubfield.height = subfield.height;
                yield return SubdivideMaze(GetRandomCutAxis(leftSubfield), leftSubfield);
                
                // 2) right subfield
                RectInt rightSubfield = new RectInt();
                rightSubfield.x = subfield.x + cutColumn;
                rightSubfield.y = subfield.y;
                rightSubfield.width = Mathf.Abs(subfield.width - cutColumn);
                rightSubfield.height = subfield.height;
                yield return SubdivideMaze(GetRandomCutAxis(rightSubfield), rightSubfield);
                
            }
        }
        
        private void CutFieldAtRow(int cutRow, RectInt field)
        {
            Direction direction = Direction.Down;

            List<int> doorColumns = new List<int>();
            int numberOfDoors = Random.Range(Mathf.Max(1, field.width / 25), field.width / 10);
            for (int i = 0; i < numberOfDoors; i++)
            {
                int doorColumn = Random.Range(field.x, field.max.x);
                doorColumns.Add(doorColumn);
            }
            
            for (int x = field.x; x < field.max.x; x++)
            {
                if (doorColumns.Contains(x)) continue;
                
                if (_gridManager.TryGetTile(x, field.y + cutRow, out Tile tile))
                {
                    tile.ToggleSelected(true);
                    tile.BuildWall(direction);
                    if (_gridManager.TryGetTile(x + direction.ToDirectionVector().x, field.y + cutRow + direction.ToDirectionVector().y, out Tile neighbour))
                    {
                        neighbour.BuildWall(direction.OppositeDirection());
                    }
                }
                
                if (tile != null) tile.ToggleSelected(false);
            }
        }

        private void CutFieldAtColumn(int cutColumn, RectInt field)
        {
            Direction direction = Direction.Left;

            List<int> doorRows = new List<int>();
            int numberOfDoors = Random.Range(Mathf.Max(1, field.height / 25), field.height / 10);
            for (int i = 0; i < numberOfDoors; i++)
            {
                int doorRow = Random.Range(field.y, field.max.y);
                doorRows.Add(doorRow);
            }

            for (int y = field.y; y < field.max.y; y++)
            {
                if (doorRows.Contains(y)) continue;

                if (_gridManager.TryGetTile(field.x + cutColumn, y, out Tile tile))
                {
                    tile.ToggleSelected(true);
                    tile.BuildWall(direction);
                    if (_gridManager.TryGetTile(field.x + cutColumn + direction.ToDirectionVector().x,
                            y + direction.ToDirectionVector().y, out Tile neighbour))
                    {
                        neighbour.BuildWall(direction.OppositeDirection());
                    }
                }

                if (tile != null) tile.ToggleSelected(false);
            }
        }
        
        private CutAxis GetRandomCutAxis(RectInt subfield)
        {
            float maxCorridorRatio = 4;
            
            if (_horizontalCounter > 2 || subfield.height >= subfield.width * maxCorridorRatio)
            {
                _horizontalCounter = 0;
                return CutAxis.Vertical;
            }

            if (_verticalCounter > 2 || subfield.width >= subfield.height * maxCorridorRatio)
            {
                _verticalCounter = 0;
                return CutAxis.Horizontal;
            }
            
            CutAxis newAxis = Random.Range(0, 2) == 1 ? CutAxis.Vertical : CutAxis.Horizontal;
            if (newAxis == CutAxis.Vertical)
            {
                _verticalCounter++;
            }
            else
            {
                _horizontalCounter++;
            }

            return newAxis;
        }
    }
}