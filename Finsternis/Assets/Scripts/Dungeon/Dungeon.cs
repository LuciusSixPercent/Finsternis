﻿namespace Finsternis
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityQuery;

    public class Dungeon : CustomBehaviour
    {

        public const Type wall = null;

        public delegate void DungeonCleared();

        public UnityEvent onDungeonCleared;

        [SerializeField]
        private int seed;

        [SerializeField]
        private bool customSeed = true;

        [SerializeField]
        private Vector2 entrance;

        [SerializeField]
        private Vector2 exit;

        [SerializeField]
        private List<DungeonGoal> goals;

        private HashSet<RoomTheme> roomThemes;
        private HashSet<CorridorTheme> corridorThemes;

        private List<Room> rooms;
        private List<Corridor> corridors;
        private DungeonSection[,] dungeonGrid;

        private Vector2 bottomRightCorner;

        private int remainingGoals;

        public int RemainingGoals { get { return this.remainingGoals; } }

        public int Seed
        {
            get { return this.seed; }
            set
            {
                if (customSeed)
                {
                    UnityEngine.Random.InitState(value);
                    this.seed = value;
                }
            }
        }

        public Vector2 Entrance { get { return entrance; } set { entrance = value; } }
        public Vector2 Exit { get { return exit; } set { exit = value; } }

        public int Width { get { return this.dungeonGrid.GetLength(0); } }
        public int Height { get { return this.dungeonGrid.GetLength(1); } }
        public Vector2 Size { get { return new Vector2(Width, Height); } }

        public DungeonSection this[int x, int y]
        {
            get
            {
                try
                {
                    return this.dungeonGrid[x, y];
                }
                catch (IndexOutOfRangeException ex)
                {
                    return TreatAccessException(x, y, ex);
                }
            }
            set
            {
                try
                {
                    this.dungeonGrid[x, y] = value;
                }
                catch (IndexOutOfRangeException ex)
                {
                    TreatAccessException(x, y, ex);
                }
            }
        }

        private DungeonSection TreatAccessException(int x, int y, IndexOutOfRangeException ex)
        {
#if DEBUG
            throw new IndexOutOfRangeException("Attempting to access a cell outside of dungeon! [" + x + ";" + y + "]", ex);
#else
            Log.E(this, "Attempting to access a cell outside of dungeon! [{0};{1}]\n{2}", x, y, ex.StackTrace);
            return null;
#endif
        }

        public DungeonSection this[float x, float y]
        {
            get { return this[(int)x, (int)y]; }
            set { this[(int)x, (int)y] = value; }
        }

        public DungeonSection this[Vector2 pos]
        {
            get { return this[(int)pos.x, (int)pos.y]; }
            set { this[(int)pos.x, (int)pos.y] = value; }
        }

        public List<Corridor> Corridors { get { return corridors; } }
        public List<Room> Rooms { get { return rooms; } }

        public void FitGridToSections()
        {
            bottomRightCorner = Vector2.zero;
            for (int col = 0; col < this.Width; col++)
            {
                for (int row = 0; row < this.Height; row++)
                {
                    if (this[col, row])
                    {
                        if (row > bottomRightCorner.y)
                            bottomRightCorner.y = row;
                        if (col > bottomRightCorner.x)
                            bottomRightCorner.x = col;
                    }
                }
            }

            var newGrid = new DungeonSection[(int)bottomRightCorner.x + 1, (int)bottomRightCorner.y + 1];
            for (int col = 0; col < newGrid.GetLength(0); col++)
            {
                for (int row = 0; row < newGrid.GetLength(1); row++)
                {
                    newGrid[col, row] = this[col, row];
                }
            }

            this.dungeonGrid = newGrid;
        }

        public Vector2 GetCenter()
        {
            return new Vector2(this.Width * .5f, this.Height * .5f);
        }

        public void Init(int width, int height)
        {
            if (customSeed)
                UnityEngine.Random.InitState(this.seed);

            this.dungeonGrid = new DungeonSection[width, height];
            this.corridors = new List<Corridor>();
            this.rooms = new List<Room>();
            this.goals = new List<DungeonGoal>();
            this.roomThemes = new HashSet<RoomTheme>();
            this.corridorThemes = new HashSet<CorridorTheme>();

            if (onDungeonCleared == null)
                onDungeonCleared = new UnityEvent();
        }

        public void AddCorridorTheme(CorridorTheme t)
        {
            corridorThemes.Add(t);
        }

        public void AddRoomTheme(RoomTheme t)
        {
            roomThemes.Add(t);
        }

        public CorridorTheme GetRandomCorridorTheme()
        {
            return this.corridorThemes.GetRandom(UnityEngine.Random.Range);
        }

        public RoomTheme GetRandomRoomTheme()
        {
            return this.roomThemes.GetRandom(UnityEngine.Random.Range);
        }

        public T GetGoal<T>() where T : DungeonGoal
        {
            foreach (DungeonGoal goal in this.goals)
            {
                if (goal is T)
                    return (T)goal;
            }
            return null;
        }

        public T[] GetGoals<T>() where T : DungeonGoal
        {
            List<T> goals = new List<T>();
            this.goals.ForEach((goal) => { if (goal is T) goals.Add(goal as T); });
            return goals.ToArray();
        }

        public T AddGoal<T>() where T : DungeonGoal
        {
            T goal = gameObject.AddComponent<T>();
            this.goals.Add(goal);
            goal.onGoalReached.AddListener(
                (g) =>
                {
                    remainingGoals--;
                    onDungeonCleared.Invoke();
                });

            remainingGoals++;
            return goal;
        }

        public Room GetRandomRoom(int min = 0, int max = -1)
        {
            return this.rooms.GetRandom(UnityEngine.Random.Range, min, max);
        }

        /// <summary>
        /// Checks if a given set of coordinates is within the dungeon bounds.
        /// </summary>
        /// <param name="cell">The coordinates to be checked.</param>
        /// <returns>True if the given cell is within the dungeon bounds.</returns>
        public bool IsWithinDungeon(Vector2 cell)
        {
            return IsWithinDungeon(cell.x, cell.y);
        }

        /// <summary>
        /// Checks if a given set of coordinates is within the dungeon bounds.
        /// </summary>
        /// <param name="x">The column to check.</param>
        /// <param name="y">The row to check.</param>
        /// <returns>True if both X and Y are within the dungeon bounds.</returns>
        public bool IsWithinDungeon(float x, float y)
        {
            return x >= 0
                && x < Width
                && y >= 0
                && y < Height;
        }

        /// <summary>
        /// Searches a given area for the given cell types;
        /// </summary>
        /// <param name="pos">The area starting point.</param>
        /// <param name="size">The width and height of the search area.</param>
        /// <param name="types">The types of cell that are being searched.</param>
        /// <returns>True if any of the types was found</returns>
        public bool SearchInArea(Vector2 pos, Vector2 size, params Type[] types)
        {
            if (types == null || types.Length < 1)
                throw new InvalidOperationException("Must provide at least one type to be searched for.");

            for (int row = (int)pos.y; row < Height && row < pos.y + size.y; row++)
            {
                for (int col = (int)pos.x; col < Width && col < pos.x + size.x; col++)
                {
                    Vector2 cell = new Vector2(col, row);
                    if (IsWithinDungeon(cell) && IsOfAnyType(cell, types))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Searches around a given coordinate for the provided tyes.
        /// </summary>
        /// <param name="pos">The coordinate whose surroundings will be checked.</param>
        /// <param name="requiredNumberOfMatches">How many matches should happen before stopping the check?</param>
        /// <param name="checkDiagonals">Should diagonals be chekced? (ie. [x-1, y-1])</param>
        /// <param name="types">The types to be considered.</param>
        /// <returns>How many cells of the provided types were found.</returns>
        public int SearchAround(Vector2 pos, int requiredNumberOfMatches, bool checkDiagonals, params Type[] types)
        {
            if (types == null || types.Length < 1)
                throw new InvalidOperationException("Must provide at least one type to be searched for.");

            int cellsFound = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == j || (!checkDiagonals && Mathf.Abs(i) == Mathf.Abs(j)))
                        continue;

                    Vector2 neighbourCell = pos + new Vector2(i, j);
                    if (IsWithinDungeon(neighbourCell) && IsOfAnyType(neighbourCell, types))
                    {
                        cellsFound++;
                    }
                    if (requiredNumberOfMatches > 0 && cellsFound == requiredNumberOfMatches)
                        return cellsFound;
                }
            }
            return cellsFound;
        }

        public List<Vector2> GetNeighbours(Vector2 center, bool checkDiagonals, params Type[] type)
        {
            List<Vector2> neighbours = new List<Vector2>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == j || (!checkDiagonals && i + j == 0))
                        continue;

                    Vector2 neighbourCell = center + new Vector2(i, j);
                    if (type.Length == 0 || IsOfAnyType(neighbourCell, type))
                    {
                        neighbours.Add(neighbourCell);
                    }
                }
            }
            return neighbours;
        }

        /// <summary>
        /// Checks if a given rectangle overlaps a corridor
        /// </summary>
        /// <param name="pos">Upper left corner of the rectangle.</param>
        /// <param name="size">Dimenstions of the rectangle.</param>
        /// <returns>True if it does overlap.</returns>
        internal bool OverlapsCorridor(Vector2 pos, Vector2 size)
        {
            Rect r = new Rect(pos, size);

            foreach (Corridor c in corridors)
            {
                if (c.Bounds.Overlaps(r))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if there is an instance of any of the provided types at a given coordinate.
        /// </summary>
        /// <param name="cell">The coordinates to be checked.</param>
        /// <param name="types">The types to be considered.</param>
        /// <returns>True if there is a match between one of the provided types and the object at the provided coordinates.</returns>
        public bool IsOfAnyType(Vector2 cell, params Type[] types)
        {
            foreach (Type type in types)
            {
                if (IsOfType(cell, type))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if there is an instance of the provided type at a given coordinate.
        /// </summary>
        /// <param name="cell">Position be checked.</param>
        /// <param name="type">The type to be considered.</param>
        /// <returns>True if there is a match between the provided type and the type of the object at the provided coordinates.</returns>
        public bool IsOfType(Vector2 cell, Type type)
        {
            if (IsWithinDungeon(cell) && this[cell])
            {
                if (this[cell].GetType().Equals(type))
                    return true;
            }
            else if (type == Dungeon.wall)
                return true;

            return false;
        }

        /// <summary>
        /// Searches for unmarked cells around a given point in the dungeon.
        /// </summary>
        /// <param name="cell">The center point for the search.</param>
        /// <param name="ignoreDiagonal">If the diagonals should not be checked.</param>
        /// <returns>True if there is at least one unmarked cell around the given point.</returns>
        private bool IsEdgeCell(Vector2 cell, bool ignoreDiagonal = true)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if ((ignoreDiagonal && Mathf.Abs(i) == Mathf.Abs(j)) || (i == 0 && i == j)) //ignore diagonals (if requested) and the cell itself (when both i and j are 0)
                        continue;

                    int y = (int)(cell.y + i);
                    int x = (int)(cell.x + j);

                    if (IsWithinDungeon(x, y)
                        && this[x, y] == null)
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        public List<DungeonFeature> GetFeaturesAt(Vector2 cell)
        {
            List<DungeonFeature> features = null;

            if (IsWithinDungeon(cell) && this[cell])
                features = this[cell].GetFeaturesAt(cell);

            return features;
        }

        public void MarkCells(DungeonSection section, DungeonSection[,] grid = null)
        {
            if (grid == null)
                grid = this.dungeonGrid;
            foreach (Vector2 cell in section)
            {
                this.dungeonGrid[(int)cell.x, (int)cell.y] = section;
            }
        }

        public override string ToString()
        {
            string s = this.name;

            for (int i = -1; i < this.Height; i++)
            {
                s += "\n|";
                for (int j = -1; j < this.Width; j++)
                {
                    if (j >= 0 && i >= 0)
                    {
                        var cell = this[j, i];
                        if (!cell)
                            s += " 00 |";
                        else if (cell is Corridor)
                            s += " 11 |";
                        else
                            s += " 22 |";
                    }
                    else if (j < 0 && i < 0)
                    {
                        s += " - - |";
                    }
                    else if (j >= 0 && i < 0)
                    {
                        s += " " + j.ToString("D2") + " |";
                    }
                    else if (j < 0 && i >= 0)
                    {
                        s += " " + i.ToString("D2") + " |";
                    }
                }
            }

            return s;
        }
    }
}