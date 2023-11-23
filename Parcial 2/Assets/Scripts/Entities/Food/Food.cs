using Entities.Agents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Food
{
    public class Food : FoodBase
    {
        [SerializeField] private int bonusFirness = 200;
        public Vector2Int CurrentPosition {  get; set; }

        protected override void GetEaten(Agent agent)
        {
            agent.Eat(bonusFirness);
            Destroy(gameObject);
        }
    }
}
