using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.Gameplay
{

    public static class Movement
    {

        public static MoveDirection GetRandomDirection()
        {
            int randomIndex = Random.Range(0, (int)MoveDirection.Size);
            return (MoveDirection)randomIndex;
        }
        public enum MoveDirection
        {
            Left,
            Right,
            Up,
            Down,
            Size
        }
    }
}

