﻿namespace Finsternis
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Corridor : DungeonSection
    {
        private Vector2 direction;
        private int length;

        public Vector2 Direction { get { return this.direction; } set { this.direction = value; } }
        public override Rect Bounds
        {
            get { return bounds; }
            set
            {
                bounds = value;
                this.length = Mathf.RoundToInt(Mathf.Max(bounds.size.x, bounds.size.y));
            }
        }

        public int Length
        {
            get { return this.length; }

            set
            {
                if (value > 0)
                {
                    this.length = value;
                    bounds.size = new Vector2(value * this.direction.x + this.direction.y, value * this.direction.y + this.direction.x);
                }
                else
                {
                    this.length = 0;
                    bounds.size = Vector2.zero;
                }
            }
        }

        public Vector2 End
        {
            get { return this.length > 0 ? this[Length - 1] : Position; }
        }

        public Vector2 this[int index]
        {
            get
            {
                if (index < 0 || index >= this.length)
                    throw new System.ArgumentOutOfRangeException("index", "Trying to access a cell with index " + index + " within " + ToString());
                return bounds.position + this.direction * index;
            }
        }

        public static Corridor CreateInstance(Rect bounds, Vector2 direction)
        {
            Corridor c = CreateInstance<Corridor>(bounds);
            c.direction = direction;
            return c;
        }

        public static Corridor CreateInstance(Vector2 position, int length, Vector2 direction)
        {
            return Corridor.CreateInstance(new Rect(position, direction * length + new Vector2(direction.y, direction.x)), direction);
        }

        public override string ToString()
        {
            return "Corridor[bounds:" + Bounds + "; direction:" + Direction + "; length: " + this.length + "]";
        }

        public override IEnumerator<Vector2> GetEnumerator()
        {
            for (int i = 0; i < this.length; i++)
                yield return this[i];
        }

        public void UpdateConnections()
        {
            foreach (DungeonSection connection in connections)
                connection.AddConnection(this);
        }

        public void RemoveLast()
        {
            Length--;
        }

        public void RemoveFirst()
        {
            Rect newBounds = new Rect();
            newBounds.position = Position + this.direction;
            newBounds.size = Size - this.direction;
            Bounds = newBounds;
        }

        public Corridor[] RemoveAt(int index)
        {
            if (index < 0 || index >= Length)
                throw new System.ArgumentOutOfRangeException("index", "Value should be in the range 0 " + Length);

            Corridor[] result = new Corridor[2];

            if (index > 0)
            {
                result[0] = CreateInstance(bounds.position, index, this.direction);
                result[0].SetTheme(this.Theme);
            }

            if (index < this.length - 1)
            {
                result[1] = CreateInstance(this[index + 1], this.length - index - 1, this.direction);
                result[1].SetTheme(this.Theme);
            }

            foreach (DungeonSection connection in connections)
            {
                if (result[0] && connection.ContainsCell(Position - Direction))
                    result[0].AddConnection(connection);
                else if (result[1] && connection.ContainsCell(End + Direction))
                    result[1].AddConnection(connection);

                connection.RemoveConnection(this);
            }
            return result;
        }

        public override bool ContainsCell(Vector2 cell)
        {
            return cell.x >= X && cell.x <= End.x && cell.y >= Y && cell.y <= End.y;
        }

        /// <summary>
        /// Adds a cells to this corridor, but only immediately before the first or after the last.
        /// </summary>
        /// <param name="cell">The cell to be added.</param>
        /// <returns>True if the cell was added.</returns>
        public override bool AddCell(Vector2 cell)
        {
            if (cell == this[0] - Direction)
            {
                Position -= Direction;
                Length++;
            }
            else if (cell == End + Direction)
                Length++;
            else
                return false;

            return true;

        }

        public override void RemoveCell(Vector2 cell)
        {
            if (ContainsCell(cell))
            {
                while (End != cell)
                    Length--;
            }
        }

        internal void AddTrap(Vector2 cell)
        {
            var trap = GetTheme<CorridorTheme>().GetRandomTrap();
            AddFeature(trap, cell);
        }

        internal void AddDoor(Vector2 cell, Vector2 offset)
        {
            var door = Instantiate(GetTheme<CorridorTheme>().GetRandomDoor());
            door.Offset = offset;
            AddFeature(door, cell);
        }

    }
}