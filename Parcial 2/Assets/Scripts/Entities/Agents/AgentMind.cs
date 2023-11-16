using Entities.Food;
using Handlers.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Entities.Agents
{
    [System.Serializable]
    public class AgentData
    {
        public Genome genome;
        public NeuralNetwork neuralNetwork;
        public int generation;
    }

    public enum State
    {
        Alive,
        Dead
    }

    public class AgentMind : MonoBehaviour
    {
        private Vector3 initialAgentPosition;
        private Vector3 lastAgentPosition;
        private int foodCollected;
        private MapHandler map;

        protected Genome genome;
        public Genome Genome => genome;

        protected NeuralNetwork neuralNetwork;
        public NeuralNetwork NeuralNetwork => neuralNetwork;

        protected AgentBehaviour agentBehaviour;
        public AgentBehaviour AgentBehaviour => agentBehaviour;

        protected int currentTurn = 0;
        public int CurrentTurn => currentTurn;

        protected int foodEaten = 0;
        public int FoodEaten => foodEaten;

        protected int currentIteration = 0;
        public int CurrentIteration => currentIteration;

        protected AgentData agentData;
        public AgentData AgentData => agentData;
        private int generationsSurvived = 0;

        public State state
        {
            get; private set;
        }

        private void Awake()
        {
            agentBehaviour = GetComponent<AgentBehaviour>();
            lastAgentPosition = default;
            generationsSurvived = 0;
        }

        public void SetInitialPosition(Vector3 initialPosition)
        {
            initialAgentPosition = initialPosition;
        }

        public void SetBrain(Genome genome, NeuralNetwork neuralNetwork, bool resetAI = true)
        {
            this.genome = genome;
            this.neuralNetwork = neuralNetwork;
            state = State.Alive;

            agentData = new AgentData();

            agentData.genome = genome;
            agentData.neuralNetwork = neuralNetwork;

            lastAgentPosition = agentBehaviour.transform.position;

            if (resetAI)
            {
                OnReset();
            }
        }

        public void Think(float dt, int turn, int iteration, MapHandler map, FoodHandler food)
        {
            if(state == State.Alive)
            {
                currentTurn = turn;
                currentIteration = iteration;

                this.map = map;

                agentBehaviour.SetBehaviourNeeds(OnAteFood, food);

                OnThink(dt, map, food);
            }
        }

        public virtual void OnGenerationEnded(out Genome genome)
        {
            genome = null;

            if(foodCollected < 1)
            {
                state = State.Dead;
                return;
            }
            else
            {
                state = State.Alive;
            }

            genome = this.genome;
        }

        protected virtual void OnThink(float dt, MapHandler map, FoodHandler food)
        {
            List<Cell> adjacentCellsToAgent = map.GetAdjacentCellsToPositions(
                new Vector2Int((int) agentBehaviour.transform.position.x, (int)agentBehaviour.transform.position.y)
                );

            List<float> inputs = new List<float>();
            float[] outputs;

            lastAgentPosition = agentBehaviour.transform.position;

            inputs.Add(foodCollected);
            inputs.Add(agentBehaviour.transform.position.magnitude);
            inputs.Add(FindClosestFood(food));

            if (adjacentCellsToAgent.Any())
            {
                for(int i = 0;i < adjacentCellsToAgent.Count;i++)
                {
                    inputs.Add(Vector3.Distance(agentBehaviour.transform.position,
                        new Vector3(adjacentCellsToAgent[i].Position.x, adjacentCellsToAgent[i].Position.y,
                        agentBehaviour.transform.position.z)));
                }
            }

            outputs = neuralNetwork.Synapsis(inputs.ToArray());

            for(int i = 0; i < outputs.Length; i++)
            {
                if (outputs[i] <1.0f && outputs[i] > 0.75f)
                {
                    agentBehaviour.Movement(UnityEngine.EventSystems.MoveDirection.Up, map.gridSize.x, map.gridSize.y);
                    genome.fitness += 0.75f;
                }
                if (outputs[i] < 0.75f && outputs[i] > 0.50f)
                {
                    agentBehaviour.Movement(UnityEngine.EventSystems.MoveDirection.Down, map.gridSize.x, map.gridSize.y);
                    genome.fitness += 0.75f;
                }
                if (outputs[i] < 0.50f && outputs[i] > 0.25f)
                {
                    agentBehaviour.Movement(UnityEngine.EventSystems.MoveDirection.Right, map.gridSize.x, map.gridSize.y);
                    genome.fitness += 0.75f;
                }
                if (outputs[i] < 0.25f && outputs[i] > 0.00f)
                {
                    agentBehaviour.Movement(UnityEngine.EventSystems.MoveDirection.Left, map.gridSize.x, map.gridSize.y);
                    genome.fitness += 0.75f;
                }
                if (outputs[i] < 0)
                {
                    agentBehaviour.Movement(UnityEngine.EventSystems.MoveDirection.None, map.gridSize.x, map.gridSize.y);
                    genome.fitness += 0.50f;
                }
            }

            if(foodCollected > 4)
            {
                genome.fitness -= 1;
            }
        }

        protected virtual void OnDead()
        {
            generationsSurvived = 0;
        }

        protected virtual void OnReset() 
        {
            genome.fitness = 0.0f;
            foodCollected = 0;
            agentBehaviour.transform.position = initialAgentPosition;
        }

        private void OnAteFood(Vector2Int foodPosition)
        {
            genome.fitness = genome.fitness > 0 ? genome.fitness * 2 : 100;
            foodCollected++;
            //genome.foodEated = foodCollected;

            if (map.Map.ContainsKey(foodPosition))
            {
                map.Map[foodPosition].SetFoodInCell(null);
            }
        }

        private float FindClosestFood(FoodHandler food)
        {
            if (food == null || food.FoodInMap == null || food.FoodInMap.Count < 1)
                return 0f;

            float closestFood = Vector3.Distance(agentBehaviour.transform.position, 
                new Vector3(food.FoodInMap[0].Position.x, food.FoodInMap[0].Position.y, agentBehaviour.transform.position.z));

            for (int i = 0; i < food.FoodInMap.Count; i++)
            {
                if (food.FoodInMap[i] != null)
                {
                    float newDistance = Vector3.Distance(agentBehaviour.transform.position, 
                        new Vector3(food.FoodInMap[i].Position.x, food.FoodInMap[i].Position.y, agentBehaviour.transform.position.z));

                    if (closestFood < newDistance)
                    {
                        closestFood = newDistance;
                    }
                }
            }

            return closestFood;
        }
    }
}