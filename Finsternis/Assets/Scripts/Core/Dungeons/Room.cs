﻿using System.Collections.Generic;
using UnityEngine;

public class Room : DungeonSection
{
    private HashSet<Vector2> _cellsMirror;
    private List<Vector2> _cells;

    public List<Vector2> Cells { get { return _cells; } }

    public Room(Vector2 position)
    {
        _bounds = new Rect(position, Vector2.zero);
        _cells = new List<Vector2>();
        _cellsMirror = new HashSet<Vector2>();
    }

    public Room(Room baseRoom)
    {
        _bounds = baseRoom._bounds;
        _cells = new List<Vector2>(baseRoom._cells);
        _cellsMirror = new HashSet<Vector2>(baseRoom._cells);
    }

    public static Room operator +(Room roomA, Room roomB)
    {
        Room mergedRooms = new Room(roomA);
        mergedRooms.Merge(roomB);

        return mergedRooms;
    }

    public void Merge(params Room[] others)
    {
        foreach (Room other in others)
        {
            foreach(Vector2 cell in other._cells)
            {
                if (_cellsMirror.Add(cell))
                    _cells.Add(cell);
            }
            _bounds.min = Vector2.Min(_bounds.min, other._bounds.min);
            _bounds.max = Vector2.Max(_bounds.max, other._bounds.max);
        }
    }

    public bool Overlaps(Room other)
    {
        return other._bounds.Overlaps(other._bounds) && _cellsMirror.Overlaps(other._cellsMirror);
    }

    public void AddCell(float x, float y)
    {
        AddCell(new Vector2(x, y));
    }

    public void AddCell(Vector2 newCell)
    {
        if (_cellsMirror.Add(newCell))
        {
            _cells.Add(newCell);
            AdjustSize(newCell);
        }
    }

    private void AdjustSize(Vector2 newCell)
    {
        _bounds.min = Vector2.Min(_bounds.min, newCell);
        _bounds.max = Vector2.Max(_bounds.max, newCell + Vector2.one);
    }

    public Vector2 GetRandomCell()
    {
        return _cells[Random.Range(0, _cells.Count)];
    }

    public override string ToString()
    {
        System.Text.StringBuilder cells = new System.Text.StringBuilder("Room[bounds: ").Append(Bounds).Append("; cells: ");
        _cells.ForEach(cell => cells.Append(cell).Append("; "));
        return cells.ToString(0, cells.Length - 2);
    }

}