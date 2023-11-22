using Entities.Food;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Entities.Agents
{
    public enum MovementPossibilities
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        NONE
    }

    public class AgentBehaviour : MonoBehaviour
    {
        public string foodTag = "Food";

        private FoodHandler foodHandler = null;
        private Action<Vector2Int> onFoodEaten = null;

        public void EatFood(FoodItem foodItem)
        {
            onFoodEaten?.Invoke(foodItem.FoodData.Position);
            foodHandler.EatFood(foodItem.FoodData.Position);

            foodItem.enabled = false;
            foodItem.gameObject.transform.localScale *= 1.05f;
            Destroy(foodItem.gameObject, 0.25f);
        }

        public void SetBehaviourNeeds(Action<Vector2Int> onFoodEaten, FoodHandler foodHandler)
        {
            this.onFoodEaten = onFoodEaten;

            if (this.foodHandler != null)
                return;

            this.foodHandler = foodHandler;
        }

        public void Movement(MoveDirection direction, int limitX, int limitY, Action OnReachLimitY = null, Action OnReachLimitX = null)
        {
            switch (direction)
            {
                case MoveDirection.Left:
                    if(transform.position.x - 1 > 0)
                    {
                        transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
                    }
                    else
                    {
                        OnReachLimitX?.Invoke();
                    }
                    break;
                case MoveDirection.Right:
                    if (transform.position.x + 1 < limitX)
                    {
                        transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
                    }
                    else
                    {
                        OnReachLimitX?.Invoke();
                    }
                    break;
                case MoveDirection.Up:
                    if(transform.position.y + 1 < limitY)
                    {
                        transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                    }
                    else
                    {
                        OnReachLimitY?.Invoke();
                    }
                    break;
                case MoveDirection.Down:
                    if (transform.position.y - 1 > 0)
                    {
                        transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                    }
                    else
                    {
                        OnReachLimitY?.Invoke();
                    }
                    break;
                case MoveDirection.None:
                    transform.position = transform.position;
                    break;
            }
        }
    }
}

