using System.Collections.Generic;
using UnityEngine;

using Utils.Constants;
using Handlers.Map;


namespace Entities.Food
{
    /// <summary>
    /// A class referencing the basic food item which the agents will be searching for into the map
    /// </summary>
    [System.Serializable]
    public class Food
    {
        private Vector2Int position;
        public Vector2Int Position => position;

        /// <summary>
        /// Generates food on a specified position
        /// </summary>
        /// <param name="position"></param>
        public Food(Vector2Int position)
        {
            this.position = position;
        }
    }

    public class FoodHandler : MonoBehaviour
    {
        public MapHandler mapHandler;
        public FoodItem foodItemToInstantiate;
        public Transform foodHolder;

        private List<Food> foodInMap;
        public List<Food> FoodInMap => foodInMap;
        private List<FoodItem> foodItemsInMap;
        private int foodCount = 0;

        public void Init(List<Vector2Int> foodPositions)
        {
            foodCount = foodPositions.Count;

            foodInMap = new List<Food>();
            foodItemsInMap = new List<FoodItem>();

            for(int i = 0; i < foodPositions.Count; i++)
            {
                //If the position is valid
                if (foodPositions[i] != Constants.InvalidPosition)
                {
                    //Create food item on position and instantiate object
                    Food food = new Food(foodPositions[i]);

                    foodInMap.Add(food);

                    FoodItem foodItem = Instantiate(foodItemToInstantiate, new Vector3(food.Position.x, food.Position.y, 0), 
                        Quaternion.identity, foodHolder);

                    foodItem.SetFoodData(food);
                    foodItem.name = "Food " + foodPositions[i].ToString();
                    foodItemsInMap.Add(foodItem);
                }
            }
        }

        public void Unload()
        {
            //Clear lists, destroy GameObjects in map and reset lists to null
            for(int i = 0; i < foodItemsInMap.Count; i++)
            {
                if (foodItemsInMap[i] != null)
                    Destroy(foodItemsInMap[i]);
            }

            foodItemsInMap.Clear();
            foodInMap.Clear();

            foodItemsInMap = null;
            foodInMap = null;
        }

        /// <summary>
        /// This method is called whenever an agent comes over a food area, which removes the food from both lists (the physical GO one
        /// as well as the backend one)
        /// </summary>
        /// <param name="foodPosition"></param>
        public void EatFood(Vector2Int foodPosition)
        {
            for (int i = 0; i < foodItemsInMap.Count; i++)
            {
                if (foodItemsInMap[i] != null)
                {
                    if (foodPosition == new Vector2Int((int)foodItemsInMap[i].transform.position.x, (int)foodItemsInMap[i].transform.position.y))
                    {
                        foodItemsInMap[i].transform.localScale *= 3;
                        foodItemsInMap.Remove(foodItemsInMap[i]);
                        break;
                    }
                }
            }

            Food toRemove = foodInMap.Find(food => food.Position == foodPosition);
            if (foodInMap.Contains(toRemove))
            {
                foodInMap.Remove(toRemove);
            }
        }

        public FoodItem MapContainsFood(Vector2Int foodPos)
        {
            for(int i = 0; i < FoodInMap.Count; i++) 
            {
                if (foodInMap[i] != null && foodInMap[i].Position == foodPos) 
                {
                    return foodItemsInMap[i];
                }
            }

            return null;
        }
    }
}

