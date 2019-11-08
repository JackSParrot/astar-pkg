using System;
using System.Collections.Generic;

namespace JackSParrot.Navigation.AStar
{
    public class Pathfinder
    {
        int _cols = 0;
        int _rows = 0;
        int _from = 0;
        int _to = 0;
        int _heightStep = 1;
        BoardSpot[] _map;
        HashSet<int> _open;
        HashSet<int> _closed;
        public List<BoardPoint> Result { get; private set; }

        int XY2Idx(int x, int y)
        {
            return y * _cols + x;
        }

        int CalculateHeuristic(int fromIdx, int targetIDx)
        {
            //As we can only move on the horizontal or vertical axis, I'll use manhattan (scaled to avoid floats)
            int diffX = Math.Abs(_map[targetIDx].position.x - _map[fromIdx].position.x) * 10;
            int diffY = Math.Abs(_map[targetIDx].position.y - _map[fromIdx].position.y) * 10;
            return (diffX == diffY) ? (diffX * 3) / 2 : diffX + diffY;
        }

        public Pathfinder(int cols, int rows, bool diagonalLinks = false)
        {
            _cols = cols;
            _rows = rows;
            _open = new HashSet<int>();
            _closed = new HashSet<int>();
            Result = new List<BoardPoint>();
            _map = new BoardSpot[cols * rows];
            for (int i = 0; i < _map.Length; ++i)
            {
                _map[i].position.Set(i, _cols);
                _map[i].walkable = true;
                _map[i].previousIdx = -1;
                _map[i].cost = 0;
                _map[i].height = 0;
                _map[i].InitNeighbours(_cols, _rows, diagonalLinks);
            }
        }

        public List<BoardPoint> FindPath(int fromX, int fromY, int toX, int toY, int heightStep = 1)
        {
            Reset();
            _open.Clear();
            _closed.Clear();
            _heightStep = heightStep;
            int fromIdx = XY2Idx(fromX, fromY);
            int toIdx = XY2Idx(toX, toY);

            if (!_map[fromIdx].walkable)
            {
                return Result;
            }
            _map[fromIdx].g = 0;
            _open.Add(fromIdx);

            while (_open.Count > 0)
            {
                int currentIdx = -1;
                using (var itr = _open.GetEnumerator())
                {
                    while (itr.MoveNext())
                    {
                        int candidate = itr.Current;
                        if (currentIdx == -1 || _map[candidate].f < _map[currentIdx].f)
                        {
                            currentIdx = candidate;
                        }
                    }
                }

                _open.Remove(currentIdx);
                _closed.Add(currentIdx);

                if (_map[currentIdx].position.index == toIdx)
                {
                    IndexesToPositions(currentIdx, Result);
                    return Result;
                }

                var neighbours = _map[currentIdx].Neighbours;
                for (int n = 0; n < neighbours.Count; ++n)
                {
                    int currentNeighbourIdx = neighbours[n];
                    if (_closed.Contains(currentNeighbourIdx) || !_map[currentNeighbourIdx].walkable || Math.Abs(_map[currentNeighbourIdx].height - _map[currentIdx].height) > _heightStep)
                    {
                        continue;
                    }
                    bool foundNew = false;
                    int potentialG = _map[currentIdx].g + CalculateHeuristic(currentIdx, currentNeighbourIdx);
                    if (_open.Contains(currentNeighbourIdx))
                    {
                        if (_map[currentNeighbourIdx].g > potentialG)
                        {
                            foundNew = true;
                        }
                    }
                    else
                    {
                        _open.Add(currentNeighbourIdx);
                        foundNew = true;
                    }
                    if (foundNew)
                    {
                        _map[currentNeighbourIdx].g = potentialG;
                        _map[currentNeighbourIdx].h = CalculateHeuristic(currentNeighbourIdx, toIdx);
                        _map[currentNeighbourIdx].f = _map[currentNeighbourIdx].g + _map[currentNeighbourIdx].h;
                        _map[currentNeighbourIdx].previousIdx = currentIdx;
                    }
                }
            }
            return Result;
        }

        public void SetObstacle(int x, int y, bool isObstacle)
        {
            int idx = XY2Idx(x, y);
            _map[idx].walkable = !isObstacle;
        }

        public void SetCost(int x, int y, int cost)
        {
            int idx = XY2Idx(x, y);
            _map[idx].cost = cost;
        }

        void IndexesToPositions(int finalIdx, List<BoardPoint> outPositions)
        {
            outPositions.Clear();
            int current = finalIdx;
            while (_map[current].previousIdx >= 0)
            {
                outPositions.Add(_map[current].position);
                current = _map[current].previousIdx;
            }
            outPositions.Reverse();
        }

        public void Init(int fromX, int fromY, int toX, int toY)
        {
            Reset();
            _open.Clear();
            _closed.Clear();
            _from = XY2Idx(fromX, fromY);
            _to = XY2Idx(toX, toY);

            _map[_from].g = 0;
            _open.Add(_from);

        }

        public void Tick()
        {
            if (!_map[_from].walkable || !_map[_to].walkable || Result.Count > 0)
            {
                return;
            }
            if (_open.Count > 0)
            {
                int currentIdx = -1;
                using (var itr = _open.GetEnumerator())
                {
                    while (itr.MoveNext())
                    {
                        int candidate = itr.Current;
                        if (currentIdx == -1 || _map[candidate].f < _map[currentIdx].f)
                        {
                            currentIdx = candidate;
                        }
                    }
                }

                _open.Remove(currentIdx);
                _closed.Add(currentIdx);

                if (_map[currentIdx].position.index == _to)
                {
                    IndexesToPositions(currentIdx, Result);
                    return;
                }

                var neighbours = _map[currentIdx].Neighbours;
                for (int n = 0; n < neighbours.Count; ++n)
                {
                    int currentNeighbourIdx = neighbours[n];
                    if (_closed.Contains(currentNeighbourIdx) || !_map[currentNeighbourIdx].walkable)
                    {
                        continue;
                    }
                    bool foundNew = false;
                    int potentialG = _map[currentIdx].g + CalculateHeuristic(currentIdx, currentNeighbourIdx) + _map[currentIdx].cost;
                    if (_open.Contains(currentNeighbourIdx))
                    {
                        if (_map[currentNeighbourIdx].g > potentialG)
                        {
                            foundNew = true;
                        }
                    }
                    else
                    {
                        foundNew = true;
                        _open.Add(currentNeighbourIdx);
                    }
                    if (foundNew)
                    {
                        _map[currentNeighbourIdx].g = potentialG;
                        _map[currentNeighbourIdx].h = CalculateHeuristic(currentNeighbourIdx, _to);
                        _map[currentNeighbourIdx].f = _map[currentNeighbourIdx].g + _map[currentNeighbourIdx].h;
                        _map[currentNeighbourIdx].previousIdx = currentIdx;
                    }
                }
            }
        }

        void Reset()
        {
            for (int i = 0; i < _map.Length; ++i)
            {
                _map[i].h = 0;
                _map[i].g = 0;
                _map[i].f = 0;
                _map[i].previousIdx = -1;
            }
            _open.Clear();
            _closed.Clear();
            Result.Clear();
            _from = -1;
            _to = -1;
        }

        public void ResetObstacles()
        {
            for (int i = 0; i < _map.Length; ++i)
            {
                _map[i].walkable = true;
            }
        }

        public void ResetCosts()
        {
            for (int i = 0; i < _map.Length; ++i)
            {
                _map[i].cost = 0;
            }
        }

        public void ResetHeight()
        {
            for (int i = 0; i < _map.Length; ++i)
            {
                _map[i].height = 0;
            }
        }
    }
}