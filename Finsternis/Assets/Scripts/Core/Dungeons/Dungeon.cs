﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public abstract class Dungeon : MonoBehaviour
{
    public class Room
    {
        private List<Vector2> _cells;
        private Rect _bounds;

        public List<Vector2> Cells { get { return _cells; } }
        public Rect Bounds { get { return _bounds; } }

        public Room(Vector2 position)
        {
            _bounds = new Rect(position, Vector2.zero);
            _cells = new List<Vector2>();
        }

        private void AdjustSize(Vector2 pos)
        {
            if (pos.x < _bounds.x)
                _bounds.x = pos.x;
            else if (pos.x > _bounds.xMax)
                _bounds.xMax = pos.x;

            if (pos.y < _bounds.y)
                _bounds.y = pos.y;
            else if (pos.y > _bounds.yMax)
                _bounds.yMax = pos.y;
        }

        public void AddCell(float x, float y)
        {
            AddCell(new Vector2(x, y));
        }

        public void AddCell(Vector2 pos)
        {
            _cells.Add(pos);
            AdjustSize(pos);
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

    public class Corridor
    {
        private Rect _bounds;
        private Vector2 _direction;
        private int _length;

        public Vector2 Direction { get { return _direction; } set { _direction = value; } }
        public Rect Bounds {
            get { return _bounds; }
            set {
                _bounds = value;
                _length = Mathf.RoundToInt(Mathf.Max(_bounds.size.x, _bounds.size.y));
            }
        }

        public int Length
        {
            get { return _length; }

            set {
                if (value > 0)
                {
                    _length = value;
                    _bounds.size = new Vector2(value * _direction.x + _direction.y, value * _direction.y + _direction.x);
                }
                else
                {
                    _length = 0;
                    _bounds.size = Vector2.zero;
                }
            }
        }

        public Vector2 LastCell
        {
            get
            {

                return _length > 0 ? _bounds.max - Vector2.one : _bounds.position;
            }
        }

        public Vector2 this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new System.ArgumentOutOfRangeException("index", "Trying to access a cell with index " + index + " within " + ToString());
                return _bounds.position + _direction*index;
            }
        }

        public Corridor(Rect bounds, Vector2 direction)
        {
            _direction = direction;
            Bounds = bounds;
        }

        public Corridor(Vector2 position, int length, Vector2 direction)
        {
            _direction = direction;
            Bounds = new Rect(position, direction * length + new Vector2(direction.y, direction.x));
        }

        public override string ToString()
        {
            return "Corridor[bounds:" + Bounds + "; direction:" + Direction + "; length: "+_length +"]";
        }

        public void RemoveLast()
        {
            Length--;
        }

        public void RemoveFirst()
        {
            Rect newBounds = new Rect(_bounds);
            newBounds.position += _direction;
            Bounds = newBounds;
        }

        public Corridor[] RemoveAt(int index)
        {
            Corridor[] result = new Corridor[2];

            if(index > 0)
                result[0] = new Corridor(_bounds.position, index, _direction);

            if(index < _length - 1)
                result[1] = new Corridor(this[index+1], _length - index - 1, _direction);

            return result;

        }
    }


    public UnityEvent onGenerationBegin;
    public UnityEvent onGenerationEnd;

    [SerializeField]
    private int _seed;

    public bool customSeed = true;

    [SerializeField]
    protected Vector2 entrance;

    [SerializeField]
    protected Vector2 exit;

    public int Seed
    {
        get { return _seed; }
        set
        {
            if (customSeed)
            {
                Random.seed = this._seed;
                _seed = value;
            }
        }
    }

    public Vector2 Entrance { get { return entrance; } }
    public Vector2 Exit { get { return exit; } }

    public virtual void Awake()
    {
        if (customSeed)
        {
            Random.seed = this._seed;
        }
    }

    public virtual void Generate()
    {
        onGenerationBegin.Invoke();
    }
}
