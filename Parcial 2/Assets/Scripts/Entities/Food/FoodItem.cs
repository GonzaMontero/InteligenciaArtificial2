using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities.Food;

public class FoodItem : MonoBehaviour
{
    private Food foodData;
    public Food FoodData => foodData;

    public void SetFoodData(Food foodData)
    {
        this.foodData = foodData;
    }

    private void OnDestroy()
    {
        Debug.Log("Destroyed Food");
    }
}
