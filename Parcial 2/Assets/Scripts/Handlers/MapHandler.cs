using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Entities.Food;
using System;
using Utils.Constants;

namespace Handlers.Map
{
    public class Cell
    {
        private Vector2Int position;
        public Vector2Int Position => position;

        private Vector2Int size;
        public Vector2Int Size => size;

        private List<Vector2Int> adjacents;
        public List<Vector2Int> Adjacents => adjacents;

        private Food foodInCell;
        public Food FoodInCell => foodInCell;

        public Cell(Vector2Int position, Vector2Int size)
        {
            this.position = position;
            this.size = size;
            adjacents = new List<Vector2Int>();
        }

        public void AddAdjacents(Vector2Int[] adjacents)
        {
            this.adjacents.AddRange(adjacents);
        }

        public void SetFoodInCell(Food food)
        {
            foodInCell = food;
        }

        public bool ValidateFoodInCell()
        {
            return foodInCell != null;
        }
    }

    public class MapHandler : MonoBehaviour
    {
        [Range(100, 500), Tooltip("Max Size Of The Grid")] public Vector2Int gridSize;
        [Tooltip("Physical Size of Cells on the Map")] public Vector2Int cellSize = default;

        public float offsetPerCell = 0;
        public GameObject physicalCellPrefab = null;
        public Transform parentForCells;

        private Cell middleCell = null;
        private Camera mainCamera = null;

        /// <summary>
        /// Stores all positions with their associated Cells
        /// </summary>
        private Dictionary<Vector2Int, Cell> map = null;
        public Dictionary<Vector2Int, Cell> Map => map;

        public void Init()
        {
            mainCamera = Camera.main;
            map = new Dictionary<Vector2Int, Cell>();
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            for(int x = 0; x < gridSize.x; x++)
            {
                for(int y =0; y < gridSize.y; y++)
                {
                    Vector2Int cellPosition = new Vector2Int((int)(x * (cellSize.x + offsetPerCell)),
                        (int)(y * (cellSize.y + offsetPerCell)));

                    Cell gridCell=new Cell(cellPosition, cellSize);

                    if (!map.ContainsKey(cellPosition))
                        map.Add(cellPosition, gridCell);

                    Instantiate(physicalCellPrefab, new Vector2(cellPosition.x, cellPosition.y), Quaternion.identity, parentForCells);
                }
            }

            ConfigureCamera();
        }

        private void ConfigureCamera()
        {
            Vector2Int middleMap = GetMiddleOfMap();
            Vector2 finalCameraPosition = new Vector2(middleMap.x + cellSize.x, middleMap.y + cellSize.y);

            mainCamera.transform.position = new Vector3(finalCameraPosition.x, finalCameraPosition.y, mainCamera.transform.position.z);
            mainCamera.orthographicSize = gridSize.x;

            if (mainCamera.TryGetComponent(out Handlers.Cam.CameraHandler cameraHandler))
            {
                cameraHandler.Initialize();
            }
        }

        private Vector2Int GetMiddleOfMap()
        {
            foreach (Cell cell in map.Values)
            {
                if (cell.Position.x == (int)(((gridSize.x + gridSize.y) * 0.5f)) &&
                    cell.Position.y == (int)(((gridSize.x + gridSize.y) * 0.5f)))
                {
                    middleCell = cell;
                    return cell.Position;
                }
            }

            return new Vector2Int(-1, -1);
        }

        public Cell GetCellByPosition(Vector2Int cellPosition)
        {
            if (!map.ContainsKey(cellPosition))
            {
                Debug.LogError("INVALID CELL POSITION: Check GetCellByPosition() Usage");
                return null;
            }

            return map[cellPosition];
        }

        public void SetGeneratedFoodOnCells(List<Food> allFoodInMap)
        {
            for(int i = 0; i < allFoodInMap.Count; i++)
            {
                if (allFoodInMap[i] != null)
                {
                    if (map.ContainsKey(allFoodInMap[i].Position))
                        map[allFoodInMap[i].Position].SetFoodInCell(allFoodInMap[i]);
                }
            }
        }

        public void ClearFoodOnCells()
        {
            foreach(Cell cell in map.Values)
            {
                if (cell != null)
                {
                    if (cell.FoodInCell != null)
                        cell.SetFoodInCell(null);
                }
            }
        }

        public List<Vector2Int> GetAllMapPositions()
        {
            List<Vector2Int> listOfMapPositions = new List<Vector2Int>();

            foreach (Cell cell in map.Values)
            {
                listOfMapPositions.Add(cell.Position);
            }

            return listOfMapPositions;
        }

        public List<Vector2Int> GetRandomUniquePositions(int initialPopulationSize)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            if (initialPopulationSize <= (gridSize.x * 3)) //Por dos ya que al ser dos equipos cada uno puede ocupar 100 celdas de arriba para abajo.
            {
                for (int i = 0; i < initialPopulationSize; i++)
                {
                    Vector2Int newPosition = Constants.InvalidPosition;
                    do
                    {
                        newPosition = GetRandomPosition();
                    } while (result.Contains(newPosition));

                    result.Add(newPosition);
                }

                result = result.Distinct().ToList();
            }
            else
            {
                Debug.Log("The population is exeding the grid size of the map.");
                return null;
            }

            return result;
        }

        public Vector2Int GetRandomPosition()
        {
            return map[new Vector2Int(UnityEngine.Random.Range(0,gridSize.x), UnityEngine.Random.Range(0,gridSize.y))].Position;
        }

        public List<Cell> GetLeftToRightCellsInColumn(int column)
        {
            List<Cell> listOfCells = new List<Cell>();

            foreach(Cell cell in map.Values) 
            { 
                if(cell.Position != Constants.InvalidPosition)
                {
                    if (cell.Position.y == column)
                    {
                        listOfCells.Add(cell);
                    }
                }
            }

            return listOfCells; 
        }

        public List<Cell> GetRightToLefttCellsInColumn(int column)
        {
            List<Cell> listOfCells = new List<Cell>();

            foreach (Cell cell in map.Values)
            {
                if (cell.Position != Constants.InvalidPosition)
                {
                    if (cell.Position.y == column)
                    {
                        listOfCells.Add(cell);
                    }
                }
            }

            List<Cell> sortedList = new List<Cell>();
            for (int i = listOfCells.Count - 1; i > 0; i--)
            {
                sortedList.Add(listOfCells[i]);
            }
            return sortedList;
        }

        public List<Cell> GetAdjacentCellsToPositions(Vector2Int cellPosition)
        {
            List<Cell> adjacents = new List<Cell>();

            Vector2Int upPosition = new Vector2Int(cellPosition.x + 1, cellPosition.y);
            Vector2Int downPosition = new Vector2Int(cellPosition.x - 1, cellPosition.y);
            Vector2Int leftPosition = new Vector2Int(cellPosition.x, cellPosition.y - 1);
            Vector2Int rightPosition = new Vector2Int(cellPosition.x, cellPosition.y + 1);

            if (map.ContainsKey(upPosition))
            {
                adjacents.Add(map[upPosition]);
            }
            if (map.ContainsKey(downPosition))
            {
                adjacents.Add(map[downPosition]);
            }
            if (map.ContainsKey(leftPosition))
            {
                adjacents.Add(map[leftPosition]);
            }
            if (map.ContainsKey(rightPosition))
            {
                adjacents.Add(map[rightPosition]);
            }

            return adjacents;
        }
    }
}


