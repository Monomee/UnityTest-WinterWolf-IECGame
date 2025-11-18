using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tray 
{
    private int traySize;

    private Cell[] m_cells;

    private Transform m_root;

    private int m_matchMin;

    public Tray(Transform root, GameSettings gameSettings)
    {
        Transform newRoot = root;
        newRoot.localPosition = new Vector3(newRoot.localPosition.x - 3, newRoot.localPosition.y);
        m_root = root;
        traySize = gameSettings.TraySize;
        m_matchMin = gameSettings.MatchesMin;
        m_cells = new Cell[traySize];

        CreateTray();
    }

    void CreateTray()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int i = 0; i < traySize; i++)
        {
            GameObject cellGO = GameObject.Instantiate(prefabBG, m_root);
            cellGO.name = "TrayCell_" + i;
            Cell cell = cellGO.AddComponent<Cell>();
            cell.Setup(i, 0); 
            m_cells[i] = cell;
            cellGO.transform.localPosition = new Vector3(i * 1.5f, -4, 0); // each cell is spaced by 1.5 units
        }
    }

    public int AddItemToTray(Item item)
    {
        if (item == null) return -1;

        int idx = Array.FindIndex(m_cells, c => c.IsEmpty);
        if (idx == -1) return -1;

        Cell targetCell = m_cells[idx];
        if (targetCell == null) return -1;

        targetCell.Free();
        targetCell.Assign(item);

        // move the existing view from board into the tray
        item.SetViewRoot(m_root);
        if (item.View != null)
        {
            item.View.DOKill(); 
            item.View.position = targetCell.transform.position;
        }
        else
        {
            targetCell.ApplyItemPosition(false);
        }

        int finalIndex = SwapItem(idx);

        return finalIndex;
    }
    public bool IsTrayFull()
    {
        return m_cells.All(c => !c.IsEmpty);
    }
    public int SwapItem(int index)
    {
        if (index < 0 || index >= traySize) return index;

        Cell srcCell = m_cells[index];
        if (srcCell == null || srcCell.IsEmpty) return index;

        Item movingItem = srcCell.Item;
        srcCell.Free();

        //find same type
        List<int> sameTypeIndices = new List<int>();
        for (int i = 0; i < traySize; i++)
            if (!m_cells[i].IsEmpty && m_cells[i].Item.IsSameType(movingItem))
                sameTypeIndices.Add(i);

        if (sameTypeIndices.Count == 0)
        {
            m_cells[index].Assign(movingItem);
            movingItem.View.DOMove(m_cells[index].transform.position, 0.18f);
            return index;
        }

        int left = sameTypeIndices.Min();
        int right = sameTypeIndices.Max();

        int target;
        if (index < left) target = left - 1;
        else if (index > right) target = right + 1;
        else target = right + 1;

        target = Mathf.Clamp(target, 0, traySize - 1);

        // shift items
        if (target < index)
        {
            for (int pos = index - 1; pos >= target; pos--)
            {
                Item from = m_cells[pos].Item;

                m_cells[pos].Free();
                m_cells[pos + 1].Free();

                if (from != null)
                {
                    m_cells[pos + 1].Assign(from);
                    from.View.DOMove(m_cells[pos + 1].transform.position, 0.18f);
                }
            }
        }
        else if (target > index)
        {
            for (int pos = index + 1; pos <= target; pos++)
            {
                Item from = m_cells[pos].Item;

                m_cells[pos].Free();
                m_cells[pos - 1].Free();

                if (from != null)
                {
                    m_cells[pos - 1].Assign(from);
                    from.View.DOMove(m_cells[pos - 1].transform.position, 0.18f);
                }
            }
        }

        // move target
        m_cells[target].Free();
        m_cells[target].Assign(movingItem);
        movingItem.View.DOMove(m_cells[target].transform.position, 0.18f);

        return target;
    }

    public List<List<Cell>> GetContiguousMatches()
    {
        List<List<Cell>> result = new List<List<Cell>>();

        int i = 0;
        while (i < traySize)
        {
            if (m_cells[i] == null || m_cells[i].IsEmpty)
            {
                i++;
                continue;
            }

            List<Cell> group = new List<Cell> { m_cells[i] };
            int j = i + 1;
            while (j < traySize && !m_cells[j].IsEmpty && m_cells[j].IsSameType(m_cells[i]))
            {
                group.Add(m_cells[j]);
                j++;
            }

            if (group.Count >= m_matchMin)
            {
                result.Add(group);
            }

            i = j;
        }

        return result;
    }
    public void ClearCells(IEnumerable<Cell> cells)
    {
        foreach (var cell in cells)
        {
            if (cell == null) continue;
            cell.ExplodeItem();
            cell.Free();
        }
    }
    public void ClearTray()
    {
        if (m_cells == null) return;

        for (int i = 0; i < m_cells.Length; i++)
        {
            if (m_cells[i] == null) continue;
            m_cells[i].Clear();
        }
    }
    public void ShiftTrayItemsLeft()
    {
        int dst = 0;
        for (int src = 0; src < traySize; src++)
        {
            if (m_cells[src] == null) continue;
            if (m_cells[src].IsEmpty) continue;

            if (src != dst)
            {
                Item item = m_cells[src].Item;
                m_cells[src].Free();

                m_cells[dst].Free();
                m_cells[dst].Assign(item);
                if (item != null && item.View != null)
                {
                    item.View.DOMove(m_cells[dst].transform.position, 0.18f);
                }
            }

            dst++;
        }

        // clear trailing slots
        for (int k = dst; k < traySize; k++)
        {
            if (m_cells[k] != null)
            {
                m_cells[k].Free();
            }
        }
    }
}
