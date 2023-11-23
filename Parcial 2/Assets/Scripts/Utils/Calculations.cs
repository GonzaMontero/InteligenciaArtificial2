using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class Calculations
    {
        public void ShuffleList<T>(List<T> list)
        {
            var count = list.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                (list[i], list[r]) = (list[r], list[i]);
            }
        }

        public int GetManhattanDistance(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
        }
    }
}

