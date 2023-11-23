using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Food
{
    public abstract class FoodBase : MonoBehaviour
    {
        protected virtual void GetEaten(Agents.Agent agent) { }
    }
}

