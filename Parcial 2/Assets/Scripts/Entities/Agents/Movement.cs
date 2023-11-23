using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Agents
{
    public static class Movement
    {
        public static MoveDirection GetRandom()
        {
            int random = Random.Range(0, (int)MoveDirection.Down);
            return (MoveDirection)random;
        }

        public enum MoveDirection
        {
            Left,
            Right,
            Up,
            Down
        }
    }
}

