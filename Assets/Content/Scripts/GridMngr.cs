using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMngr : Singleton<GridMngr>
{
    [Header("DON'T CONSIDER BORDERS")]
    [SerializeField] private int numMaxCols; //12
    [SerializeField] private int numMaxRows; //16

    [Header("")]
    [SerializeField] private float offsetNextObjectPosition; //0.5
    [SerializeField] private GameObject gridDrawCell;
    [SerializeField] private Transform cellsContainer;
    [SerializeField] private Transform gridDrawContainer;

    public Dictionary<string, Tuple<int, int>> cellIndexesAt { get; private set; }
    public bool[,] cellsOccupied { get; private set; }
    public Transform[,] cells { get; private set; }

    private Vector3 startCellsSpawnerPosition;  //Vector3(-2.75,3.75,0)
    [SerializeField] private Vector2 minGridBounds;
    [SerializeField] private Vector2 maxGridBounds;

    public Vector2 MinGridBounds { get { return minGridBounds; } }
    public Vector2 MaxGridBounds { get { return maxGridBounds; } }

    // Start is called before the first frame update
    void Start()
    {
        startCellsSpawnerPosition = minGridBounds;

        cellIndexesAt = new Dictionary<string, Tuple<int, int>>();
        cells = new Transform[numMaxRows, numMaxCols];
        cellsOccupied = new bool[numMaxRows, numMaxCols];

        DrawGrid();
        GenerateCellPositions();
    }

    public void GenerateCellPositions()
    {
        Vector3 cellPosition = startCellsSpawnerPosition;

        for (int y = 0; y < numMaxRows; y++)
        {
            for (int x = 0; x < numMaxCols; x++)
            {
                cellIndexesAt[cellPosition.ToString()] = new Tuple<int, int>(y, x);
                cellsOccupied[y, x] = false;

                //print(cellPosition);
                cellPosition.x += offsetNextObjectPosition;
            }

            cellPosition.y -= offsetNextObjectPosition;
            cellPosition.x = startCellsSpawnerPosition.x;
        }
    }

    #region collisionsCheck

    public bool IsCurrentBlockCollidingLeft()
    {
        int cellRowIndex;
        int cellColIndex;
        Vector3 cellPosition;

        for (int i = 0; i < BlocksSpawnMngr.Instance.currentBlock.transform.childCount; i++)
        {
            cellPosition = BlocksSpawnMngr.Instance.currentBlock.transform.GetChild(i).position;

            if (cellPosition.x <= minGridBounds.x)
            {
                return true;
            }

            else if (cellPosition.x > maxGridBounds.x)
            {
                return false;
            }

            if (cellPosition.y > minGridBounds.y || cellPosition.y < maxGridBounds.y)
            {
                continue;
            }

            cellRowIndex = cellIndexesAt[cellPosition.ToString()].Item1;
            cellColIndex = cellIndexesAt[cellPosition.ToString()].Item2;

            if (cellColIndex - 1 < 0)
            {
                return true;
            }

            if (cellsOccupied[cellRowIndex, cellColIndex - 1])
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCurrentBlockCollidingRight()
    {
        int cellRowIndex;
        int cellColIndex;
        Vector3 cellPosition;

        for (int i = 0; i < BlocksSpawnMngr.Instance.currentBlock.transform.childCount; i++)
        {
            cellPosition = BlocksSpawnMngr.Instance.currentBlock.transform.GetChild(i).position;

            if (cellPosition.x >= maxGridBounds.x)
            {
                return true;
            }

            else if (cellPosition.x < minGridBounds.x)
            {
                return false;
            }

            if (cellPosition.y > minGridBounds.y || cellPosition.y < maxGridBounds.y)
            {
                continue;
            }

            cellRowIndex = cellIndexesAt[cellPosition.ToString()].Item1;
            cellColIndex = cellIndexesAt[cellPosition.ToString()].Item2;

            if (cellColIndex + 1 >= cellsOccupied.GetLength(1))
            {
                return true;
            }

            if (cellsOccupied[cellRowIndex, cellColIndex + 1])
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCurrentBlockCollidingDown()
    {
        int cellRowIndex;
        int cellColIndex;
        Vector3 cellPosition;

        for (int i = 0; i < BlocksSpawnMngr.Instance.currentBlock.transform.childCount; i++)
        {
            cellPosition = BlocksSpawnMngr.Instance.currentBlock.transform.GetChild(i).position;

            if (cellPosition.y <= maxGridBounds.y)
            {
                return true;
            }

            else if (cellPosition.y > minGridBounds.y)
            {
                return false;
            }

            if (cellPosition.x < minGridBounds.x || cellPosition.x > maxGridBounds.x)
            {
                continue;
            }

            cellRowIndex = cellIndexesAt[cellPosition.ToString()].Item1;
            cellColIndex = cellIndexesAt[cellPosition.ToString()].Item2;

            if (cellRowIndex + 1 >= cellsOccupied.GetLength(0))
            {
                return true;
            }

            if (cellsOccupied[cellRowIndex + 1, cellColIndex])
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    //we check, before placing a block, if it's overlapping with occupied cells
    //in case of left/right movement at last, near to cells
    public bool IsCurrentBlockOverlappingOccuppiedCells()
    {
        try
        {
            int cellRowIndex;
            int cellColIndex;
            Vector3 cellPosition;

            for (int i = 0; i < BlocksSpawnMngr.Instance.currentBlock.transform.childCount; i++)
            {
                cellPosition = BlocksSpawnMngr.Instance.currentBlock.transform.GetChild(i).position;
                cellRowIndex = cellIndexesAt[cellPosition.ToString()].Item1;
                cellColIndex = cellIndexesAt[cellPosition.ToString()].Item2;

                if (cellsOccupied[cellRowIndex, cellColIndex])
                {
                    BlocksSpawnMngr.Instance.currentBlockController.moveSpeed = 0;

                    return true;
                }
            }

            return false;
        }

        catch
        {
            return false;
        }
    }

    public List<int> GetCellsRowIndexesToDelete()
    {
        List<int> rowIndexes = new List<int>();
        bool deleteRow;

        for (int y = 0; y < numMaxRows; y++)
        {
            deleteRow = true;

            for (int x = 0; x < numMaxCols; x++)
            {
                //if there's at least one cell in the current row as not occupied,
                //it'll set deleteRow as false and skip the columns for
                if (!cellsOccupied[y, x])
                {
                    deleteRow = false;
                    break;
                }
            }

            if (deleteRow)
            {
                rowIndexes.Add(y);
            }
        }

        return rowIndexes;
    }

    public void DeleteTetrisedRows()
    {
        List<int> rowsToDeleteIndexes = GetCellsRowIndexesToDelete();

        for (int y = 0; y < rowsToDeleteIndexes.Count; y++)
        {
            for (int x = 0; x < numMaxCols; x++)
            {
                Destroy(cells[rowsToDeleteIndexes[y], x].gameObject);
                cellsOccupied[rowsToDeleteIndexes[y], x] = false;
            }
        }

        if (rowsToDeleteIndexes.Count == 0)
        {
            return;
        }

        //shifts all cells up to first deleted rows to down
        int firstRowToDeleteIndex = rowsToDeleteIndexes[0];
        float nextCellPosition = 0.5f * rowsToDeleteIndexes.Count;
        int nextCellRowIndex;

        for (int y = firstRowToDeleteIndex - 1; y >= 0; y--)
        {
            for (int x = 0; x < numMaxCols; x++)
            {
                if (cellsOccupied[y, x])
                {
                    cellsOccupied[y, x] = false;

                    cells[y, x].position -= new Vector3(0, nextCellPosition, 0);

                    nextCellRowIndex = cellIndexesAt[cells[y, x].position.ToString()].Item1;

                    cells[nextCellRowIndex, x] = cells[y, x];
                    cellsOccupied[nextCellRowIndex, x] = true;

                    cells[y, x] = null;
                }
            }
        }
    }

    public void SetGridOccupiedCells(Transform[] cells, bool value = true)
    {
        int cellRow;
        int cellCol;

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].position.y > minGridBounds.y)  //if a block collided with the up frame (game over)
            {
                Time.timeScale = 0;
                return;
            }

            cellRow = cellIndexesAt[cells[i].position.ToString()].Item1;
            cellCol = cellIndexesAt[cells[i].position.ToString()].Item2;

            cellsOccupied[cellRow, cellCol] = value;
            this.cells[cellRow, cellCol] = cells[i];
        }
    }

    private void FixedUpdate()
    {
        if (BlocksSpawnMngr.Instance.currentBlockController == null)
        {
            return;
        }

        if (IsCurrentBlockCollidingDown())
        {
            if (IsCurrentBlockOverlappingOccuppiedCells())
            {
                BlocksSpawnMngr.Instance.currentBlockController.MoveUpOfOne();
            }

            Transform[] cells = BlocksSpawnMngr.Instance.currentBlockController.UnpackBlock(cellsContainer);

            SetGridOccupiedCells(cells);
            Destroy(BlocksSpawnMngr.Instance.currentBlock);

            DeleteTetrisedRows();

            BlocksSpawnMngr.Instance.SpawnBlock();
        }
    }

    public void DrawGrid()
    {
        Vector3 gridDrawCellPosition = startCellsSpawnerPosition;

        for (int y = 0; y < numMaxRows; y++)
        {
            for (int x = 0; x < numMaxCols; x++)
            {
                GameObject objectSpawned = Instantiate(gridDrawCell, gridDrawContainer);
                objectSpawned.transform.position = gridDrawCellPosition;

                gridDrawCellPosition.x += offsetNextObjectPosition;
            }

            gridDrawCellPosition.y -= offsetNextObjectPosition;
            gridDrawCellPosition.x = startCellsSpawnerPosition.x;
        }
    }
}
