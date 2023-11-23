using UnityEngine;

namespace IA.Gameplay
{
    public class Food : MonoBehaviour, IFood
    {

        [SerializeField] private int bonusFitness = 200;
        
        public Vector2Int CurrentPosition { get; set; }
        public void GetEaten(Agent agent)
        {
            agent.Eat(bonusFitness);
            Destroy(gameObject);
        }
    }
    
}

